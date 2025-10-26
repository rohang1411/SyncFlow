namespace SyncFlow.Models;

/// <summary>
/// Represents progress information for a file transfer operation
/// </summary>
public class TransferProgress
{
    /// <summary>
    /// Number of files copied so far
    /// </summary>
    public int FilesCopied { get; set; }

    /// <summary>
    /// Total number of files to copy
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Current file being processed
    /// </summary>
    public string CurrentFile { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// Bytes transferred so far
    /// </summary>
    public long BytesTransferred { get; set; }

    /// <summary>
    /// Total bytes to transfer
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Transfer speed in MB/s
    /// </summary>
    public double TransferSpeed { get; set; }

    /// <summary>
    /// Time elapsed since transfer started
    /// </summary>
    public TimeSpan TimeElapsed { get; set; }
}