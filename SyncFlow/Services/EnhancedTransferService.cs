using Microsoft.Extensions.Logging;
using SyncFlow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SyncFlow.Services
{
    public class EnhancedTransferService : IEnhancedTransferService
    {
        private readonly IFileOperations _fileOperations;
        private readonly IFileInventoryService _fileInventoryService;
        private readonly IStorageValidationService _storageValidationService;
        private readonly ILogger<EnhancedTransferService> _logger;
        private readonly SemaphoreSlim _progressThrottle = new(1, 1);
        private DateTime _lastProgressUpdate = DateTime.MinValue;
        private const int ProgressThrottleMs = 100;

        public EnhancedTransferService(
            IFileOperations fileOperations,
            IFileInventoryService fileInventoryService,
            IStorageValidationService storageValidationService,
            ILogger<EnhancedTransferService> logger)
        {
            _fileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
            _fileInventoryService = fileInventoryService ?? throw new ArgumentNullException(nameof(fileInventoryService));
            _storageValidationService = storageValidationService ?? throw new ArgumentNullException(nameof(storageValidationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EnhancedTransferResult> TransferWithValidationAsync(TransferProfile profile, IProgress<EnhancedTransferProgress> progress)
        {
            return await TransferWithValidationAsync(profile, progress, CancellationToken.None);
        }

        public async Task<EnhancedTransferResult> TransferWithValidationAsync(TransferProfile profile, IProgress<EnhancedTransferProgress> progress, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new EnhancedTransferResult
            {
                StartTime = DateTime.Now
            };

            var enhancedProgress = new EnhancedTransferProgress
            {
                StartTime = DateTime.Now
            };

            try
            {
                _logger.LogInformation("Starting enhanced transfer for profile: {ProfileName}", profile.Name);

                // Step 1: Build file inventory
                await ReportProgressThrottled(progress, enhancedProgress, "Building file inventory...");
                
                var inventoryTasks = profile.FolderMappings.Select(async mapping =>
                {
                    try
                    {
                        return await _fileInventoryService.BuildFileInventoryAsync(mapping.SourcePath, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to build inventory for {SourcePath}", mapping.SourcePath);
                        return new FileInventoryResult();
                    }
                });

                var inventories = await Task.WhenAll(inventoryTasks);
                
                enhancedProgress.TotalFiles = inventories.Sum(i => i.TotalFileCount);
                enhancedProgress.TotalBytes = inventories.Sum(i => i.TotalSizeBytes);
                result.TotalFiles = enhancedProgress.TotalFiles;
                result.TotalBytes = enhancedProgress.TotalBytes;

                // Step 2: Validate storage space
                await ReportProgressThrottled(progress, enhancedProgress, "Validating storage space...");
                
                result.StorageInfo = await _storageValidationService.ValidateStorageSpaceAsync(profile);
                
                if (!result.StorageInfo.HasSufficientSpace)
                {
                    _logger.LogWarning("Insufficient storage space for transfer");
                    // Continue with transfer but log the warning
                }

                // Step 3: Execute transfers
                await ReportProgressThrottled(progress, enhancedProgress, "Starting file transfers...");

                for (int mappingIndex = 0; mappingIndex < profile.FolderMappings.Count; mappingIndex++)
                {
                    var mapping = profile.FolderMappings[mappingIndex];
                    var inventory = inventories[mappingIndex];

                    if (inventory.TotalFileCount == 0)
                    {
                        _logger.LogInformation("No files to transfer in {SourcePath}", mapping.SourcePath);
                        continue;
                    }

                    enhancedProgress.CurrentFile = $"Processing {mapping.SourcePath}";
                    await ReportProgressThrottled(progress, enhancedProgress);

                    await TransferMappingAsync(mapping, inventory, enhancedProgress, progress, cancellationToken);

                    // Check for critically low space during transfer
                    if (_storageValidationService.IsSpaceCriticallyLow(mapping.DestinationPath))
                    {
                        var error = new TransferError
                        {
                            SourcePath = mapping.SourcePath,
                            DestinationPath = mapping.DestinationPath,
                            ErrorMessage = "Transfer paused: Critically low disk space",
                            ErrorType = "Storage"
                        };
                        enhancedProgress.Errors.Add(error);
                        _logger.LogWarning("Transfer paused due to low disk space");
                        break;
                    }
                }

                stopwatch.Stop();
                result.EndTime = DateTime.Now;
                result.Duration = stopwatch.Elapsed;
                result.SuccessfulFiles = enhancedProgress.SuccessfulFiles;
                result.FailedFiles = enhancedProgress.FailedFiles;
                result.TransferredBytes = enhancedProgress.ProcessedBytes;
                result.Errors = enhancedProgress.Errors;
                result.IsSuccess = result.FailedFiles == 0 && result.SuccessfulFiles > 0;

                _logger.LogInformation("Enhanced transfer completed: {SuccessfulFiles}/{TotalFiles} files in {Duration}",
                    result.SuccessfulFiles, result.TotalFiles, result.FormattedDuration);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Transfer cancelled for profile: {ProfileName}", profile.Name);
                result.IsSuccess = false;
                result.EndTime = DateTime.Now;
                result.Duration = stopwatch.Elapsed;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enhanced transfer failed for profile: {ProfileName}", profile.Name);
                result.IsSuccess = false;
                result.EndTime = DateTime.Now;
                result.Duration = stopwatch.Elapsed;
                result.Errors.Add(new TransferError
                {
                    ErrorMessage = ex.Message,
                    Exception = ex,
                    ErrorType = "System"
                });
                return result;
            }
        }

        private async Task TransferMappingAsync(FolderMapping mapping, FileInventoryResult inventory, 
            EnhancedTransferProgress progress, IProgress<EnhancedTransferProgress> progressReporter, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Ensure destination directory exists
                if (!Directory.Exists(mapping.DestinationPath))
                {
                    Directory.CreateDirectory(mapping.DestinationPath);
                }

                // Transfer files individually for better progress tracking
                foreach (var fileInfo in inventory.Files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var relativePath = Path.GetRelativePath(Path.GetDirectoryName(mapping.SourcePath) ?? "", fileInfo.FullName);
                    var destinationFile = Path.Combine(mapping.DestinationPath, relativePath);
                    
                    progress.CurrentFile = fileInfo.Name;
                    await ReportProgressThrottled(progressReporter, progress);

                    try
                    {
                        // Create destination directory if needed
                        var destinationDir = Path.GetDirectoryName(destinationFile);
                        if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                        {
                            Directory.CreateDirectory(destinationDir);
                        }

                        // Copy the file
                        File.Copy(fileInfo.FullName, destinationFile, true);
                        
                        progress.SuccessfulFiles++;
                        progress.ProcessedBytes += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        progress.FailedFiles++;
                        var error = new TransferError
                        {
                            SourcePath = fileInfo.FullName,
                            DestinationPath = destinationFile,
                            ErrorMessage = ex.Message,
                            Exception = ex,
                            ErrorType = GetErrorType(ex)
                        };
                        progress.Errors.Add(error);
                        _logger.LogError(ex, "Failed to copy file: {SourcePath}", fileInfo.FullName);
                    }

                    progress.ProcessedFiles++;
                }
            }
            catch (Exception ex)
            {
                var error = new TransferError
                {
                    SourcePath = mapping.SourcePath,
                    DestinationPath = mapping.DestinationPath,
                    ErrorMessage = ex.Message,
                    Exception = ex,
                    ErrorType = GetErrorType(ex)
                };
                progress.Errors.Add(error);
                _logger.LogError(ex, "Failed to transfer mapping: {SourcePath}", mapping.SourcePath);
            }
        }

        private string GetErrorType(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => "Access Denied",
                DirectoryNotFoundException => "Directory Not Found",
                FileNotFoundException => "File Not Found",
                PathTooLongException => "Path Too Long",
                IOException => "IO Error",
                _ => "Unknown Error"
            };
        }

        public async Task<StorageValidationResult> ValidateStorageSpaceAsync(TransferProfile profile)
        {
            return await _storageValidationService.ValidateStorageSpaceAsync(profile);
        }

        public async Task<FileInventoryResult> BuildFileInventoryAsync(string sourcePath)
        {
            return await _fileInventoryService.BuildFileInventoryAsync(sourcePath);
        }

        public async Task<EnhancedTransferResult> RetryFailedTransfersAsync(EnhancedTransferResult previousResult, IProgress<EnhancedTransferProgress> progress)
        {
            if (previousResult.Errors == null || !previousResult.Errors.Any())
            {
                return previousResult;
            }

            var retryProgress = new EnhancedTransferProgress
            {
                StartTime = DateTime.Now,
                TotalFiles = previousResult.Errors.Count
            };

            var retryResult = new EnhancedTransferResult
            {
                StartTime = DateTime.Now,
                TotalFiles = previousResult.Errors.Count
            };

            foreach (var error in previousResult.Errors)
            {
                if (string.IsNullOrEmpty(error.SourcePath) || string.IsNullOrEmpty(error.DestinationPath))
                    continue;

                retryProgress.CurrentFile = Path.GetFileName(error.SourcePath);
                await ReportProgressThrottled(progress, retryProgress);

                try
                {
                    var destinationDir = Path.GetDirectoryName(error.DestinationPath);
                    if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    File.Copy(error.SourcePath, error.DestinationPath, true);
                    retryResult.SuccessfulFiles++;
                }
                catch (Exception ex)
                {
                    retryResult.FailedFiles++;
                    retryResult.Errors.Add(new TransferError
                    {
                        SourcePath = error.SourcePath,
                        DestinationPath = error.DestinationPath,
                        ErrorMessage = ex.Message,
                        Exception = ex,
                        ErrorType = GetErrorType(ex)
                    });
                }

                retryProgress.ProcessedFiles++;
            }

            retryResult.EndTime = DateTime.Now;
            retryResult.Duration = retryResult.EndTime - retryResult.StartTime;
            retryResult.IsSuccess = retryResult.FailedFiles == 0;

            return retryResult;
        }

        private async Task ReportProgressThrottled(IProgress<EnhancedTransferProgress> progress, EnhancedTransferProgress data, string? currentFile = null)
        {
            if (progress == null) return;

            if (!string.IsNullOrEmpty(currentFile))
                data.CurrentFile = currentFile;

            var now = DateTime.Now;
            if ((now - _lastProgressUpdate).TotalMilliseconds < ProgressThrottleMs)
                return;

            await _progressThrottle.WaitAsync();
            try
            {
                progress.Report(data);
                _lastProgressUpdate = now;
            }
            finally
            {
                _progressThrottle.Release();
            }
        }
    }
}