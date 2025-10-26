using Microsoft.Extensions.Logging;
using System.Windows;
using SyncFlow.Models;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;

namespace SyncFlow.Services;

/// <summary>
/// Service for managing application themes and visual effects
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private readonly AppSettings _appSettings;
    private string _currentTheme = "Dark";

    public ThemeService(ILogger<ThemeService> logger, AppSettings appSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        
        // Subscribe to settings changes
        _appSettings.PropertyChanged += OnAppSettingsChanged;
    }

    public string CurrentTheme => _currentTheme;

    public bool IsDarkTheme => _currentTheme == "Dark";

    public event EventHandler<string>? ThemeChanged;

    public void ApplyTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
            throw new ArgumentException("Theme name cannot be null or empty", nameof(themeName));

        if (_currentTheme == themeName)
            return;

        try
        {
            var app = Application.Current;
            if (app?.Resources == null)
            {
                _logger.LogWarning("Application resources not available for theme change");
                return;
            }

            // Clear existing theme resources
            var resourcesToRemove = app.Resources.MergedDictionaries
                .Where(d => d.Source?.OriginalString?.Contains("Theme.xaml") == true)
                .ToList();

            foreach (var resource in resourcesToRemove)
            {
                app.Resources.MergedDictionaries.Remove(resource);
            }

            // Load new theme
            var themeUri = new Uri($"Styles/{themeName}Theme.xaml", UriKind.Relative);
            var themeDict = new ResourceDictionary { Source = themeUri };
            app.Resources.MergedDictionaries.Add(themeDict);

            _currentTheme = themeName;
            _logger.LogInformation("Theme changed to: {ThemeName}", themeName);
            
            ThemeChanged?.Invoke(this, themeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply theme: {ThemeName}", themeName);
            throw;
        }
    }

    public void ToggleTheme()
    {
        var newTheme = IsDarkTheme ? "Light" : "Dark";
        ApplyTheme(newTheme);
    }

    public void ApplyVisualEffects()
    {
        try
        {
            var app = Application.Current;
            if (app?.MainWindow is FluentWindow mainWindow)
            {
                ApplyWindowEffects(mainWindow);
            }

            // Apply to all open FluentWindows
            foreach (Window window in app?.Windows ?? new WindowCollection())
            {
                if (window is FluentWindow fluentWindow && window != app.MainWindow)
                {
                    ApplyWindowEffects(fluentWindow);
                }
            }

            _logger.LogInformation("Visual effects applied - Transparency: {Transparency}, Glass: {Glass}", 
                _appSettings.IsTransparencyEnabled, _appSettings.EnableGlassEffect);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply visual effects");
        }
    }

    private void ApplyWindowEffects(FluentWindow window)
    {
        try
        {
            _logger.LogInformation("Applying window effects - Transparency: {Transparency}, Glass: {Glass}, Amount: {Amount}", 
                _appSettings.IsTransparencyEnabled, _appSettings.EnableGlassEffect, _appSettings.TransparencyAmount);

            // Ensure ExtendsContentIntoTitleBar is set before applying backdrop
            if (!window.ExtendsContentIntoTitleBar)
            {
                window.ExtendsContentIntoTitleBar = true;
            }

            // Calculate opacity from transparency amount (10-100 -> 0.90-1.00)
            double opacity = 1.0 - (_appSettings.TransparencyAmount / 1000.0);
            opacity = Math.Max(0.90, Math.Min(1.0, opacity));

            // Mutual exclusivity is enforced in SettingsViewModel, but double-check here
            if (_appSettings.IsTransparencyEnabled && _appSettings.EnableGlassEffect)
            {
                _logger.LogWarning("Both transparency and glass effect are enabled - this should not happen. Disabling glass effect.");
                _appSettings.EnableGlassEffect = false;
            }

            if (_appSettings.EnableGlassEffect)
            {
                // Enable Glass/Mica effect (Windows 11)
                try
                {
                    window.WindowBackdropType = WindowBackdropType.Mica;
                    window.Opacity = 1.0; // Glass effect doesn't use opacity
                    _logger.LogInformation("Applied Mica (glass) effect to window: {WindowTitle}", window.Title);
                }
                catch (Exception micaEx)
                {
                    _logger.LogWarning(micaEx, "Mica effect not supported, falling back to Acrylic");
                    try
                    {
                        window.WindowBackdropType = WindowBackdropType.Acrylic;
                        window.Opacity = 0.95;
                        _logger.LogInformation("Applied Acrylic effect as fallback to window: {WindowTitle}", window.Title);
                    }
                    catch (Exception acrylicEx)
                    {
                        _logger.LogWarning(acrylicEx, "Acrylic effect also not supported, using no backdrop");
                        window.WindowBackdropType = WindowBackdropType.None;
                        window.Opacity = 1.0;
                    }
                }
            }
            else if (_appSettings.IsTransparencyEnabled)
            {
                // Enable Acrylic effect with custom opacity (Windows 10+)
                try
                {
                    window.WindowBackdropType = WindowBackdropType.Acrylic;
                    window.Opacity = opacity;
                    _logger.LogInformation("Applied Acrylic effect with opacity {Opacity} to window: {WindowTitle}", opacity, window.Title);
                }
                catch (Exception acrylicEx)
                {
                    _logger.LogWarning(acrylicEx, "Acrylic effect not supported, using opacity only");
                    window.WindowBackdropType = WindowBackdropType.None;
                    window.Opacity = opacity;
                }
            }
            else
            {
                // Disable all effects
                window.WindowBackdropType = WindowBackdropType.None;
                window.Opacity = 1.0;
                _logger.LogInformation("Disabled all visual effects for window: {WindowTitle}", window.Title);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply effects to window: {WindowTitle}", window.Title);
            
            // Fallback to no effects
            try
            {
                window.WindowBackdropType = WindowBackdropType.None;
                window.ExtendsContentIntoTitleBar = true;
                window.Opacity = 1.0;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Failed to apply fallback effects");
            }
        }
    }

    private void OnAppSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppSettings.IsTransparencyEnabled) || 
            e.PropertyName == nameof(AppSettings.EnableGlassEffect) ||
            e.PropertyName == nameof(AppSettings.TransparencyAmount))
        {
            ApplyVisualEffects();
        }
        else if (e.PropertyName == nameof(AppSettings.IsDarkMode))
        {
            ApplyTheme(_appSettings.IsDarkMode ? "Dark" : "Light");
        }
    }
}