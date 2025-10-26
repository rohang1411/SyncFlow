using System.Text.Json;
using SyncFlow.Models;

namespace SyncFlow.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly string _storagePath;
    private readonly Dictionary<Guid, TransferProfile> _profiles;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProfileRepository()
    {
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SyncFlow",
            "profiles.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _profiles = LoadProfiles();
    }

    public async Task<List<TransferProfile>> GetAllAsync()
    {
        await Task.CompletedTask;
        return _profiles.Values.ToList();
    }

    public async Task<TransferProfile?> GetByIdAsync(Guid id)
    {
        await Task.CompletedTask;
        return _profiles.GetValueOrDefault(id);
    }

    public async Task SaveAsync(TransferProfile profile)
    {
        _profiles[profile.Id] = profile;
        await SaveProfiles();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!_profiles.ContainsKey(id))
        {
            return false;
        }

        _profiles.Remove(id);
        await SaveProfiles();
        return true;
    }

    public async Task<bool> ExistsAsync(string name, Guid? excludeId = null)
    {
        await Task.CompletedTask;
        return _profiles.Values.Any(p => 
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && 
            (!excludeId.HasValue || p.Id != excludeId.Value));
    }

    public async Task<string> ExportProfilesAsync(IEnumerable<TransferProfile> profiles)
    {
        var json = JsonSerializer.Serialize(profiles, _jsonOptions);
        return await Task.FromResult(json);
    }

    public async Task ImportProfilesAsync(string json)
    {
        var profiles = JsonSerializer.Deserialize<List<TransferProfile>>(json, _jsonOptions);
        if (profiles == null) return;

        foreach (var profile in profiles)
        {
            // Migrate from legacy format if needed
            profile.MigrateFromLegacyFormat();
            
            // Ensure unique ID and update timestamps
            profile.Id = Guid.NewGuid();
            profile.CreatedDate = DateTime.UtcNow;
            profile.LastModified = DateTime.UtcNow;
            _profiles[profile.Id] = profile;
        }

        await SaveProfiles();
    }

    private Dictionary<Guid, TransferProfile> LoadProfiles()
    {
        try
        {
            if (File.Exists(_storagePath))
            {
                var json = File.ReadAllText(_storagePath);
                var profiles = JsonSerializer.Deserialize<List<TransferProfile>>(json, _jsonOptions);
                
                if (profiles != null)
                {
                    // Migrate any profiles from old format
                    foreach (var profile in profiles)
                    {
                        profile.MigrateFromLegacyFormat();
                    }
                    
                    return profiles.ToDictionary(p => p.Id);
                }
            }
        }
        catch
        {
            // If loading fails, start with empty dictionary
        }

        return new Dictionary<Guid, TransferProfile>();
    }

    private async Task SaveProfiles()
    {
        var directory = Path.GetDirectoryName(_storagePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        var json = JsonSerializer.Serialize(_profiles.Values.ToList(), _jsonOptions);
        await File.WriteAllTextAsync(_storagePath, json);
    }
}