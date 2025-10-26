using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;
using MessageBoxResult = System.Windows.MessageBoxResult;
using Color = System.Windows.Media.Color;

namespace SyncFlow.Views;

/// <summary>
/// Modern dialog window with Fluent Design
/// </summary>
public partial class ModernDialogWindow : FluentWindow
{
    public ModernDialogWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string Title { get; set; } = "Dialog";
    public string Message { get; set; } = string.Empty;
    public string IconText { get; set; } = "ℹ️";
    public System.Windows.Media.Brush IconBackground { get; set; } = new SolidColorBrush(Color.FromRgb(52, 152, 219));
    public string PrimaryButtonText { get; set; } = "OK";
    public string SecondaryButtonText { get; set; } = "Cancel";
    public Visibility PrimaryButtonVisibility { get; set; } = Visibility.Visible;
    public Visibility SecondaryButtonVisibility { get; set; } = Visibility.Collapsed;
    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

    public static ModernDialogWindow CreateSuccessDialog(string title, string message)
    {
        return new ModernDialogWindow
        {
            Title = title,
            Message = message,
            IconText = "✓",
            IconBackground = new SolidColorBrush(Color.FromRgb(39, 174, 96)), // Green
            PrimaryButtonText = "OK",
            SecondaryButtonVisibility = Visibility.Collapsed
        };
    }

    public static ModernDialogWindow CreateErrorDialog(string title, string message)
    {
        return new ModernDialogWindow
        {
            Title = title,
            Message = message,
            IconText = "✕",
            IconBackground = new SolidColorBrush(Color.FromRgb(231, 76, 60)), // Red
            PrimaryButtonText = "OK",
            SecondaryButtonVisibility = Visibility.Collapsed
        };
    }

    public static ModernDialogWindow CreateWarningDialog(string title, string message)
    {
        return new ModernDialogWindow
        {
            Title = title,
            Message = message,
            IconText = "⚠",
            IconBackground = new SolidColorBrush(Color.FromRgb(243, 156, 18)), // Orange
            PrimaryButtonText = "OK",
            SecondaryButtonVisibility = Visibility.Collapsed
        };
    }

    public static ModernDialogWindow CreateInfoDialog(string title, string message)
    {
        return new ModernDialogWindow
        {
            Title = title,
            Message = message,
            IconText = "ℹ",
            IconBackground = new SolidColorBrush(Color.FromRgb(52, 152, 219)), // Blue
            PrimaryButtonText = "OK",
            SecondaryButtonVisibility = Visibility.Collapsed
        };
    }

    public static ModernDialogWindow CreateConfirmationDialog(string title, string message)
    {
        return new ModernDialogWindow
        {
            Title = title,
            Message = message,
            IconText = "?",
            IconBackground = new SolidColorBrush(Color.FromRgb(52, 152, 219)), // Blue
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
            PrimaryButtonVisibility = Visibility.Visible,
            SecondaryButtonVisibility = Visibility.Visible
        };
    }

    private void OnPrimaryButtonClick(object sender, RoutedEventArgs e)
    {
        Result = PrimaryButtonText == "Yes" ? MessageBoxResult.Yes : MessageBoxResult.OK;
        DialogResult = true;
        Close();
    }

    private void OnSecondaryButtonClick(object sender, RoutedEventArgs e)
    {
        Result = SecondaryButtonText == "No" ? MessageBoxResult.No : MessageBoxResult.Cancel;
        DialogResult = false;
        Close();
    }
}
