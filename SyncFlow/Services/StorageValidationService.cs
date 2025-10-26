using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SyncFlow.Models;

namespace SyncFlow.Services
{
    public class StorageValidationService : IStorageValidationService
    {
        private readonly IFileInventoryService _fileInventoryService;

        public StorageValidationService(IFileInventoryService fileInventoryService)
        {
            _fileInventoryService = fileInventoryService;
        }

        public async Task<StorageValidationResult> ValidateStorageSpaceAsync(string destinationPath, long requiredBytes)
        {
            var result = new StorageValidationResult
            {
                RequiredBytes = requiredBytes,
                DestinationDrive = await GetDriveLetterAsync(destinationPath)
            };

            try
            {
                result.AvailableBytes = await GetAvailableFreeSpaceAsync(destinationPath);
                result.HasSufficientSpace = result.AvailableBytes >= requiredBytes;

                if (result.HasSufficientSpace)
                {
                    result.FormattedMessage = $"Sufficient space available on {result.DestinationDrive}\n" +
                                            $"Required: {result.FormattedRequired}\n" +
                                            $"Available: {result.FormattedAvailable}";
                }
                else
                {
                    var shortage = requiredBytes - result.AvailableBytes;
                    result.FormattedMessage = $"Insufficient space on {result.DestinationDrive}\n" +
                                            $"Required: {result.FormattedRequired}\n" +
                                            $"Available: {result.FormattedAvailable}\n" +
                                            $"Shortage: {result.FormattedShortage}\n\n" +
                                            $"Please free up at least {FormatBytes(shortage)} of space or choose a different destination.";
                }
            }
            catch (Exception ex)
            {
                result.HasSufficientSpace = false;
                result.FormattedMessage = $"Unable to check available space: {ex.Message}";
            }

            return result;
        }

        public async Task<StorageValidationResult> ValidateStorageSpaceAsync(TransferProfile profile)
        {
            if (profile?.FolderMappings == null || !profile.FolderMappings.Any())
            {
                return new StorageValidationResult
                {
                    HasSufficientSpace = false,
                    FormattedMessage = "No folder mappings defined in profile"
                };
            }

            long totalRequiredBytes = 0;
            var validationTasks = profile.FolderMappings.Select(async mapping =>
            {
                try
                {
                    var inventory = await _fileInventoryService.BuildFileInventoryAsync(mapping.SourcePath);
                    return inventory.TotalSizeBytes;
                }
                catch
                {
                    return 0L; // Skip mappings we can't access
                }
            });

            var sizes = await Task.WhenAll(validationTasks);
            totalRequiredBytes = sizes.Sum();

            // Use the first destination path for validation (assuming same drive for all mappings)
            var firstDestination = profile.FolderMappings.First().DestinationPath;
            return await ValidateStorageSpaceAsync(firstDestination, totalRequiredBytes);
        }

        public async Task<long> GetAvailableFreeSpaceAsync(string path)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path)) ?? "C:");
                    return drive.AvailableFreeSpace;
                }
                catch
                {
                    return 0L;
                }
            });
        }

        public async Task<string> GetDriveLetterAsync(string path)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var root = Path.GetPathRoot(Path.GetFullPath(path));
                    return string.IsNullOrEmpty(root) ? "Unknown Drive" : root;
                }
                catch
                {
                    return "Unknown Drive";
                }
            });
        }

        public bool IsSpaceCriticallyLow(string path, long threshold = 100 * 1024 * 1024)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path)) ?? "C:");
                return drive.AvailableFreeSpace < threshold;
            }
            catch
            {
                return true; // Assume critically low if we can't check
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 0) return "0 B";
            
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}