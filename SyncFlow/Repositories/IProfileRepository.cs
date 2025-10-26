using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SyncFlow.Models;

namespace SyncFlow.Repositories;

/// <summary>
/// Repository interface for managing transfer profile persistence
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Gets all saved transfer profiles
    /// </summary>
    /// <returns>List of all transfer profiles</returns>
    Task<List<TransferProfile>> GetAllAsync();

    /// <summary>
    /// Gets a specific profile by ID
    /// </summary>
    /// <param name="id">The profile ID</param>
    /// <returns>The transfer profile or null if not found</returns>
    Task<TransferProfile?> GetByIdAsync(Guid id);

    /// <summary>
    /// Saves a transfer profile (creates new or updates existing)
    /// </summary>
    /// <param name="profile">The profile to save</param>
    Task SaveAsync(TransferProfile profile);

    /// <summary>
    /// Deletes a transfer profile
    /// </summary>
    /// <param name="id">The ID of the profile to delete</param>
    /// <returns>True if the profile was deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a profile with the given name already exists
    /// </summary>
    /// <param name="name">The profile name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if a profile with the name exists</returns>
    Task<bool> ExistsAsync(string name, Guid? excludeId = null);

    /// <summary>
    /// Exports the specified profiles to a JSON string
    /// </summary>
    /// <param name="profiles">The profiles to export</param>
    /// <returns>JSON string containing the profiles</returns>
    Task<string> ExportProfilesAsync(IEnumerable<TransferProfile> profiles);

    /// <summary>
    /// Imports profiles from a JSON string
    /// </summary>
    /// <param name="json">JSON string containing the profiles</param>
    Task ImportProfilesAsync(string json);
}