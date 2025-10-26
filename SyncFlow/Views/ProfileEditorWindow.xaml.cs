using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using SyncFlow.Models;
using SyncFlow.Services;
using Wpf.Ui.Controls;
using MessageBoxResult = System.Windows.MessageBoxResult;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;

namespace SyncFlow.Views;

public partial class ProfileEditorWindow : FluentWindow
{
    private readonly TransferProfile _profile;
    private readonly IDialogService _dialogService;
    private readonly ObservableCollection<FolderMapping> _mappings;
    private readonly ILogger<ProfileEditorWindow>? _logger;

    public TransferProfile? EditedProfile { get; private set; }

    public ProfileEditorWindow(TransferProfile profile, IDialogService dialogService, ILogger<ProfileEditorWindow>? logger = null)
    {
        _logger = logger;
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        try
        {
            // Configure window for backdrop effects BEFORE InitializeComponent
            ConfigureWindowForBackdropEffects();
            
            InitializeComponent();

            // Initialize mappings collection
            _mappings = new ObservableCollection<FolderMapping>(
                profile.FolderMappings.Select(m => m.Clone()));

            // Initialize controls
            NameTextBox.Text = profile.Name;
            MappingsItemsControl.ItemsSource = _mappings;
            OverwriteCheckBox.IsChecked = profile.OverwriteExisting;
            
            _logger?.LogInformation("ProfileEditorWindow initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize ProfileEditorWindow");
            
            // Apply fallback configuration
            ApplyFallbackConfiguration();
            
            try
            {
                InitializeComponent();
                
                _mappings = new ObservableCollection<FolderMapping>(
                    profile.FolderMappings.Select(m => m.Clone()));
                NameTextBox.Text = profile.Name;
                MappingsItemsControl.ItemsSource = _mappings;
                OverwriteCheckBox.IsChecked = profile.OverwriteExisting;
            }
            catch (Exception innerEx)
            {
                _logger?.LogError(innerEx, "Failed to initialize ProfileEditorWindow even with fallback configuration");
                throw;
            }
        }
    }

    private void ConfigureWindowForBackdropEffects()
    {
        try
        {
            // Set ExtendsContentIntoTitleBar before applying backdrop effects
            // This is already set in XAML, but we ensure it's set programmatically as well
            ExtendsContentIntoTitleBar = true;
            
            // Validate that required resources exist
            ValidateBackdropResources();
            
            _logger?.LogInformation("Window configured for backdrop effects");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to configure window for backdrop effects");
            // Don't rethrow - fallback will be applied
        }
    }

    private void ValidateBackdropResources()
    {
        try
        {
            // Check if WindowBackgroundBrush exists
            var backgroundBrush = Application.Current.TryFindResource("WindowBackgroundBrush");
            if (backgroundBrush == null)
            {
                _logger?.LogWarning("WindowBackgroundBrush not found, creating fallback");
                Application.Current.Resources["WindowBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error validating backdrop resources");
        }
    }

    private void ApplyFallbackConfiguration()
    {
        try
        {
            // Disable backdrop effects if they're causing issues
            WindowBackdropType = Wpf.Ui.Controls.WindowBackdropType.None;
            ExtendsContentIntoTitleBar = false;
            
            // Apply basic styling
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            Foreground = new SolidColorBrush(Colors.White);
            
            _logger?.LogInformation("Applied fallback configuration to ProfileEditorWindow");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to apply fallback configuration");
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        try
        {
            base.OnSourceInitialized(e);
            
            // Additional backdrop effect validation after window is fully initialized
            ValidateBackdropEffectApplication();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during SourceInitialized, applying fallback");
            
            // If backdrop effects fail, disable them gracefully
            try
            {
                WindowBackdropType = Wpf.Ui.Controls.WindowBackdropType.None;
                _logger?.LogInformation("Disabled backdrop effects due to initialization error");
            }
            catch (Exception fallbackEx)
            {
                _logger?.LogError(fallbackEx, "Failed to disable backdrop effects");
            }
        }
    }

    private void ValidateBackdropEffectApplication()
    {
        try
        {
            // Check if backdrop effect was applied successfully
            if (WindowBackdropType != Wpf.Ui.Controls.WindowBackdropType.None && !ExtendsContentIntoTitleBar)
            {
                _logger?.LogWarning("Backdrop effect enabled but ExtendsContentIntoTitleBar is false, correcting...");
                ExtendsContentIntoTitleBar = true;
            }
            
            // Ensure window has proper background
            if (Background == null)
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                _logger?.LogWarning("Applied fallback background to ProfileEditorWindow");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error validating backdrop effect application");
        }
    }

    private void OnAddMapping(object sender, RoutedEventArgs e)
    {
        // Select source folder
        var sourceFolder = _dialogService.SelectDirectory("Select Source Folder");
        if (string.IsNullOrEmpty(sourceFolder))
            return;

        // Select destination folder
        var destinationFolder = _dialogService.SelectDirectory("Select Destination Folder");
        if (string.IsNullOrEmpty(destinationFolder))
            return;

        // Add new mapping
        var mapping = new FolderMapping
        {
            Id = Guid.NewGuid(),
            SourcePath = sourceFolder,
            DestinationPath = destinationFolder
        };

        _mappings.Add(mapping);
    }

    private void OnRemoveMapping(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is FolderMapping mapping)
        {
            var result = _dialogService.ShowConfirmation(
                $"Are you sure you want to remove this mapping?\n\n{mapping.SourcePath} → {mapping.DestinationPath}",
                "Confirm Remove");

            if (result == MessageBoxResult.Yes)
            {
                _mappings.Remove(mapping);
            }
        }
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate profile name
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                _dialogService.ShowError("Profile name is required.", "Validation Error");
                return;
            }

            // Validate mappings
            if (_mappings.Count == 0)
            {
                _dialogService.ShowError("At least one folder mapping is required.", "Validation Error");
                return;
            }

            // Check for invalid mappings
            var invalidMappings = _mappings.Where(m => !m.IsValid).ToList();
            if (invalidMappings.Any())
            {
                _dialogService.ShowError("All folder mappings must have valid source and destination paths.", "Validation Error");
                return;
            }

            // Update profile
            _profile.Name = NameTextBox.Text;
            _profile.FolderMappings = _mappings.ToList();
            _profile.OverwriteExisting = OverwriteCheckBox.IsChecked ?? false;
            _profile.Touch();

            EditedProfile = _profile;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving profile");
            _dialogService.ShowError($"An error occurred while saving the profile: {ex.Message}", "Save Error");
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        try
        {
            DialogResult = false;
            Close();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error canceling profile editor");
            // Still try to close the window
            try
            {
                Hide();
            }
            catch
            {
                // If we can't even hide, just log the error
                _logger?.LogError("Failed to close or hide ProfileEditorWindow");
            }
        }
    }
}
