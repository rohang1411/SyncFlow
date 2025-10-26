using SyncFlow.Models;

namespace SyncFlow.Services;

/// <summary>
/// Service interface for post-transfer verification
/// </summary>
public interface IVerificationService
{
    /// <summary>
    /// Verifies that a transfer completed successfully by comparing file counts
    /// </summary>
    /// <param name="profile">The transfer profile that was executed</param>
    /// <param name="result">The transfer result to verify</param>
    /// <returns>Verification result with details</returns>
    Task<VerificationResult> VerifyTransferAsync(TransferProfile profile, TransferResult result);

    /// <summary>
    /// Generates a detailed verification report comparing source and destination folders
    /// </summary>
    /// <param name="profile">The transfer profile to verify</param>
    /// <returns>Detailed verification report</returns>
    Task<VerificationReport> GenerateDetailedReportAsync(TransferProfile profile);

    /// <summary>
    /// Counts all files in the specified folders recursively
    /// </summary>
    /// <param name="folders">The folders to count files in</param>
    /// <returns>Total number of files</returns>
    Task<int> CountFilesInFoldersAsync(IEnumerable<string> folders);
}