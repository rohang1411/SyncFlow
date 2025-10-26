using SyncFlow.Exceptions;
using SyncFlow.Models;
using SyncFlow.Repositories;

namespace SyncFlow.Services;

/// <summary>
/// Service implementation for managing transfer profiles
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IProfileRepository _repository;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(IProfileRepository repository, ILogger<ProfileService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<TransferProfile>> GetAllProfilesAsync()
    {
        try
        {
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get profiles");
            throw new SyncFlowException("Failed to get profiles", ex);
        }
    }

    public async Task<TransferProfile?> GetProfileByIdAsync(Guid id)
    {
        try
        {
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get profile with ID: {Id}", id);
            throw new SyncFlowException($"Failed to get profile with ID: {id}", ex);
        }
    }

    public async Task<TransferProfile> CreateProfileAsync(TransferProfile profile)
    {
        try
        {
            ValidateProfile(profile);
            profile.Id = Guid.NewGuid(); // Ensure new ID for creation
            await _repository.SaveAsync(profile);
            return profile;
        }
        catch (ProfileValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create profile: {ProfileName}", profile.Name);
            throw new SyncFlowException($"Failed to create profile: {profile.Name}", ex);
        }
    }

    public async Task<TransferProfile> UpdateProfileAsync(TransferProfile profile)
    {
        try
        {
            ValidateProfile(profile);
            var existing = await _repository.GetByIdAsync(profile.Id);
            if (existing == null)
            {
                throw new SyncFlowException($"Profile not found with ID: {profile.Id}");
            }
            
            await _repository.SaveAsync(profile);
            return profile;
        }
        catch (ProfileValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update profile: {ProfileName}", profile.Name);
            throw new SyncFlowException($"Failed to update profile: {profile.Name}", ex);
        }
    }

    public async Task DeleteProfileAsync(Guid id)
    {
        try
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                throw new SyncFlowException($"Profile not found with ID: {id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete profile with ID: {Id}", id);
            throw new SyncFlowException($"Failed to delete profile with ID: {id}", ex);
        }
    }

    public async Task SaveProfileAsync(TransferProfile profile)
    {
        try
        {
            ValidateProfile(profile);
            
            // Check for duplicates (excluding the current profile if updating)
            var allProfiles = await _repository.GetAllAsync();
            var duplicate = allProfiles.FirstOrDefault(p => p.Id != profile.Id && p.IsDuplicate(profile));
            
            if (duplicate != null)
            {
                _logger.LogWarning("Duplicate profile detected: {ProfileName}", profile.Name);
                // Throw a specific exception that can be caught by the UI
                throw new SyncFlowException($"A profile with the same configuration already exists: '{duplicate.Name}'. Please modify the profile or use a different name.");
            }
            
            // Check if profile exists
            var existing = await _repository.GetByIdAsync(profile.Id);
            
            if (existing == null)
            {
                // New profile - ensure it has an ID
                if (profile.Id == Guid.Empty)
                {
                    profile.Id = Guid.NewGuid();
                }
                profile.CreatedDate = DateTime.UtcNow;
            }
            
            profile.LastModified = DateTime.UtcNow;
            await _repository.SaveAsync(profile);
        }
        catch (ProfileValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile: {ProfileName}", profile.Name);
            throw new SyncFlowException($"Failed to save profile: {profile.Name}", ex);
        }
    }

    public async Task<string> ExportProfilesAsync(IEnumerable<TransferProfile> profiles)
    {
        try
        {
            return await _repository.ExportProfilesAsync(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export profiles");
            throw new SyncFlowException("Failed to export profiles", ex);
        }
    }

    public async Task ImportProfilesAsync(string json)
    {
        try
        {
            await _repository.ImportProfilesAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import profiles");
            throw new SyncFlowException("Failed to import profiles", ex);
        }
    }

    private void ValidateProfile(TransferProfile profile)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            errors.Add("Profile name is required");
        }

        if (profile.FolderMappings == null || !profile.FolderMappings.Any())
        {
            errors.Add("At least one folder mapping is required");
        }
        else
        {
            for (int i = 0; i < profile.FolderMappings.Count; i++)
            {
                var mapping = profile.FolderMappings[i];
                
                if (string.IsNullOrWhiteSpace(mapping.SourcePath))
                {
                    errors.Add($"Folder mapping {i + 1}: Source path is required");
                }
                else if (!Directory.Exists(mapping.SourcePath))
                {
                    errors.Add($"Folder mapping {i + 1}: Source folder does not exist: {mapping.SourcePath}");
                }

                if (string.IsNullOrWhiteSpace(mapping.DestinationPath))
                {
                    errors.Add($"Folder mapping {i + 1}: Destination path is required");
                }
            }
        }

        if (errors.Any())
        {
            throw new ProfileValidationException(errors);
        }
    }
}