using System;
using System.Windows;
using Wpf.Ui.Controls;
using SyncFlow.ViewModels;
using SyncFlow.Services;
using SyncFlow.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SyncFlow.Models;

namespace SyncFlow.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private readonly MainViewModel _viewModel;
        private readonly IDialogService _dialogService;
        private readonly IThemeService _themeService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppSettings _appSettings;

        public MainWindow(
            IProfileService profileService,
            ITransferService transferService,
            IVerificationService verificationService,
            IDialogService dialogService,
            IThemeService themeService,
            AppSettings appSettings,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));

            // Create and set the MainViewModel
            var enhancedTransferService = serviceProvider.GetRequiredService<IEnhancedTransferService>();
            var logger = serviceProvider.GetService<ILogger<MainViewModel>>();
            
            _viewModel = new MainViewModel(
                profileService,
                transferService,
                enhancedTransferService,
                verificationService,
                dialogService,
                logger);

            _viewModel.EditProfileRequested += OnEditProfileRequested;
            _viewModel.OpenSettingsRequested += OnOpenSettingsRequested;

            DataContext = _viewModel;

            // Load profiles on window loaded
            Loaded += async (s, e) => await _viewModel.LoadProfilesAsync();
        }

        private void OnEditProfileRequested(object? sender, ProfileViewModel profileViewModel)
        {
            // Create or get the profile editor window
            var editorWindow = new ProfileEditorWindow(profileViewModel.Profile.Clone(), _dialogService)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (editorWindow.ShowDialog() == true && editorWindow.EditedProfile != null)
            {
                // Save the edited profile
                _ = _viewModel.SaveProfileAsync(editorWindow.EditedProfile);
            }
        }

        private void OnOpenSettingsRequested(object? sender, EventArgs e)
        {
            // Create and show settings window
            var settingsWindow = new SettingsWindow(_appSettings, _themeService, _dialogService)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            settingsWindow.ShowDialog();
            // Settings are automatically applied through the ViewModel
        }
    }
}
