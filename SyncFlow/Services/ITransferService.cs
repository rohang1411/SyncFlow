using SyncFlow.Models;

namespace SyncFlow.Services;

/// <summary>
/// Service interface for handling file transfer operations
/// </summary>
public interface ITransferService
{
    /// <summary>
    /// Executes a file transfer operation for the specified profile
    /// </summary>
    /// <param name="profile">The transfer profile containing source and destination information</param>
    /// <param name="progress">Progress reporter for transfer updates</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Transfer result containing success status and details</returns>
    Task<TransferResult> ExecuteTransferAsync(TransferProfile profile, 
        IProgress<TransferProgress>? progress, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that all paths in the profile are accessible
    /// </summary>
    /// <param name="profile">The profile to validate</param>
    /// <returns>True if all paths are valid and accessible</returns>
    Task<bool> ValidatePathsAsync(TransferProfile profile);
}