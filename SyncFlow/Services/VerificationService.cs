using Microsoft.Extensions.Logging;
using SyncFlow.Exceptions;
using SyncFlow.Models;

namespace SyncFlow.Services;

/// <summary>
/// Service implementation for post-transfer verification
/// </summary>
public class VerificationService : IVerificationService
{
    private readonly IFileOperations _fileOperations;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(IFileOperations fileOperations, ILogger<VerificationService> logger)
    {
        _fileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VerificationResult> VerifyTransferAsync(TransferProfile profile, TransferResult result)
    {
        try
        {
            _logger.LogInformation("Starting verification for profile: {ProfileName}", profile.Name);

            var verificationResult = new VerificationResult
            {
                ProfileId = profile.Id,
                StartTime = DateTime.UtcNow,
                IsSuccessful = true
            };

            // Count files in all folder mappings
            int sourceFileCount = 0;
            int destinationFileCount = 0;

            foreach (var mapping in profile.FolderMappings)
            {
                if (_fileOperations.DirectoryExists(mapping.SourcePath))
                {
                    sourceFileCount += await _fileOperations.CountFilesRecursivelyAsync(mapping.SourcePath);
                }

                if (_fileOperations.DirectoryExists(mapping.DestinationPath))
                {
                    destinationFileCount += await _fileOperations.CountFilesRecursivelyAsync(mapping.DestinationPath);
                }
            }

            verificationResult.SourceFileCount = sourceFileCount;
            verificationResult.DestinationFileCount = destinationFileCount;
            verificationResult.FilesMatch = sourceFileCount == destinationFileCount;

            if (!verificationResult.FilesMatch)
            {
                verificationResult.IsSuccessful = false;
                verificationResult.ErrorMessage = $"File count mismatch: Source has {sourceFileCount} files, destination has {destinationFileCount} files";
                _logger.LogWarning("File count mismatch for profile: {ProfileName}. Source: {SourceCount}, Destination: {DestCount}", 
                    profile.Name, sourceFileCount, destinationFileCount);
            }

            verificationResult.EndTime = DateTime.UtcNow;

            _logger.LogInformation("Verification completed for profile: {ProfileName}. Success: {Success}", 
                profile.Name, verificationResult.IsSuccessful);

            return verificationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Verification failed for profile: {ProfileName}", profile.Name);
            return new VerificationResult
            {
                ProfileId = profile.Id,
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                EndTime = DateTime.UtcNow
            };
        }
    }

    public async Task<VerificationReport> GenerateDetailedReportAsync(TransferProfile profile)
    {
        try
        {
            _logger.LogInformation("Generating detailed verification report for profile: {ProfileName}", profile.Name);

            var report = new VerificationReport();
            var sourceFiles = new Dictionary<string, long>(); // path -> size
            var destFiles = new Dictionary<string, long>();

            // Collect all source files
            foreach (var mapping in profile.FolderMappings)
            {
                if (_fileOperations.DirectoryExists(mapping.SourcePath))
                {
                    var files = Directory.GetFiles(mapping.SourcePath, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var relativePath = Path.GetRelativePath(mapping.SourcePath, file);
                        var fileInfo = new FileInfo(file);
                        sourceFiles[relativePath] = fileInfo.Length;
                    }
                }
            }

            // Collect all destination files
            foreach (var mapping in profile.FolderMappings)
            {
                if (_fileOperations.DirectoryExists(mapping.DestinationPath))
                {
                    var files = Directory.GetFiles(mapping.DestinationPath, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var relativePath = Path.GetRelativePath(mapping.DestinationPath, file);
                        var fileInfo = new FileInfo(file);
                        destFiles[relativePath] = fileInfo.Length;
                    }
                }
            }

            report.TotalSourceFiles = sourceFiles.Count;
            report.TotalDestinationFiles = destFiles.Count;

            // Find missing files (in source but not in destination)
            foreach (var sourceFile in sourceFiles.Keys)
            {
                if (!destFiles.ContainsKey(sourceFile))
                {
                    report.MissingFiles.Add(sourceFile);
                }
                else if (sourceFiles[sourceFile] != destFiles[sourceFile])
                {
                    report.SizeMismatches[sourceFile] = 
                        $"Source: {FormatBytes(sourceFiles[sourceFile])}, Dest: {FormatBytes(destFiles[sourceFile])}";
                }
                else
                {
                    report.MatchingFiles++;
                }
            }

            // Find extra files (in destination but not in source)
            foreach (var destFile in destFiles.Keys)
            {
                if (!sourceFiles.ContainsKey(destFile))
                {
                    report.ExtraFiles.Add(destFile);
                }
            }

            _logger.LogInformation("Detailed report generated. Missing: {Missing}, Mismatches: {Mismatches}, Extra: {Extra}",
                report.MissingFiles.Count, report.SizeMismatches.Count, report.ExtraFiles.Count);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate detailed report for profile: {ProfileName}", profile.Name);
            throw new SyncFlowException("Failed to generate verification report", ex);
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public async Task<int> CountFilesInFoldersAsync(IEnumerable<string> folders)
    {
        try
        {
            int totalFiles = 0;
            foreach (var folder in folders)
            {
                if (_fileOperations.DirectoryExists(folder))
                {
                    totalFiles += await _fileOperations.CountFilesRecursivelyAsync(folder);
                }
            }
            return totalFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count files in folders");
            throw new SyncFlowException("Failed to count files in folders", ex);
        }
    }
}
