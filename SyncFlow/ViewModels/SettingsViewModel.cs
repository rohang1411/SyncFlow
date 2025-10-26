using System;
using System.Windows.Input;
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

        private bool _isDarkMode;
        private bool _isTransparencyEnabled;
        private bool _enableGlassEffect;
        private string _profileStoragePath;

        public SettingsViewModel(AppSettings settings, IThemeService themeService, IDialogService dialogService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Initialize from settings
            _isDarkMode = _settings.IsDarkMode;
            _isTransparencyEnabled = _settings.IsTransparencyEnabled;
            _enableGlassEffect = _settings.EnableGlassEffect;
            _profileStoragePath = _settings.ProfileStoragePath;

            // Initialize commands
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
            BrowseStoragePathCommand = new RelayCommand(ExecuteBrowseStoragePath);
        }

        #region Properties

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetProperty(ref _isDarkMode, value))
                {
                    // Apply theme immediately for preview
                    _themeService.ApplyTheme(value ? "Dark" : "Light");
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
                    // Apply effects immediately for preview
                    _themeService.ApplyVisualEffects();
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
                    // Apply effects immediately for preview
                    _themeService.ApplyVisualEffects();
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

        public event EventHandler? SaveRequested;
        public event EventHandler? CancelRequested;

        #endregion

        #region Command Implementations

        private void ExecuteSave(object? parameter)
        {
            // Save settings
            _settings.IsDarkMode = IsDarkMode;
            _settings.IsTransparencyEnabled = IsTransparencyEnabled;
            _settings.EnableGlassEffect = EnableGlassEffect;
            _settings.ProfileStoragePath = ProfileStoragePath;

            // Apply final effects
            _themeService.ApplyVisualEffects();

            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteCancel(object? parameter)
        {
            // Revert all settings if cancelled
            _themeService.ApplyTheme(_settings.IsDarkMode ? "Dark" : "Light");
            _themeService.ApplyVisualEffects();

            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteBrowseStoragePath(object? parameter)
        {
            var selectedPath = _dialogService.SelectDirectory("Select Profile Storage Location");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                ProfileStoragePath = selectedPath;
            }
        }

        #endregion
    }
}

