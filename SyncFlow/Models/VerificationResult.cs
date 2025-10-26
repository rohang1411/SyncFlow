namespace SyncFlow.Models;

/// <summary>
/// Represents the result of post-transfer verification
/// </summary>
public class VerificationResult
{
    /// <summary>
    /// Profile ID that was verified
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    /// Start time of verification
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of verification
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Indicates whether verification passed
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Number of files counted in source folders
    /// </summary>
    public int SourceFileCount { get; set; }

    /// <summary>
    /// Number of files found in destination folder
    /// </summary>
    public int DestinationFileCount { get; set; }

    /// <summary>
    /// Whether file counts match
    /// </summary>
    public bool FilesMatch { get; set; }

    /// <summary>
    /// Error message if verification failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Time taken to perform verification
    /// </summary>
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;

    /// <summary>
    /// Gets a summary message of the verification result
    /// </summary>
    public string GetSummaryMessage()
    {
        if (IsSuccessful)
        {
            return $"Verification passed. Source: {SourceFileCount} files, Destination: {DestinationFileCount} files.";
        }
        else
        {
            return $"Verification failed. {ErrorMessage}";
        }
    }

    /// <summary>
    /// Gets detailed verification information
    /// </summary>
    public string GetDetailedSummary()
    {
        var summary = $"Verification Summary:\n";
        summary += $"- Source file count: {SourceFileCount}\n";
        summary += $"- Destination file count: {DestinationFileCount}\n";
        summary += $"- Files match: {FilesMatch}\n";
        summary += $"- Verification duration: {Duration:hh\\:mm\\:ss}\n";

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            summary += $"- Error: {ErrorMessage}\n";
        }

        return summary;
    }
}