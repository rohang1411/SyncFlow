using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using SyncFlow.ViewModels;
using SyncFlow.Models;
using SyncFlow.Services;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;

namespace SyncFlow.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : FluentWindow
    {
        private readonly SettingsViewModel _viewModel;
        private readonly ILogger<SettingsWindow>? _logger;

        public SettingsWindow(AppSettings settings, IThemeService themeService, IDialogService dialogService, IProfileService profileService, ILogger<SettingsWindow>? logger = null, ILogger<SettingsViewModel>? viewModelLogger = null)
        {
            _logger = logger;
            
            try
            {
                // Validate resources before initializing component
                ValidateRequiredResources();
                
                InitializeComponent();

                _viewModel = new SettingsViewModel(settings, themeService, dialogService, profileService, viewModelLogger);
                _viewModel.SaveRequested += OnSaveRequested;
                _viewModel.CancelRequested += OnCancelRequested;

                DataContext = _viewModel;
                
                _logger?.LogInformation("SettingsWindow initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize SettingsWindow");
                
                // Apply fallback styling if resource loading fails
                ApplyFallbackStyling();
                
                // Still try to initialize the component with fallback resources
                try
                {
                    InitializeComponent();
                    _viewModel = new SettingsViewModel(settings, themeService, dialogService, profileService, viewModelLogger);
                    _viewModel.SaveRequested += OnSaveRequested;
                    _viewModel.CancelRequested += OnCancelRequested;
                    DataContext = _viewModel;
                }
                catch (Exception innerEx)
                {
                    _logger?.LogError(innerEx, "Failed to initialize SettingsWindow even with fallback styling");
                    throw;
                }
            }
        }

        private void ValidateRequiredResources()
        {
            var requiredResources = new List<string>
            {
                "WindowBackgroundBrush",
                "CardBackgroundBrush",
                "PrimaryTextBrush",
                "SecondaryTextBrush",
                "BorderBrush",
                "PrimaryButtonStyle",
                "SecondaryButtonStyle",
                "ModernTextBoxStyle",
                "AnimatedToggleButtonStyle"
            };

            var missingResources = new List<string>();

            foreach (var resourceKey in requiredResources)
            {
                try
                {
                    var resource = Application.Current.TryFindResource(resourceKey);
                    if (resource == null)
                    {
                        missingResources.Add(resourceKey);
                        _logger?.LogWarning("Missing resource: {ResourceKey}", resourceKey);
                    }
                }
                catch (Exception ex)
                {
                    missingResources.Add(resourceKey);
                    _logger?.LogError(ex, "Error accessing resource: {ResourceKey}", resourceKey);
                }
            }

            if (missingResources.Count > 0)
            {
                _logger?.LogWarning("Found {Count} missing resources: {Resources}", 
                    missingResources.Count, string.Join(", ", missingResources));
                
                // Create fallback resources
                CreateFallbackResources(missingResources);
            }
        }

        private void CreateFallbackResources(List<string> missingResources)
        {
            var resources = Application.Current.Resources;

            foreach (var resourceKey in missingResources)
            {
                try
                {
                    switch (resourceKey)
                    {
                        case "WindowBackgroundBrush":
                            resources[resourceKey] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                            break;
                        case "CardBackgroundBrush":
                            resources[resourceKey] = new SolidColorBrush(Color.FromRgb(37, 37, 38));
                            break;
                        case "PrimaryTextBrush":
                            resources[resourceKey] = new SolidColorBrush(Colors.White);
                            break;
                        case "SecondaryTextBrush":
                            resources[resourceKey] = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                            break;
                        case "BorderBrush":
                            resources[resourceKey] = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                            break;
                        case "PrimaryButtonStyle":
                        case "SecondaryButtonStyle":
                        case "ModernTextBoxStyle":
                        case "AnimatedToggleButtonStyle":
                            // For styles, we'll create basic fallback styles
                            CreateFallbackStyle(resourceKey);
                            break;
                    }
                    
                    _logger?.LogInformation("Created fallback resource: {ResourceKey}", resourceKey);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to create fallback resource: {ResourceKey}", resourceKey);
                }
            }
        }

        private void CreateFallbackStyle(string styleKey)
        {
            var resources = Application.Current.Resources;

            try
            {
                switch (styleKey)
                {
                    case "PrimaryButtonStyle":
                        var primaryButtonStyle = new Style(typeof(System.Windows.Controls.Button));
                        primaryButtonStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(0, 120, 212))));
                        primaryButtonStyle.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Colors.White)));
                        primaryButtonStyle.Setters.Add(new Setter(PaddingProperty, new Thickness(16, 8, 16, 8)));
                        resources[styleKey] = primaryButtonStyle;
                        break;

                    case "SecondaryButtonStyle":
                        var secondaryButtonStyle = new Style(typeof(System.Windows.Controls.Button));
                        secondaryButtonStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 45, 45))));
                        secondaryButtonStyle.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Colors.White)));
                        secondaryButtonStyle.Setters.Add(new Setter(PaddingProperty, new Thickness(16, 8, 16, 8)));
                        resources[styleKey] = secondaryButtonStyle;
                        break;

                    case "ModernTextBoxStyle":
                        var textBoxStyle = new Style(typeof(System.Windows.Controls.TextBox));
                        textBoxStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 45, 45))));
                        textBoxStyle.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Colors.White)));
                        textBoxStyle.Setters.Add(new Setter(PaddingProperty, new Thickness(8, 4, 8, 4)));
                        resources[styleKey] = textBoxStyle;
                        break;

                    case "AnimatedToggleButtonStyle":
                        var toggleButtonStyle = new Style(typeof(System.Windows.Controls.Primitives.ToggleButton));
                        toggleButtonStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 45, 45))));
                        toggleButtonStyle.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Colors.White)));
                        resources[styleKey] = toggleButtonStyle;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create fallback style: {StyleKey}", styleKey);
            }
        }

        private void ApplyFallbackStyling()
        {
            try
            {
                // Apply basic fallback styling directly to the window
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                Foreground = new SolidColorBrush(Colors.White);
                
                _logger?.LogInformation("Applied fallback styling to SettingsWindow");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to apply fallback styling");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                base.OnSourceInitialized(e);
                
                // Additional resource validation after window is fully initialized
                ValidateWindowResources();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during SourceInitialized");
                // Don't rethrow - window should still be usable
            }
        }

        private void ValidateWindowResources()
        {
            try
            {
                // Check if all visual elements have proper styling
                if (Background == null)
                {
                    Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    _logger?.LogWarning("Applied fallback background to SettingsWindow");
                }

                // Validate that the window can be displayed properly
                if (double.IsNaN(Width) || Width <= 0)
                {
                    Width = 550;
                    _logger?.LogWarning("Applied fallback width to SettingsWindow");
                }

                if (double.IsNaN(Height) || Height <= 0)
                {
                    Height = 600;
                    _logger?.LogWarning("Applied fallback height to SettingsWindow");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating window resources");
            }
        }

        private void OnSaveRequested(object? sender, EventArgs e)
        {
            try
            {
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error handling save request");
                // Still try to close the window
                try
                {
                    Close();
                }
                catch
                {
                    // If we can't close normally, hide the window
                    Hide();
                }
            }
        }

        private void OnCancelRequested(object? sender, EventArgs e)
        {
            try
            {
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error handling cancel request");
                // Still try to close the window
                try
                {
                    Close();
                }
                catch
                {
                    // If we can't close normally, hide the window
                    Hide();
                }
            }
        }

        private void OnSettingCardClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Find the ToggleButton within the clicked Border
                if (sender is System.Windows.Controls.Border border)
                {
                    var toggleButton = FindVisualChild<System.Windows.Controls.Primitives.ToggleButton>(border);
                    if (toggleButton != null)
                    {
                        // Toggle the button
                        toggleButton.IsChecked = !toggleButton.IsChecked;
                        e.Handled = true;
                        _logger?.LogDebug("Toggled setting via card click: {Name}", toggleButton.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error handling setting card click");
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }
    }
}

