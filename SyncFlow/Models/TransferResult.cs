using System;
using System.Collections.Generic;

namespace SyncFlow.Models;

/// <summary>
/// Represents the result of a file transfer operation
/// </summary>
public class TransferResult
{
    /// <summary>
    /// Profile ID that was transferred
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    /// Status of the transfer
    /// </summary>
    public TransferStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the transfer started
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Timestamp when the transfer completed
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Total number of files to transfer
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Number of files that were successfully transferred
    /// </summary>
    public int FilesCopied { get; set; }

    /// <summary>
    /// Error message if transfer failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indicates whether the transfer completed successfully
    /// </summary>
    public bool Success => Status == TransferStatus.Completed;

    /// <summary>
    /// Time taken to complete the transfer operation
    /// </summary>
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;

    /// <summary>
    /// Gets a summary message of the transfer result
    /// </summary>
    public string GetSummary()
    {
        if (Success)
        {
            return $"Transfer completed successfully. {FilesCopied} files transferred.";
        }
        else
        {
            return $"Transfer failed. {ErrorMessage}";
        }
    }

    /// <summary>
    /// Gets detailed information about the transfer
    /// </summary>
    public string GetDetailedSummary()
    {
        var summary = $"Transfer Summary:\n";
        summary += $"- Status: {Status}\n";
        summary += $"- Duration: {Duration:hh\\:mm\\:ss}\n";
        summary += $"- Total files: {TotalFiles}\n";
        summary += $"- Files copied: {FilesCopied}\n";

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            summary += $"- Error: {ErrorMessage}\n";
        }

        return summary;
    }
}