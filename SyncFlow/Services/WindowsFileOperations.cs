using Microsoft.Extensions.Logging;
using SyncFlow.Exceptions;
using System.IO;

namespace SyncFlow.Services;

/// <summary>
/// Windows-specific implementation of file operations using native Windows file copy
/// </summary>
public class WindowsFileOperations : IFileOperations
{
    private readonly ILogger<WindowsFileOperations> _logger;

    public WindowsFileOperations(ILogger<WindowsFileOperations> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> CopyDirectoryAsync(string sourceDirectory, string destinationDirectory,
        bool overwriteExisting = false, bool showProgressDialog = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dir = new DirectoryInfo(sourceDirectory);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");
            }

            int filesCopied = 0;
            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDirectory);

            foreach (FileInfo file in dir.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();

                string targetFilePath = Path.Combine(destinationDirectory, file.Name);
                if (await CopyFileAsync(file.FullName, targetFilePath, overwriteExisting, cancellationToken))
                {
                    filesCopied++;
                }
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string newDestinationDir = Path.Combine(destinationDirectory, subdir.Name);
                filesCopied += await CopyDirectoryAsync(subdir.FullName, newDestinationDir,
                    overwriteExisting, showProgressDialog, cancellationToken);
            }

            return filesCopied;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not TransferException)
        {
            _logger.LogError(ex, "Failed to copy directory: {SourceDir} to {DestinationDir}",
                sourceDirectory, destinationDirectory);
            throw new TransferException("Failed to copy directory", sourceDirectory, destinationDirectory, ex);
        }
    }

    public async Task<bool> CopyFileAsync(string sourceFile, string destinationFile,
        bool overwriteExisting = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException($"Source file not found: {sourceFile}");
            }

            if (File.Exists(destinationFile))
            {
                if (!overwriteExisting)
                {
                    return false;
                }
            }

            using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);

            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy file: {SourceFile} to {DestinationFile}",
                sourceFile, destinationFile);
            throw new TransferException("Failed to copy file", sourceFile, destinationFile, ex);
        }
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public bool DirectoryExists(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    public async Task<List<string>> GetFilesRecursivelyAsync(string directoryPath, string searchPattern = "*")
    {
        return await Task.Run(() =>
        {
            try
            {
                return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get files from directory: {DirectoryPath}", directoryPath);
                throw new SyncFlowException($"Failed to get files from directory: {directoryPath}", ex);
            }
        });
    }

    public async Task<int> CountFilesRecursivelyAsync(string directoryPath, string searchPattern = "*")
    {
        var files = await GetFilesRecursivelyAsync(directoryPath, searchPattern);
        return files.Count;
    }

    public long GetFileSize(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file size: {FilePath}", filePath);
            throw new SyncFlowException($"Failed to get file size: {filePath}", ex);
        }
    }

    public void CreateDirectory(string directoryPath)
    {
        try
        {
            Directory.CreateDirectory(directoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory: {DirectoryPath}", directoryPath);
            throw new SyncFlowException($"Failed to create directory: {directoryPath}", ex);
        }
    }
}