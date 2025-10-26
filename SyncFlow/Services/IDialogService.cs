using System.Windows;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace SyncFlow.Services;

/// <summary>
/// Service interface for handling dialogs and user interactions
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a folder browser dialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <returns>Selected folder path or null if cancelled</returns>
    string? SelectDirectory(string title = "Select Directory");

    /// <summary>
    /// Shows a file open dialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="filter">File filter (e.g., "JSON Files|*.json")</param>
    /// <returns>Selected file path or null if cancelled</returns>
    string? SelectFile(string title, string filter);

    /// <summary>
    /// Shows a file save dialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="filter">File filter (e.g., "JSON Files|*.json")</param>
    /// <param name="defaultFileName">Default file name</param>
    /// <returns>Selected file path or null if cancelled</returns>
    string? SaveFile(string title, string filter, string defaultFileName = "");

    /// <summary>
    /// Shows an error dialog with modern styling
    /// </summary>
    /// <param name="message">Error message to display</param>
    /// <param name="title">Dialog title</param>
    void ShowError(string message, string title = "Error");

    /// <summary>
    /// Shows a warning dialog with modern styling
    /// </summary>
    /// <param name="message">Warning message to display</param>
    /// <param name="title">Dialog title</param>
    /// <param name="buttons">Message box buttons to display</param>
    /// <returns>Message box result</returns>
    MessageBoxResult ShowWarning(string message, string title = "Warning", MessageBoxButton buttons = MessageBoxButton.OK);

    /// <summary>
    /// Shows an information dialog with modern styling
    /// </summary>
    /// <param name="message">Information message to display</param>
    /// <param name="title">Dialog title</param>
    void ShowInfo(string message, string title = "Information");

    /// <summary>
    /// Shows a success dialog with modern styling
    /// </summary>
    /// <param name="message">Success message to display</param>
    /// <param name="title">Dialog title</param>
    void ShowSuccess(string message, string title = "Success");

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons
    /// </summary>
    /// <param name="message">Confirmation message to display</param>
    /// <param name="title">Dialog title</param>
    /// <returns>Message box result (Yes or No)</returns>
    MessageBoxResult ShowConfirmation(string message, string title = "Confirm");
}