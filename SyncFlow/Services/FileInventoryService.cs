using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SyncFlow.Models;

namespace SyncFlow.Services
{
    public class FileInventoryService : IFileInventoryService
    {
        public async Task<FileInventoryResult> BuildFileInventoryAsync(string sourcePath)
        {
            return await BuildFileInventoryAsync(sourcePath, CancellationToken.None);
        }

        public async Task<FileInventoryResult> BuildFileInventoryAsync(string sourcePath, CancellationToken cancellationToken)
        {
            var result = new FileInventoryResult();
            
            if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
            {
                result.InaccessiblePaths.Add($"Source path does not exist: {sourcePath}");
                return result;
            }

            try
            {
                await Task.Run(() => ScanDirectory(sourcePath, result, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result.InaccessiblePaths.Add($"Error scanning {sourcePath}: {ex.Message}");
            }

            return result;
        }

        public async Task<long> CalculateDirectorySizeAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return 0;

            return await Task.Run(() =>
            {
                long totalSize = 0;
                try
                {
                    var files = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            totalSize += fileInfo.Length;
                        }
                        catch
                        {
                            // Skip files we can't access
                        }
                    }
                }
                catch
                {
                    // Return partial size if enumeration fails
                }
                return totalSize;
            });
        }

        public async Task<int> CountFilesAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return 0;

            return await Task.Run(() =>
            {
                try
                {
                    return Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories).Count();
                }
                catch
                {
                    return 0;
                }
            });
        }

        private void ScanDirectory(string directoryPath, FileInventoryResult result, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Count directories
                var directories = Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories);
                foreach (var dir in directories)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        result.DirectoryCount++;
                    }
                    catch (Exception ex)
                    {
                        result.InaccessiblePaths.Add($"Directory access error {dir}: {ex.Message}");
                    }
                }

                // Scan files
                var files = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (var filePath in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    try
                    {
                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo.Exists)
                        {
                            result.Files.Add(fileInfo);
                            result.TotalFileCount++;
                            result.TotalSizeBytes += fileInfo.Length;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        result.InaccessiblePaths.Add($"Access denied: {filePath}");
                    }
                    catch (DirectoryNotFoundException)
                    {
                        result.InaccessiblePaths.Add($"Directory not found: {Path.GetDirectoryName(filePath)}");
                    }
                    catch (FileNotFoundException)
                    {
                        result.InaccessiblePaths.Add($"File not found: {filePath}");
                    }
                    catch (PathTooLongException)
                    {
                        result.InaccessiblePaths.Add($"Path too long: {filePath}");
                    }
                    catch (IOException ex)
                    {
                        result.InaccessiblePaths.Add($"IO error {filePath}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        result.InaccessiblePaths.Add($"Unexpected error {filePath}: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                result.InaccessiblePaths.Add($"Access denied to directory: {directoryPath}");
            }
            catch (DirectoryNotFoundException)
            {
                result.InaccessiblePaths.Add($"Directory not found: {directoryPath}");
            }
            catch (Exception ex)
            {
                result.InaccessiblePaths.Add($"Error scanning directory {directoryPath}: {ex.Message}");
            }
        }
    }
}