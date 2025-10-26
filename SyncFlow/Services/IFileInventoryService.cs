using System.Threading.Tasks;
using SyncFlow.Models;

namespace SyncFlow.Services
{
    public interface IFileInventoryService
    {
        Task<FileInventoryResult> BuildFileInventoryAsync(string sourcePath);
        Task<FileInventoryResult> BuildFileInventoryAsync(string sourcePath, System.Threading.CancellationToken cancellationToken);
        Task<long> CalculateDirectorySizeAsync(string directoryPath);
        Task<int> CountFilesAsync(string directoryPath);
    }
}