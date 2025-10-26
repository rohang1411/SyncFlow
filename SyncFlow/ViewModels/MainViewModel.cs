using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using SyncFlow.Commands;
using SyncFlow.Models;
using SyncFlow.Services;

namespace SyncFlow.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application window
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly ITransferService _transferService;
        private readonly IEnhancedTransferService _enhancedTransferService;
        private readonly IVerificationService _verificationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<MainViewModel>? _logger;

        private ProfileViewModel? _selectedProfile;

        public MainViewModel(
            IProfileService profileService,
            ITransferService transferService,
            IEnhancedTransferService enhancedTransferService,
            IVerificationService verificationService,
            IDialogService dialogService,
            ILogger<MainViewModel>? logger = null)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _enhancedTransferService = enhancedTransferService ?? throw new ArgumentNullException(nameof(enhancedTransferService));
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger;

            Profiles = new ObservableCollection<ProfileViewModel>();

            // Initialize commands
            CreateProfileCommand = new RelayCommand(ExecuteCreateProfile);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            RefreshProfilesCommand = new AsyncRelayCommand(ExecuteRefreshProfiles);
            ExportProfilesCommand = new AsyncRelayCommand(ExecuteExportProfiles);
            ImportProfilesCommand = new AsyncRelayCommand(ExecuteImportProfiles);

            // Load profiles on initialization
            _ = LoadProfilesAsync();
        }

        #region Properties

        public ObservableCollection<ProfileViewModel> Profiles { get; }

        public ProfileViewModel? SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);
        }

        public bool HasProfiles => Profiles.Count > 0;
        public bool HasNoProfiles => !HasProfiles;

        #endregion

        #region Commands

        public ICommand CreateProfileCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand RefreshProfilesCommand { get; }
        public ICommand ExportProfilesCommand { get; }
        public ICommand ImportProfilesCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteCreateProfile(object? parameter)
        {
            var newProfile = new TransferProfile
            {
                Id = Guid.NewGuid(),
                Name = "New Profile",
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            // This will be handled by the view to show the editor dialog
            // For now, we'll add a placeholder
            var profileViewModel = CreateProfileViewModel(newProfile);
            profileViewModel.EditRequested += OnProfileEditRequested;
            profileViewModel.DeleteRequested += OnProfileDeleteRequested;

            // Show edit dialog immediately for new profile
            OnProfileEditRequested(profileViewModel, EventArgs.Empty);
        }

        private void ExecuteOpenSettings(object? parameter)
        {
            // This event will be handled by the view to show the settings dialog
            OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task ExecuteRefreshProfiles()
        {
            await LoadProfilesAsync();
        }

        private async Task ExecuteExportProfiles()
        {
            try
            {
                var filePath = _dialogService.SaveFile("Export Profiles", "JSON Files|*.json", "profiles.json");
                if (string.IsNullOrEmpty(filePath))
                    return;

                var json = await _profileService.ExportProfilesAsync(Profiles.Select(p => p.Profile));
                await File.WriteAllTextAsync(filePath, json);

                _dialogService.ShowSuccess($"Successfully exported {Profiles.Count} profiles to {Path.GetFileName(filePath)}", "Export Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to export profiles: {ex.Message}", "Export Failed");
            }
        }

        private async Task ExecuteImportProfiles()
        {
            try
            {
                var filePath = _dialogService.SelectFile("Import Profiles", "JSON Files|*.json");
                if (string.IsNullOrEmpty(filePath))
                    return;

                var json = await File.ReadAllTextAsync(filePath);
                await _profileService.ImportProfilesAsync(json);
                await LoadProfilesAsync();

                _dialogService.ShowSuccess($"Successfully imported profiles from {Path.GetFileName(filePath)}", "Import Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to import profiles: {ex.Message}", "Import Failed");
            }
        }

        #endregion

        #region Public Methods

        public async Task LoadProfilesAsync()
        {
            try
            {
                var profiles = await _profileService.GetAllProfilesAsync();
                
                Profiles.Clear();
                foreach (var profile in profiles)
                {
                    var viewModel = CreateProfileViewModel(profile);
                    viewModel.EditRequested += OnProfileEditRequested;
                    viewModel.DeleteRequested += OnProfileDeleteRequested;
                    Profiles.Add(viewModel);
                }

                OnPropertyChanged(nameof(HasProfiles));
                OnPropertyChanged(nameof(HasNoProfiles));
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to load profiles: {ex.Message}", "Error");
            }
        }

        public async Task SaveProfileAsync(TransferProfile profile)
        {
            try
            {
                await _profileService.SaveProfileAsync(profile);
                
                // Update or add the profile in the collection
                var existingViewModel = Profiles.FirstOrDefault(p => p.Id == profile.Id);
                if (existingViewModel == null)
                {
                    // New profile
                    var newViewModel = CreateProfileViewModel(profile);
                    newViewModel.EditRequested += OnProfileEditRequested;
                    newViewModel.DeleteRequested += OnProfileDeleteRequested;
                    Profiles.Add(newViewModel);
                }
                else
                {
                    // Profile was edited - reload it
                    await LoadProfilesAsync();
                }

                OnPropertyChanged(nameof(HasProfiles));
                OnPropertyChanged(nameof(HasNoProfiles));
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to save profile: {ex.Message}", "Error");
            }
        }

        public async Task DeleteProfileAsync(ProfileViewModel profileViewModel)
        {
            try
            {
                _logger?.LogInformation("Delete requested for profile: {ProfileName} (ID: {ProfileId})", 
                    profileViewModel.ProfileName, profileViewModel.Id);

                var result = _dialogService.ShowWarning(
                    $"Are you sure you want to delete the profile '{profileViewModel.ProfileName}'?",
                    "Confirm Delete",
                    System.Windows.MessageBoxButton.YesNo);

                _logger?.LogInformation("User response to delete confirmation: {Result}", result);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    _logger?.LogInformation("Deleting profile from service: {ProfileName}", profileViewModel.ProfileName);
                    await _profileService.DeleteProfileAsync(profileViewModel.Id);
                    
                    _logger?.LogInformation("Removing profile from UI collection");
                    Profiles.Remove(profileViewModel);

                    OnPropertyChanged(nameof(HasProfiles));
                    OnPropertyChanged(nameof(HasNoProfiles));

                    _logger?.LogInformation("Profile deleted successfully: {ProfileName}", profileViewModel.ProfileName);
                    _dialogService.ShowSuccess("Profile deleted successfully", "Success");
                }
                else
                {
                    _logger?.LogInformation("Profile deletion cancelled by user");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete profile: {ProfileName}", profileViewModel.ProfileName);
                _dialogService.ShowError($"Failed to delete profile: {ex.Message}", "Error");
            }
        }

        #endregion

        #region Helper Methods

        private ProfileViewModel CreateProfileViewModel(TransferProfile profile)
        {
            return new ProfileViewModel(profile, _transferService, _verificationService, _enhancedTransferService);
        }

        public async Task<EnhancedTransferResult> ExecuteEnhancedTransferAsync(TransferProfile profile, IProgress<EnhancedTransferProgress> progress)
        {
            try
            {
                _logger?.LogInformation("Starting enhanced transfer for profile: {ProfileName}", profile.Name);

                // Step 1: Validate storage space
                var storageValidation = await _enhancedTransferService.ValidateStorageSpaceAsync(profile);
                
                if (!storageValidation.HasSufficientSpace)
                {
                    var userChoice = _dialogService.ShowWarning(
                        $"Insufficient Storage Space\n\n{storageValidation.FormattedMessage}\n\nDo you want to proceed anyway?",
                        "Storage Warning",
                        System.Windows.MessageBoxButton.YesNo);

                    if (userChoice != System.Windows.MessageBoxResult.Yes)
                    {
                        _logger?.LogInformation("Transfer cancelled by user due to insufficient storage");
                        return new EnhancedTransferResult
                        {
                            IsSuccess = false,
                            StorageInfo = storageValidation,
                            Errors = new List<TransferError>
                            {
                                new TransferError
                                {
                                    ErrorMessage = "Transfer cancelled by user due to insufficient storage space",
                                    ErrorType = "User Cancelled"
                                }
                            }
                        };
                    }
                }

                // Step 2: Execute enhanced transfer
                var result = await _enhancedTransferService.TransferWithValidationAsync(profile, progress);
                
                _logger?.LogInformation("Enhanced transfer completed: {SuccessfulFiles}/{TotalFiles} files", 
                    result.SuccessfulFiles, result.TotalFiles);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Enhanced transfer failed for profile: {ProfileName}", profile.Name);
                
                return new EnhancedTransferResult
                {
                    IsSuccess = false,
                    Errors = new List<TransferError>
                    {
                        new TransferError
                        {
                            ErrorMessage = ex.Message,
                            Exception = ex,
                            ErrorType = "System Error"
                        }
                    }
                };
            }
        }

        public async Task<StorageValidationResult> ValidateStorageForProfileAsync(TransferProfile profile)
        {
            try
            {
                return await _enhancedTransferService.ValidateStorageSpaceAsync(profile);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to validate storage for profile: {ProfileName}", profile.Name);
                
                return new StorageValidationResult
                {
                    HasSufficientSpace = false,
                    FormattedMessage = $"Error validating storage: {ex.Message}"
                };
            }
        }

        public async Task<EnhancedTransferResult> RetryFailedTransfersAsync(EnhancedTransferResult previousResult, IProgress<EnhancedTransferProgress> progress)
        {
            try
            {
                _logger?.LogInformation("Retrying failed transfers: {FailedCount} files", previousResult.FailedFiles);
                
                var retryResult = await _enhancedTransferService.RetryFailedTransfersAsync(previousResult, progress);
                
                _logger?.LogInformation("Retry completed: {SuccessfulFiles}/{TotalFiles} files", 
                    retryResult.SuccessfulFiles, retryResult.TotalFiles);

                return retryResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to retry transfers");
                
                return new EnhancedTransferResult
                {
                    IsSuccess = false,
                    Errors = new List<TransferError>
                    {
                        new TransferError
                        {
                            ErrorMessage = ex.Message,
                            Exception = ex,
                            ErrorType = "Retry Error"
                        }
                    }
                };
            }
        }

        public void ShowTransferResults(EnhancedTransferResult result)
        {
            try
            {
                if (result.IsSuccess)
                {
                    _dialogService.ShowSuccess(result.DetailedSummary, "Transfer Complete");
                }
                else
                {
                    var message = result.DetailedSummary;
                    if (result.Errors.Any())
                    {
                        message += "\n\nWould you like to retry the failed transfers?";
                        var retryChoice = _dialogService.ShowWarning(message, "Transfer Completed with Errors", 
                            System.Windows.MessageBoxButton.YesNo);
                        
                        if (retryChoice == System.Windows.MessageBoxResult.Yes)
                        {
                            // This event will be handled by the view to initiate retry
                            RetryTransferRequested?.Invoke(this, result);
                        }
                    }
                    else
                    {
                        _dialogService.ShowError(message, "Transfer Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing transfer results");
                _dialogService.ShowError($"Transfer completed but failed to display results: {ex.Message}", "Display Error");
            }
        }

        private void OnProfileEditRequested(object? sender, EventArgs e)
        {
            if (sender is ProfileViewModel profileViewModel)
            {
                // This event will be handled by the view to show the editor dialog
                EditProfileRequested?.Invoke(this, profileViewModel);
            }
        }

        private void OnProfileDeleteRequested(object? sender, EventArgs e)
        {
            if (sender is ProfileViewModel profileViewModel)
            {
                _ = DeleteProfileAsync(profileViewModel);
            }
        }

        #endregion

        #region Events

        public event EventHandler<ProfileViewModel>? EditProfileRequested;
        public event EventHandler? OpenSettingsRequested;
        public event EventHandler<EnhancedTransferResult>? RetryTransferRequested;

        #endregion
    }
}

