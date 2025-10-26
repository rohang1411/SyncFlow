using SyncFlow.Models;

namespace SyncFlow.Services;

/// <summary>
/// Service interface for managing transfer profiles
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Gets all transfer profiles
    /// </summary>
    Task<IEnumerable<TransferProfile>> GetAllProfilesAsync();

    /// <summary>
    /// Gets a transfer profile by its ID
    /// </summary>
    Task<TransferProfile?> GetProfileByIdAsync(Guid id);

    /// <summary>
    /// Creates a new transfer profile
    /// </summary>
    Task<TransferProfile> CreateProfileAsync(TransferProfile profile);

    /// <summary>
    /// Updates an existing transfer profile
    /// </summary>
    Task<TransferProfile> UpdateProfileAsync(TransferProfile profile);

    /// <summary>
    /// Deletes a transfer profile
    /// </summary>
    Task DeleteProfileAsync(Guid id);

    /// <summary>
    /// Saves a transfer profile (creates new or updates existing)
    /// </summary>
    /// <param name="profile">The profile to save</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveProfileAsync(TransferProfile profile);

    /// <summary>
    /// Exports profiles to JSON
    /// </summary>
    /// <param name="profiles">Profiles to export</param>
    /// <returns>JSON string</returns>
    Task<string> ExportProfilesAsync(IEnumerable<TransferProfile> profiles);

    /// <summary>
    /// Imports profiles from JSON
    /// </summary>
    /// <param name="json">JSON string containing profiles</param>
    /// <returns>Task representing the async operation</returns>
    Task ImportProfilesAsync(string json);
}