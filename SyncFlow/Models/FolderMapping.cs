using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SyncFlow.Models;

/// <summary>
/// Represents a mapping between a source folder and a destination folder
/// </summary>
public class FolderMapping : INotifyPropertyChanged
{
    private string _sourcePath = string.Empty;
    private string _destinationPath = string.Empty;

    /// <summary>
    /// Unique identifier for this mapping
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Source folder path
    /// </summary>
    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (_sourcePath != value)
            {
                _sourcePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    /// <summary>
    /// Destination folder path
    /// </summary>
    public string DestinationPath
    {
        get => _destinationPath;
        set
        {
            if (_destinationPath != value)
            {
                _destinationPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    /// <summary>
    /// Indicates whether this mapping is valid
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(SourcePath) && 
                           !string.IsNullOrWhiteSpace(DestinationPath);

    /// <summary>
    /// Display text for UI binding
    /// </summary>
    public string DisplayText => $"{SourcePath} → {DestinationPath}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Creates a deep copy of this folder mapping
    /// </summary>
    public FolderMapping Clone()
    {
        return new FolderMapping
        {
            Id = Id,
            SourcePath = SourcePath,
            DestinationPath = DestinationPath
        };
    }

    /// <summary>
    /// Checks if this mapping is equal to another mapping
    /// </summary>
    public bool IsEqualTo(FolderMapping other)
    {
        if (other == null) return false;
        return string.Equals(SourcePath, other.SourcePath, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(DestinationPath, other.DestinationPath, StringComparison.OrdinalIgnoreCase);
    }
}
