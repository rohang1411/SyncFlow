using Microsoft.Extensions.Logging;
using SyncFlow.Exceptions;
using SyncFlow.Models;
using System.Diagnostics;

namespace SyncFlow.Services;

/// <summary>
/// Service implementation for handling file transfer operations
/// </summary>
public class TransferService : ITransferService
{
    private readonly IFileOperations _fileOperations;
    private readonly ILogger<TransferService> _logger;
    private readonly SemaphoreSlim _progressThrottle = new(1, 1);
    private DateTime _lastProgressUpdate = DateTime.MinValue;
    private const int ProgressThrottleMs = 100; // Minimum 100ms between updates

    public TransferService(IFileOperations fileOperations, ILogger<TransferService> logger)
    {
        _fileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransferResult> ExecuteTransferAsync(TransferProfile profile, 
        IProgress<TransferProgress>? progress, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting transfer for profile: {ProfileName}", profile.Name);

            var result = new TransferResult
            {
                ProfileId = profile.Id,
                StartTime = DateTime.UtcNow,
                Status = TransferStatus.Running
            };

            int totalFiles = 0;
            int copiedFiles = 0;
            long totalBytes = 0;
            long copiedBytes = 0;

            // Count total files first for all mappings
            foreach (var mapping in profile.FolderMappings)
            {
                if (_fileOperations.DirectoryExists(mapping.SourcePath))
                {
                    totalFiles += await _fileOperations.CountFilesRecursivelyAsync(mapping.SourcePath);
                }
            }

            result.TotalFiles = totalFiles;

            // Execute transfers for each folder mapping
            for (int i = 0; i < profile.FolderMappings.Count; i++)
            {
                var mapping = profile.FolderMappings[i];
                
                if (!_fileOperations.DirectoryExists(mapping.SourcePath))
                {
                    _logger.LogWarning("Source folder does not exist: {SourcePath}", mapping.SourcePath);
                    continue;
                }

                await ReportProgressThrottled(progress, new TransferProgress
                {
                    FilesCopied = copiedFiles,
                    TotalFiles = totalFiles,
                    CurrentFile = $"Processing mapping {i + 1}/{profile.FolderMappings.Count}: {mapping.SourcePath}",
                    Percentage = totalFiles > 0 ? (double)copiedFiles / totalFiles * 100 : 0,
                    BytesTransferred = copiedBytes,
                    TotalBytes = totalBytes,
                    TransferSpeed = CalculateSpeed(copiedBytes, stopwatch.Elapsed),
                    TimeElapsed = stopwatch.Elapsed
                });

                var filesCopied = await _fileOperations.CopyDirectoryAsync(
                    mapping.SourcePath,
                    mapping.DestinationPath,
                    profile.OverwriteExisting,
                    false, // Don't show native progress dialog
                    cancellationToken);

                copiedFiles += filesCopied;

                // Report progress after each mapping
                await ReportProgressThrottled(progress, new TransferProgress
                {
                    FilesCopied = copiedFiles,
                    TotalFiles = totalFiles,
                    CurrentFile = $"Completed: {mapping.SourcePath} → {mapping.DestinationPath}",
                    Percentage = totalFiles > 0 ? (double)copiedFiles / totalFiles * 100 : 0,
                    BytesTransferred = copiedBytes,
                    TotalBytes = totalBytes,
                    TransferSpeed = CalculateSpeed(copiedBytes, stopwatch.Elapsed),
                    TimeElapsed = stopwatch.Elapsed
                });
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.FilesCopied = copiedFiles;
            result.Status = TransferStatus.Completed;

            _logger.LogInformation("Transfer completed for profile: {ProfileName}. Files copied: {FilesCopied}/{TotalFiles} in {Duration}ms", 
                profile.Name, copiedFiles, totalFiles, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Transfer cancelled for profile: {ProfileName}", profile.Name);
            return new TransferResult
            {
                ProfileId = profile.Id,
                Status = TransferStatus.Cancelled,
                EndTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transfer failed for profile: {ProfileName}", profile.Name);
            return new TransferResult
            {
                ProfileId = profile.Id,
                Status = TransferStatus.Failed,
                ErrorMessage = ex.Message,
                EndTime = DateTime.UtcNow
            };
        }
    }

    private async Task ReportProgressThrottled(IProgress<TransferProgress>? progress, TransferProgress data)
    {
        if (progress == null) return;

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

    private double CalculateSpeed(long bytes, TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds == 0) return 0;
        return bytes / elapsed.TotalSeconds / (1024 * 1024); // MB/s
    }

    public async Task<bool> ValidatePathsAsync(TransferProfile profile)
    {
        try
        {
            // Check all folder mappings
            foreach (var mapping in profile.FolderMappings)
            {
                if (!_fileOperations.DirectoryExists(mapping.SourcePath))
                {
                    _logger.LogWarning("Source folder does not exist: {SourcePath}", mapping.SourcePath);
                    return false;
                }

                if (!_fileOperations.DirectoryExists(mapping.DestinationPath))
                {
                    _logger.LogInformation("Destination folder does not exist, will be created: {DestinationPath}", 
                        mapping.DestinationPath);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Path validation failed for profile: {ProfileName}", profile.Name);
            return false;
        }
    }
}
