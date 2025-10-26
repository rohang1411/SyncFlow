namespace SyncFlow.Models;

/// <summary>
/// Represents the current status of a transfer operation
/// </summary>
public enum TransferStatus
{
    /// <summary>
    /// Transfer is ready to be executed
    /// </summary>
    Ready,

    /// <summary>
    /// Transfer is currently running
    /// </summary>
    Running,

    /// <summary>
    /// Transfer is in verification phase
    /// </summary>
    Verifying,

    /// <summary>
    /// Transfer completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Transfer failed with errors
    /// </summary>
    Failed,

    /// <summary>
    /// Transfer was cancelled by user
    /// </summary>
    Cancelled
}

/// <summary>
/// Extension methods for TransferStatus enum
/// </summary>
public static class TransferStatusExtensions
{
    /// <summary>
    /// Gets a user-friendly display message for the status
    /// </summary>
    /// <param name="status">The transfer status</param>
    /// <returns>Display message</returns>
    public static string GetDisplayMessage(this TransferStatus status)
    {
        return status switch
        {
            TransferStatus.Ready => "Ready",
            TransferStatus.Running => "Running...",
            TransferStatus.Verifying => "Verifying...",
            TransferStatus.Completed => "Completed Successfully",
            TransferStatus.Failed => "Failed with Errors",
            TransferStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Determines if the status indicates an active operation
    /// </summary>
    /// <param name="status">The transfer status</param>
    /// <returns>True if the operation is active</returns>
    public static bool IsActive(this TransferStatus status)
    {
        return status == TransferStatus.Running || status == TransferStatus.Verifying;
    }

    /// <summary>
    /// Determines if the status indicates a completed operation (success or failure)
    /// </summary>
    /// <param name="status">The transfer status</param>
    /// <returns>True if the operation is completed</returns>
    public static bool IsCompleted(this TransferStatus status)
    {
        return status == TransferStatus.Completed || 
               status == TransferStatus.Failed || 
               status == TransferStatus.Cancelled;
    }
}