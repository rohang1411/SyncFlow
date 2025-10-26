using Microsoft.Extensions.Logging;
using System.Windows;

namespace SyncFlow.Services;

/// <summary>
/// Service for managing application themes
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private string _currentTheme = "Dark";

    public ThemeService(ILogger<ThemeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
}