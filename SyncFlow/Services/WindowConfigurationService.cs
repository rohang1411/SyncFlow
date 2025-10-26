using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using SyncFlow.Views;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;
using Control = System.Windows.Controls.Control;

namespace SyncFlow.Services
{
    public class WindowConfigurationService : IWindowConfigurationService
    {
        private readonly ILogger<WindowConfigurationService> _logger;
        private static readonly Dictionary<string, object> FallbackResources = new();

        public WindowConfigurationService(ILogger<WindowConfigurationService> logger)
        {
            _logger = logger;
            InitializeFallbackResources();
        }

        public void ConfigureSettingsWindow(SettingsWindow window)
        {
            try
            {
                _logger.LogInformation("Configuring SettingsWindow");
                
                // Validate and apply resources
                ValidateAndApplyWindowResources(window);
                
                // Configure backdrop effects if supported
                if (window is FluentWindow fluentWindow)
                {
                    ConfigureBackdropEffects(fluentWindow);
                }
                
                _logger.LogInformation("SettingsWindow configured successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure SettingsWindow, applying fallback");
                ApplyFallbackStyling(window);
            }
        }

        public void ConfigureProfileEditor(ProfileEditorWindow window)
        {
            try
            {
                _logger.LogInformation("Configuring ProfileEditorWindow");
                
                // Validate and apply resources
                ValidateAndApplyWindowResources(window);
                
                // Configure backdrop effects
                ConfigureBackdropEffects(window);
                
                _logger.LogInformation("ProfileEditorWindow configured successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure ProfileEditorWindow, applying fallback");
                ApplyFallbackStyling(window);
            }
        }

        public bool ValidateResourceBindings(FrameworkElement element)
        {
            try
            {
                var requiredResources = new[]
                {
                    "WindowBackgroundBrush",
                    "CardBackgroundBrush",
                    "PrimaryTextBrush",
                    "SecondaryTextBrush",
                    "BorderBrush",
                    "PrimaryButtonStyle",
                    "SecondaryButtonStyle",
                    "ModernTextBoxStyle"
                };

                var missingResources = new List<string>();

                foreach (var resourceKey in requiredResources)
                {
                    var resource = element.TryFindResource(resourceKey) ?? Application.Current.TryFindResource(resourceKey);
                    if (resource == null)
                    {
                        missingResources.Add(resourceKey);
                        _logger.LogWarning("Missing resource: {ResourceKey}", resourceKey);
                    }
                }

                if (missingResources.Count > 0)
                {
                    CreateMissingResources(missingResources);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating resource bindings");
                return false;
            }
        }

        public void ApplyFallbackStyling(Window window)
        {
            try
            {
                _logger.LogInformation("Applying fallback styling to window: {WindowType}", window.GetType().Name);
                
                // Apply basic window styling
                window.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                window.Foreground = new SolidColorBrush(Colors.White);
                
                // Disable backdrop effects if it's a FluentWindow
                if (window is FluentWindow fluentWindow)
                {
                    fluentWindow.WindowBackdropType = WindowBackdropType.None;
                    fluentWindow.ExtendsContentIntoTitleBar = false;
                }
                
                // Ensure window dimensions are valid
                if (double.IsNaN(window.Width) || window.Width <= 0)
                {
                    window.Width = 600;
                }
                
                if (double.IsNaN(window.Height) || window.Height <= 0)
                {
                    window.Height = 500;
                }
                
                _logger.LogInformation("Fallback styling applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply fallback styling");
            }
        }

        public void ConfigureBackdropEffects(FluentWindow window)
        {
            try
            {
                if (!IsBackdropEffectSupported())
                {
                    _logger.LogInformation("Backdrop effects not supported, using fallback");
                    window.WindowBackdropType = WindowBackdropType.None;
                    return;
                }

                // Ensure ExtendsContentIntoTitleBar is set before applying backdrop
                if (window.WindowBackdropType != WindowBackdropType.None && !window.ExtendsContentIntoTitleBar)
                {
                    _logger.LogInformation("Setting ExtendsContentIntoTitleBar for backdrop effects");
                    window.ExtendsContentIntoTitleBar = true;
                }

                // Validate background brush exists
                var backgroundBrush = window.TryFindResource("WindowBackgroundBrush") ?? 
                                    Application.Current.TryFindResource("WindowBackgroundBrush");
                
                if (backgroundBrush == null)
                {
                    _logger.LogWarning("WindowBackgroundBrush not found, creating fallback");
                    var fallbackBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    Application.Current.Resources["WindowBackgroundBrush"] = fallbackBrush;
                    window.Background = fallbackBrush;
                }

                _logger.LogInformation("Backdrop effects configured for window: {WindowType}", window.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure backdrop effects, disabling them");
                
                try
                {
                    window.WindowBackdropType = WindowBackdropType.None;
                    window.ExtendsContentIntoTitleBar = false;
                    window.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Failed to disable backdrop effects");
                }
            }
        }

        public bool IsBackdropEffectSupported()
        {
            try
            {
                // Check Windows version and other requirements for backdrop effects
                var osVersion = Environment.OSVersion.Version;
                
                // Windows 11 (build 22000) and later support Mica backdrop
                if (osVersion.Major >= 10 && osVersion.Build >= 22000)
                {
                    return true;
                }
                
                // Windows 10 with certain updates supports Acrylic
                if (osVersion.Major >= 10 && osVersion.Build >= 17134)
                {
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking backdrop effect support");
                return false;
            }
        }

        private void ValidateAndApplyWindowResources(Window window)
        {
            try
            {
                // Ensure all required resources are available
                if (!ValidateResourceBindings(window))
                {
                    _logger.LogWarning("Some resources were missing and have been created as fallbacks");
                }

                // Apply window-specific configurations
                if (window.Background == null)
                {
                    var backgroundBrush = window.TryFindResource("WindowBackgroundBrush") as Brush ??
                                        Application.Current.TryFindResource("WindowBackgroundBrush") as Brush ??
                                        new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    window.Background = backgroundBrush;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating and applying window resources");
            }
        }

        private void CreateMissingResources(List<string> missingResources)
        {
            var resources = Application.Current.Resources;

            foreach (var resourceKey in missingResources)
            {
                try
                {
                    if (FallbackResources.TryGetValue(resourceKey, out var fallbackResource))
                    {
                        resources[resourceKey] = fallbackResource;
                        _logger.LogInformation("Created fallback resource: {ResourceKey}", resourceKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create fallback resource: {ResourceKey}", resourceKey);
                }
            }
        }

        private void InitializeFallbackResources()
        {
            try
            {
                // Initialize fallback brushes
                FallbackResources["WindowBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                FallbackResources["CardBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(37, 37, 38));
                FallbackResources["PrimaryTextBrush"] = new SolidColorBrush(Colors.White);
                FallbackResources["SecondaryTextBrush"] = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                FallbackResources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(51, 51, 51));

                // Initialize fallback styles
                var primaryButtonStyle = new Style(typeof(System.Windows.Controls.Button));
                primaryButtonStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0, 120, 212))));
                primaryButtonStyle.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.White)));
                primaryButtonStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(16, 8, 16, 8)));
                FallbackResources["PrimaryButtonStyle"] = primaryButtonStyle;

                var secondaryButtonStyle = new Style(typeof(System.Windows.Controls.Button));
                secondaryButtonStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 45, 45))));
                secondaryButtonStyle.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.White)));
                secondaryButtonStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(16, 8, 16, 8)));
                FallbackResources["SecondaryButtonStyle"] = secondaryButtonStyle;

                var textBoxStyle = new Style(typeof(System.Windows.Controls.TextBox));
                textBoxStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 45, 45))));
                textBoxStyle.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.White)));
                textBoxStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 4, 8, 4)));
                FallbackResources["ModernTextBoxStyle"] = textBoxStyle;

                _logger.LogInformation("Fallback resources initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize fallback resources");
            }
        }
    }
}