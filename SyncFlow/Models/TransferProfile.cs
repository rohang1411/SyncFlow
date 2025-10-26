using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SyncFlow.Models
{
    /// <summary>
    /// Represents a transfer profile configuration for file operations
    /// </summary>
    public class TransferProfile : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private System.Collections.Generic.List<FolderMapping> _folderMappings = new();

        // Legacy properties for backward compatibility (will be migrated)
        private System.Collections.Generic.List<string>? _sourceFolders;
        private string? _destinationFolder;

        /// <summary>
        /// Unique identifier for the profile
        /// </summary>
        public System.Guid Id { get; set; } = System.Guid.NewGuid();

        /// <summary>
        /// User-defined name for the profile
        /// </summary>
        [Required(ErrorMessage = "Profile name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Profile name must be between 1 and 100 characters")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// List of folder mappings (source → destination pairs)
        /// </summary>
        [Required(ErrorMessage = "At least one folder mapping is required")]
        [MinLength(1, ErrorMessage = "At least one folder mapping must be specified")]
        public System.Collections.Generic.List<FolderMapping> FolderMappings
        {
            get => _folderMappings;
            set
            {
                if (_folderMappings != value)
                {
                    _folderMappings = value ?? new System.Collections.Generic.List<FolderMapping>();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Legacy: List of source folders (for backward compatibility during deserialization)
        /// </summary>
        [Obsolete("Use FolderMappings instead")]
        public System.Collections.Generic.List<string>? SourceFolders
        {
            get => _sourceFolders;
            set => _sourceFolders = value;
        }

        /// <summary>
        /// Legacy: Destination folder (for backward compatibility during deserialization)
        /// </summary>
        [Obsolete("Use FolderMappings instead")]
        public string? DestinationFolder
        {
            get => _destinationFolder;
            set => _destinationFolder = value;
        }

        /// <summary>
        /// Overwrite existing files in the destination
        /// </summary>
        public bool OverwriteExisting { get; set; }

        /// <summary>
        /// Date and time when the profile was created
        /// </summary>
        public System.DateTime CreatedDate { get; set; } = System.DateTime.UtcNow;

        /// <summary>
        /// Date and time when the profile was last modified
        /// </summary>
        public System.DateTime LastModified { get; set; } = System.DateTime.UtcNow;

        /// <summary>
        /// Property changed event for UI binding
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Validates the profile configuration
        /// </summary>
        /// <returns>True if the profile is valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   FolderMappings.Count > 0 &&
                   FolderMappings.All(m => m.IsValid);
        }

        /// <summary>
        /// Gets validation errors for the profile
        /// </summary>
        /// <returns>List of validation error messages</returns>
        public System.Collections.Generic.List<string> GetValidationErrors()
        {
            var errors = new System.Collections.Generic.List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Profile name is required");

            if (FolderMappings.Count == 0)
                errors.Add("At least one folder mapping is required");

            for (int i = 0; i < FolderMappings.Count; i++)
            {
                var mapping = FolderMappings[i];
                if (!mapping.IsValid)
                    errors.Add($"Folder mapping {i + 1} has invalid paths");
            }

            return errors;
        }

        /// <summary>
        /// Checks if this profile is a duplicate of another profile
        /// </summary>
        public bool IsDuplicate(TransferProfile other)
        {
            if (other == null) return false;

            // Compare name (case-insensitive)
            if (!string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            // Compare folder mappings count
            if (FolderMappings.Count != other.FolderMappings.Count)
                return false;

            // Compare each folder mapping
            foreach (var mapping in FolderMappings)
            {
                if (!other.FolderMappings.Any(m => m.IsEqualTo(mapping)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Migrates from old format (SourceFolders + DestinationFolder) to new format (FolderMappings)
        /// </summary>
        public void MigrateFromLegacyFormat()
        {
            // If already has mappings, no need to migrate
            if (FolderMappings?.Any() == true)
                return;

            // If has legacy data, migrate it
            if (_sourceFolders?.Any() == true && !string.IsNullOrWhiteSpace(_destinationFolder))
            {
                FolderMappings = _sourceFolders
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(source => new FolderMapping
                    {
                        Id = Guid.NewGuid(),
                        SourcePath = source,
                        DestinationPath = _destinationFolder!
                    })
                    .ToList();

                // Clear legacy data
                _sourceFolders = null;
                _destinationFolder = null;
            }
        }

        /// <summary>
        /// Creates a deep copy of the profile
        /// </summary>
        /// <returns>A new TransferProfile instance with the same values</returns>
        public TransferProfile Clone()
        {
            return new TransferProfile
            {
                Id = Id,
                Name = Name,
                FolderMappings = FolderMappings.Select(m => m.Clone()).ToList(),
                CreatedDate = CreatedDate,
                LastModified = LastModified,
                OverwriteExisting = OverwriteExisting
            };
        }

        /// <summary>
        /// Updates the LastModified timestamp
        /// </summary>
        public void Touch()
        {
            LastModified = System.DateTime.UtcNow;
            OnPropertyChanged(nameof(LastModified));
        }
    }
}