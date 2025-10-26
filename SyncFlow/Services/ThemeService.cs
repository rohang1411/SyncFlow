using Microsoft.Extensions.Logging;
using System.Windows;
using SyncFlow.Models;
using Wpf.Ui.Controls;

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
            var app = System.Windows.Application.Current;
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
            if (_appSettings.IsTransparencyEnabled && _appSettings.EnableGlassEffect)
            {
                // Enable Mica effect (Windows 11)
                window.WindowBackdropType = WindowBackdropType.Mica;
                window.ExtendsContentIntoTitleBar = true;
            }
            else if (_appSettings.IsTransparencyEnabled)
            {
                // Enable Acrylic effect (Windows 10+)
                window.WindowBackdropType = WindowBackdropType.Acrylic;
                window.ExtendsContentIntoTitleBar = true;
            }
            else
            {
                // Disable all effects
                window.WindowBackdropType = WindowBackdropType.None;
                window.ExtendsContentIntoTitleBar = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply effects to window: {WindowTitle}", window.Title);
            
            // Fallback to no effects
            try
            {
                window.WindowBackdropType = WindowBackdropType.None;
                window.ExtendsContentIntoTitleBar = false;
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
            e.PropertyName == nameof(AppSettings.EnableGlassEffect))
        {
            ApplyVisualEffects();
        }
        else if (e.PropertyName == nameof(AppSettings.IsDarkMode))
        {
            ApplyTheme(_appSettings.IsDarkMode ? "Dark" : "Light");
        }
    }
}