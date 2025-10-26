using System.Threading.Tasks;
using SyncFlow.Models;

namespace SyncFlow.Services
{
    public interface IStorageValidationService
    {
        Task<StorageValidationResult> ValidateStorageSpaceAsync(string destinationPath, long requiredBytes);
        Task<StorageValidationResult> ValidateStorageSpaceAsync(TransferProfile profile);
        Task<long> GetAvailableFreeSpaceAsync(string path);
        Task<string> GetDriveLetterAsync(string path);
        bool IsSpaceCriticallyLow(string path, long threshold = 100 * 1024 * 1024); // 100MB default
    }
}