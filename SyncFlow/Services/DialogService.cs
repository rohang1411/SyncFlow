using Microsoft.Win32;
using SyncFlow.Views;
using System.Windows;
using Application = System.Windows.Application;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SyncFlow.Services;

public class DialogService : IDialogService
{
    public string? SelectDirectory(string title = "Select Directory")
    {
        var dialog = new OpenFolderDialog
        {
            Title = title
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public string? SelectFile(string title, string filter)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveFile(string title, string filter, string defaultFileName = "")
    {
        var dialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter,
            FileName = defaultFileName
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowError(string message, string title = "Error")
    {
        var dialog = ModernDialogWindow.CreateErrorDialog(title, message);
        dialog.Owner = Application.Current.MainWindow;
        dialog.ShowDialog();
    }

    public MessageBoxResult ShowWarning(string message, string title = "Warning", MessageBoxButton buttons = MessageBoxButton.OK)
    {
        if (buttons == MessageBoxButton.YesNo)
        {
            var dialog = ModernDialogWindow.CreateConfirmationDialog(title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            return dialog.Result;
        }
        else
        {
            var dialog = ModernDialogWindow.CreateWarningDialog(title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            return MessageBoxResult.OK;
        }
    }

    public void ShowInfo(string message, string title = "Information")
    {
        var dialog = ModernDialogWindow.CreateInfoDialog(title, message);
        dialog.Owner = Application.Current.MainWindow;
        dialog.ShowDialog();
    }

    public void ShowSuccess(string message, string title = "Success")
    {
        var dialog = ModernDialogWindow.CreateSuccessDialog(title, message);
        dialog.Owner = Application.Current.MainWindow;
        dialog.ShowDialog();
    }

    public MessageBoxResult ShowConfirmation(string message, string title = "Confirm")
    {
        var dialog = ModernDialogWindow.CreateConfirmationDialog(title, message);
        dialog.Owner = Application.Current.MainWindow;
        dialog.ShowDialog();
        return dialog.Result;
    }
}