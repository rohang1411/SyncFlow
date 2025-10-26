using System.Windows;
using SyncFlow.Views;

namespace SyncFlow.Services
{
    public interface IWindowConfigurationService
    {
        void ConfigureSettingsWindow(SettingsWindow window);
        void ConfigureProfileEditor(ProfileEditorWindow window);
        bool ValidateResourceBindings(FrameworkElement element);
        void ApplyFallbackStyling(Window window);
        void ConfigureBackdropEffects(Wpf.Ui.Controls.FluentWindow window);
        bool IsBackdropEffectSupported();
    }
}