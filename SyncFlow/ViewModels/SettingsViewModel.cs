using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using SyncFlow.Commands;
using SyncFlow.Models;
using SyncFlow.Services;

namespace SyncFlow.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings dialog
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AppSettings _settings;
        private readonly IThemeService _themeService;
        private readonly IDialogService _dialogService;
        private readonly IProfileService _profileService;
        private readonly ILogger<SettingsViewModel>? _logger;

        private bool _isDarkMode;
        private bool _isTransparencyEnabled;
        private bool _enableGlassEffect;
        private double _transparencyAmount;
        private bool _enableAnimations;
        private string _profileStoragePath;

        public SettingsViewModel(AppSettings settings, IThemeService themeService, IDialogService dialogService, IProfileService profileService, ILogger<SettingsViewModel>? logger = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _logger = logger;

            // Initialize from settings
            _isDarkMode = _settings.IsDarkMode;
            _isTransparencyEnabled = _settings.IsTransparencyEnabled;
            _enableGlassEffect = _settings.EnableGlassEffect;
            _transparencyAmount = _settings.TransparencyAmount;
            _enableAnimations = _settings.EnableAnimations;
            _profileStoragePath = _settings.ProfileStoragePath;

            // Initialize commands
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
            BrowseStoragePathCommand = new RelayCommand(ExecuteBrowseStoragePath);
            ExportProfilesCommand = new AsyncRelayCommand(ExecuteExportProfiles);
            ImportProfilesCommand = new AsyncRelayCommand(ExecuteImportProfiles);
        }

        #region Properties

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetProperty(ref _isDarkMode, value))
                {
                    // Update settings immediately for preview
                    _settings.IsDarkMode = value;
                    _themeService.ApplyTheme(value ? "Dark" : "Light");
                    _logger?.LogInformation("Theme changed to: {Theme}", value ? "Dark" : "Light");
                }
            }
        }

        public bool IsTransparencyEnabled
        {
            get => _isTransparencyEnabled;
            set
            {
                if (SetProperty(ref _isTransparencyEnabled, value))
                {
                    // Mutual exclusivity: disable glass effect if transparency is enabled
                    if (value && _enableGlassEffect)
                    {
                        _logger?.LogInformation("Disabling glass effect due to transparency being enabled");
                        EnableGlassEffect = false;
                    }

                    // Update settings immediately for preview
                    _settings.IsTransparencyEnabled = value;
                    _themeService.ApplyVisualEffects();
                    OnPropertyChanged(nameof(IsTransparencySliderEnabled));
                    _logger?.LogInformation("Transparency enabled: {Enabled}", value);
                }
            }
        }

        public bool EnableGlassEffect
        {
            get => _enableGlassEffect;
            set
            {
                if (SetProperty(ref _enableGlassEffect, value))
                {
                    // Mutual exclusivity: disable transparency if glass effect is enabled
                    if (value && _isTransparencyEnabled)
                    {
                        _logger?.LogInformation("Disabling transparency due to glass effect being enabled");
                        IsTransparencyEnabled = false;
                    }

                    // Update settings immediately for preview
                    _settings.EnableGlassEffect = value;
                    _themeService.ApplyVisualEffects();
                    _logger?.LogInformation("Glass effect enabled: {Enabled}", value);
                }
            }
        }

        public double TransparencyAmount
        {
            get => _transparencyAmount;
            set
            {
                if (SetProperty(ref _transparencyAmount, value))
                {
                    // Update settings immediately for preview
                    _settings.TransparencyAmount = value;
                    if (_isTransparencyEnabled)
                    {
                        _themeService.ApplyVisualEffects();
                    }
                    _logger?.LogDebug("Transparency amount changed to: {Amount}", value);
                }
            }
        }

        public bool IsTransparencySliderEnabled => IsTransparencyEnabled;

        public bool EnableAnimations
        {
            get => _enableAnimations;
            set
            {
                if (SetProperty(ref _enableAnimations, value))
                {
                    // Update settings immediately
                    _settings.EnableAnimations = value;
                    _logger?.LogInformation("Animations enabled: {Enabled}", value);
                }
            }
        }

        public string ProfileStoragePath
        {
            get => _profileStoragePath;
            set => SetProperty(ref _profileStoragePath, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseStoragePathCommand { get; }
        public ICommand ExportProfilesCommand { get; }
        public ICommand ImportProfilesCommand { get; }

        public event EventHandler? SaveRequested;
        public event EventHandler? CancelRequested;

        #endregion

        #region Command Implementations

        private void ExecuteSave(object? parameter)
        {
            try
            {
                // Save settings
                _settings.IsDarkMode = IsDarkMode;
                _settings.IsTransparencyEnabled = IsTransparencyEnabled;
                _settings.EnableGlassEffect = EnableGlassEffect;
                _settings.TransparencyAmount = TransparencyAmount;
                _settings.EnableAnimations = EnableAnimations;
                _settings.ProfileStoragePath = ProfileStoragePath;

                // Apply final effects
                _themeService.ApplyVisualEffects();

                _logger?.LogInformation("Settings saved successfully");
                SaveRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save settings");
                _dialogService.ShowError($"Failed to save settings: {ex.Message}", "Save Error");
            }
        }

        private void ExecuteCancel(object? parameter)
        {
            try
            {
                // Revert all settings if cancelled
                _themeService.ApplyTheme(_settings.IsDarkMode ? "Dark" : "Light");
                _themeService.ApplyVisualEffects();

                _logger?.LogInformation("Settings cancelled, reverted to previous state");
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to cancel settings");
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ExecuteBrowseStoragePath(object? parameter)
        {
            try
            {
                var selectedPath = _dialogService.SelectDirectory("Select Profile Storage Location");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    ProfileStoragePath = selectedPath;
                    _logger?.LogInformation("Profile storage path changed to: {Path}", selectedPath);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to browse storage path");
                _dialogService.ShowError($"Failed to select directory: {ex.Message}", "Browse Error");
            }
        }

        private async Task ExecuteExportProfiles()
        {
            try
            {
                _logger?.LogInformation("Starting profile export");
                
                var filePath = _dialogService.SaveFile("Export Profiles", "JSON Files|*.json", "syncflow-profiles.json");
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger?.LogInformation("Profile export cancelled by user");
                    return;
                }

                var profiles = await _profileService.GetAllProfilesAsync();
                var profileList = profiles.ToList();
                
                if (!profileList.Any())
                {
                    _dialogService.ShowWarning("No profiles to export.", "Export Profiles");
                    _logger?.LogWarning("No profiles available to export");
                    return;
                }

                var json = await _profileService.ExportProfilesAsync(profileList);
                await File.WriteAllTextAsync(filePath, json);

                _dialogService.ShowSuccess($"Successfully exported {profileList.Count} profile(s) to {Path.GetFileName(filePath)}", "Export Complete");
                _logger?.LogInformation("Successfully exported {Count} profiles to {Path}", profileList.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to export profiles");
                _dialogService.ShowError($"Failed to export profiles: {ex.Message}", "Export Failed");
            }
        }

        private async Task ExecuteImportProfiles()
        {
            try
            {
                _logger?.LogInformation("Starting profile import");
                
                var filePath = _dialogService.SelectFile("Import Profiles", "JSON Files|*.json");
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger?.LogInformation("Profile import cancelled by user");
                    return;
                }

                if (!File.Exists(filePath))
                {
                    _dialogService.ShowError($"File not found: {filePath}", "Import Failed");
                    _logger?.LogError("Import file not found: {Path}", filePath);
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                await _profileService.ImportProfilesAsync(json);

                _dialogService.ShowSuccess($"Successfully imported profiles from {Path.GetFileName(filePath)}", "Import Complete");
                _logger?.LogInformation("Successfully imported profiles from {Path}", filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to import profiles");
                _dialogService.ShowError($"Failed to import profiles: {ex.Message}", "Import Failed");
            }
        }

        #endregion
    }
}
