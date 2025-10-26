namespace SyncFlow.Services;

/// <summary>
/// Service interface for managing application themes
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current theme (Light or Dark)
    /// </summary>
    string CurrentTheme { get; }

    /// <summary>
    /// Gets whether dark theme is currently active
    /// </summary>
    bool IsDarkTheme { get; }

    /// <summary>
    /// Applies the specified theme
    /// </summary>
    /// <param name="themeName">Theme name (Light or Dark)</param>
    void ApplyTheme(string themeName);

    /// <summary>
    /// Toggles between light and dark themes
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// Event raised when theme changes
    /// </summary>
    event EventHandler<string>? ThemeChanged;
}
