using System;
using System.Threading;
using System.Threading.Tasks;
using SyncFlow.Models;

namespace SyncFlow.Services
{
    public interface IEnhancedTransferService
    {
        Task<EnhancedTransferResult> TransferWithValidationAsync(TransferProfile profile, IProgress<EnhancedTransferProgress> progress);
        Task<EnhancedTransferResult> TransferWithValidationAsync(TransferProfile profile, IProgress<EnhancedTransferProgress> progress, CancellationToken cancellationToken);
        Task<StorageValidationResult> ValidateStorageSpaceAsync(TransferProfile profile);
        Task<FileInventoryResult> BuildFileInventoryAsync(string sourcePath);
        Task<EnhancedTransferResult> RetryFailedTransfersAsync(EnhancedTransferResult previousResult, IProgress<EnhancedTransferProgress> progress);
    }
}