using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SyncFlow.Services;

/// <summary>
/// Interface for file system operations
/// </summary>
public interface IFileOperations
{
    /// <summary>
    /// Copies a directory and all its contents to a destination
    /// </summary>
    /// <param name="sourceDirectory">Source directory path</param>
    /// <param name="destinationDirectory">Destination directory path</param>
    /// <param name="overwriteExisting">Whether to overwrite existing files</param>
    /// <param name="showProgressDialog">Whether to show native Windows progress dialog</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of files copied</returns>
    Task<int> CopyDirectoryAsync(string sourceDirectory, string destinationDirectory, 
        bool overwriteExisting = false, bool showProgressDialog = true, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a single file to a destination
    /// </summary>
    /// <param name="sourceFile">Source file path</param>
    /// <param name="destinationFile">Destination file path</param>
    /// <param name="overwriteExisting">Whether to overwrite existing file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file was copied, false if skipped</returns>
    Task<bool> CopyFileAsync(string sourceFile, string destinationFile, 
        bool overwriteExisting = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists at the specified path
    /// </summary>
    /// <param name="filePath">File path to check</param>
    /// <returns>True if file exists</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Checks if a directory exists at the specified path
    /// </summary>
    /// <param name="directoryPath">Directory path to check</param>
    /// <returns>True if directory exists</returns>
    bool DirectoryExists(string directoryPath);

    /// <summary>
    /// Gets all files in a directory recursively
    /// </summary>
    /// <param name="directoryPath">Directory path</param>
    /// <param name="searchPattern">Search pattern (default: "*")</param>
    /// <returns>List of file paths</returns>
    Task<List<string>> GetFilesRecursivelyAsync(string directoryPath, string searchPattern = "*");

    /// <summary>
    /// Counts files in a directory recursively
    /// </summary>
    /// <param name="directoryPath">Directory path</param>
    /// <param name="searchPattern">Search pattern (default: "*")</param>
    /// <returns>Number of files</returns>
    Task<int> CountFilesRecursivelyAsync(string directoryPath, string searchPattern = "*");

    /// <summary>
    /// Gets the size of a file in bytes
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns>File size in bytes</returns>
    long GetFileSize(string filePath);

    /// <summary>
    /// Creates a directory if it doesn't exist
    /// </summary>
    /// <param name="directoryPath">Directory path to create</param>
    void CreateDirectory(string directoryPath);
}