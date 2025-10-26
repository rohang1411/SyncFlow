using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using SyncFlow.Commands;
using SyncFlow.Models;
using SyncFlow.Services;

namespace SyncFlow.ViewModels
{
    /// <summary>
    /// ViewModel for an individual transfer profile with state management
    /// </summary>
    public class ProfileViewModel : ViewModelBase
    {
        private readonly TransferProfile _profile;
        private readonly ITransferService _transferService;
        private readonly IVerificationService _verificationService;
        private readonly IEnhancedTransferService? _enhancedTransferService;
        private readonly ILogger<ProfileViewModel>? _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        // State properties
        private ProfileState _currentState = ProfileState.Idle;
        private string _statusMessage = "Ready";
        private double _progressPercentage;
        private string _currentFile = string.Empty;
        private long _bytesTransferred;
        private long _totalBytes;
        private double _transferSpeed; // MB/s
        private TimeSpan _timeElapsed;
        private TimeSpan _timeRemaining;
        private string _errorMessage = string.Empty;
        private int _totalFiles;
        private int _processedFiles;
        private int _successfulFiles;
        private int _failedFiles;
        private EnhancedTransferResult? _lastTransferResult;

        public ProfileViewModel(TransferProfile profile, ITransferService transferService, IVerificationService verificationService, IEnhancedTransferService? enhancedTransferService = null, ILogger<ProfileViewModel>? logger = null)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _enhancedTransferService = enhancedTransferService;
            _logger = logger;

            // Initialize commands
            RunTransferCommand = new AsyncRelayCommand(ExecuteRunTransfer, CanExecuteRunTransfer);
            CancelTransferCommand = new RelayCommand(ExecuteCancelTransfer, CanExecuteCancelTransfer);
            VerifyCommand = new AsyncRelayCommand(ExecuteVerify, CanExecuteVerify);
            ResetCommand = new RelayCommand(ExecuteReset, _ => CanExecuteReset());
            EditCommand = new RelayCommand(ExecuteEdit, _ => CanExecuteEdit());
            DeleteCommand = new RelayCommand(ExecuteDelete, _ => CanExecuteDelete());
        }

        #region Properties

        public Guid Id => _profile.Id;
        public string ProfileName => _profile.Name;
        public string DestinationFolder => _profile.FolderMappings.FirstOrDefault()?.DestinationPath ?? "No mappings";
        public TransferProfile Profile => _profile;

        public ProfileState CurrentState
        {
            get => _currentState;
            private set
            {
                if (SetProperty(ref _currentState, value))
                {
                    OnPropertyChanged(nameof(IsIdle));
                    OnPropertyChanged(nameof(IsRunning));
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(IsFailed));
                    ((AsyncRelayCommand)RunTransferCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CancelTransferCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public double ProgressPercentage
        {
            get => _progressPercentage;
            private set => SetProperty(ref _progressPercentage, value);
        }

        public string CurrentFile
        {
            get => _currentFile;
            private set => SetProperty(ref _currentFile, value);
        }

        public long BytesTransferred
        {
            get => _bytesTransferred;
            private set
            {
                if (SetProperty(ref _bytesTransferred, value))
                {
                    OnPropertyChanged(nameof(TransferredText));
                }
            }
        }

        public long TotalBytes
        {
            get => _totalBytes;
            private set
            {
                if (SetProperty(ref _totalBytes, value))
                {
                    OnPropertyChanged(nameof(TotalBytesText));
                }
            }
        }

        public double TransferSpeed
        {
            get => _transferSpeed;
            private set
            {
                if (SetProperty(ref _transferSpeed, value))
                {
                    OnPropertyChanged(nameof(TransferSpeedText));
                }
            }
        }

        public TimeSpan TimeElapsed
        {
            get => _timeElapsed;
            private set
            {
                if (SetProperty(ref _timeElapsed, value))
                {
                    OnPropertyChanged(nameof(TimeElapsedText));
                }
            }
        }

        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            private set
            {
                if (SetProperty(ref _timeRemaining, value))
                {
                    OnPropertyChanged(nameof(TimeRemainingText));
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        // State flags
        public bool IsIdle => CurrentState == ProfileState.Idle;
        public bool IsRunning => CurrentState == ProfileState.Running;
        public bool IsCompleted => CurrentState == ProfileState.Completed;
        public bool IsFailed => CurrentState == ProfileState.Failed;

        public int TotalFiles
        {
            get => _totalFiles;
            private set => SetProperty(ref _totalFiles, value);
        }

        public int ProcessedFiles
        {
            get => _processedFiles;
            private set => SetProperty(ref _processedFiles, value);
        }

        public int SuccessfulFiles
        {
            get => _successfulFiles;
            private set => SetProperty(ref _successfulFiles, value);
        }

        public int FailedFiles
        {
            get => _failedFiles;
            private set => SetProperty(ref _failedFiles, value);
        }

        public EnhancedTransferResult? LastTransferResult
        {
            get => _lastTransferResult;
            private set => SetProperty(ref _lastTransferResult, value);
        }

        // Formatted text properties
        public string TransferredText => FormatBytes(BytesTransferred);
        public string TotalBytesText => FormatBytes(TotalBytes);
        public string TransferSpeedText => $"{TransferSpeed:F1} MB/s";
        public string TimeElapsedText => FormatTime(TimeElapsed);
        public string TimeRemainingText => FormatTime(TimeRemaining);
        public string ProgressText => $"{TransferredText} / {TotalBytesText} ({ProgressPercentage:F0}%)";
        public string FileProgressText => $"{ProcessedFiles} / {TotalFiles} files";
        public string DetailedProgressText => $"{SuccessfulFiles} successful, {FailedFiles} failed out of {ProcessedFiles} processed";
        public bool HasEnhancedTransferService => _enhancedTransferService != null;

        #endregion

        #region Commands

        public ICommand RunTransferCommand { get; }
        public ICommand CancelTransferCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public event EventHandler? EditRequested;
        public event EventHandler? DeleteRequested;
        public event EventHandler<VerificationReport>? VerificationCompleted;

        #endregion

        #region Command Implementations

        private bool CanExecuteRunTransfer() => (CurrentState == ProfileState.Idle || CurrentState == ProfileState.Completed || CurrentState == ProfileState.Failed) && _profile.IsValid();

        private async Task ExecuteRunTransfer()
        {
            try
            {
                // Reset if coming from Completed or Failed state
                if (CurrentState != ProfileState.Idle)
                {
                    ResetState();
                }

                CurrentState = ProfileState.Running;
                StatusMessage = "Starting transfer...";
                _cancellationTokenSource = new CancellationTokenSource();

                if (_enhancedTransferService != null)
                {
                    // Use enhanced transfer service
                    await ExecuteEnhancedTransfer();
                }
                else
                {
                    // Fallback to original transfer service
                    await ExecuteStandardTransfer();
                }
            }
            catch (OperationCanceledException)
            {
                CurrentState = ProfileState.Idle;
                StatusMessage = "Transfer cancelled";
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                CurrentState = ProfileState.Failed;
                StatusMessage = "Transfer failed with error";
                ErrorMessage = ex.Message;
                _logger?.LogError(ex, "Transfer failed for profile: {ProfileName}", _profile.Name);
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task ExecuteEnhancedTransfer()
        {
            if (_enhancedTransferService == null) return;

            try
            {
                var progress = new Progress<EnhancedTransferProgress>(UpdateEnhancedProgress);
                var result = await _enhancedTransferService.TransferWithValidationAsync(_profile, progress, _cancellationTokenSource!.Token);

                LastTransferResult = result;

                if (result.IsSuccess)
                {
                    CurrentState = ProfileState.Completed;
                    StatusMessage = $"Completed Successfully - {result.SuccessfulFiles}/{result.TotalFiles} files transferred";
                }
                else
                {
                    CurrentState = ProfileState.Failed;
                    StatusMessage = $"Transfer completed with errors - {result.SuccessfulFiles}/{result.TotalFiles} files transferred";
                    ErrorMessage = result.Errors.FirstOrDefault()?.ErrorMessage ?? "Unknown error";
                }

                _logger?.LogInformation("Enhanced transfer completed: {SuccessfulFiles}/{TotalFiles} files", 
                    result.SuccessfulFiles, result.TotalFiles);
            }
            catch (Exception ex)
            {
                CurrentState = ProfileState.Failed;
                StatusMessage = "Enhanced transfer failed";
                ErrorMessage = ex.Message;
                _logger?.LogError(ex, "Enhanced transfer failed for profile: {ProfileName}", _profile.Name);
            }
        }

        private async Task ExecuteStandardTransfer()
        {
            try
            {
                var progress = new Progress<TransferProgress>(UpdateProgress);
                var result = await _transferService.ExecuteTransferAsync(_profile, progress, _cancellationTokenSource!.Token);

                if (result.Success)
                {
                    StatusMessage = "Verifying transfer...";
                    var verificationResult = await _verificationService.VerifyTransferAsync(_profile, result);

                    if (verificationResult.IsSuccessful)
                    {
                        CurrentState = ProfileState.Completed;
                        StatusMessage = $"Completed Successfully - {result.FilesCopied} files transferred";
                    }
                    else
                    {
                        CurrentState = ProfileState.Failed;
                        StatusMessage = "Transfer verification failed";
                        ErrorMessage = verificationResult.ErrorMessage ?? "File count mismatch";
                    }
                }
                else
                {
                    CurrentState = ProfileState.Failed;
                    StatusMessage = "Transfer failed";
                    ErrorMessage = result.ErrorMessage ?? "Unknown error";
                }
            }
            catch (Exception ex)
            {
                CurrentState = ProfileState.Failed;
                StatusMessage = "Standard transfer failed";
                ErrorMessage = ex.Message;
                _logger?.LogError(ex, "Standard transfer failed for profile: {ProfileName}", _profile.Name);
            }
        }

        private bool CanExecuteCancelTransfer() => CurrentState == ProfileState.Running;

        private void ExecuteCancelTransfer()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling...";
        }

        private bool CanExecuteVerify() => CurrentState == ProfileState.Idle;

        private async Task ExecuteVerify()
        {
            try
            {
                StatusMessage = "Verifying folders...";
                var report = await _verificationService.GenerateDetailedReportAsync(_profile);
                StatusMessage = "Ready";
                VerificationCompleted?.Invoke(this, report);
            }
            catch (Exception ex)
            {
                StatusMessage = "Verification failed";
                ErrorMessage = ex.Message;
            }
        }

        private bool CanExecuteReset() => CurrentState == ProfileState.Completed || CurrentState == ProfileState.Failed;

        private void ExecuteReset(object? parameter)
        {
            ResetState();
        }

        private bool CanExecuteEdit() => CurrentState == ProfileState.Idle || CurrentState == ProfileState.Completed || CurrentState == ProfileState.Failed;

        private void ExecuteEdit(object? parameter)
        {
            EditRequested?.Invoke(this, EventArgs.Empty);
        }

        private bool CanExecuteDelete() => CurrentState == ProfileState.Idle || CurrentState == ProfileState.Completed || CurrentState == ProfileState.Failed;

        private void ExecuteDelete(object? parameter)
        {
            _logger?.LogInformation("ExecuteDelete called for profile: {ProfileName}, State: {State}", 
                _profile.Name, CurrentState);
            
            if (DeleteRequested != null)
            {
                _logger?.LogInformation("Invoking DeleteRequested event for profile: {ProfileName}", _profile.Name);
                DeleteRequested.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger?.LogWarning("DeleteRequested event has no subscribers for profile: {ProfileName}", _profile.Name);
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateProgress(TransferProgress progress)
        {
            // Ensure UI updates happen on UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                try
                {
                    CurrentFile = progress.CurrentFile;
                    ProgressPercentage = progress.Percentage;
                    
                    // Note: These properties would need to be added to TransferProgress model
                    // For now, using simplified progress
                    StatusMessage = $"Transferring... {progress.FilesCopied} of {progress.TotalFiles} files";
                    
                    _logger?.LogDebug("Progress updated: {Percent}% - {Copied}/{Total} files", 
                        progress.Percentage, progress.FilesCopied, progress.TotalFiles);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error updating progress");
                }
            });
        }

        private void UpdateEnhancedProgress(EnhancedTransferProgress progress)
        {
            // Ensure UI updates happen on UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                try
                {
                    CurrentFile = progress.CurrentFile;
                    ProgressPercentage = progress.PercentComplete;
                    TotalFiles = progress.TotalFiles;
                    ProcessedFiles = progress.ProcessedFiles;
                    SuccessfulFiles = progress.SuccessfulFiles;
                    FailedFiles = progress.FailedFiles;
                    BytesTransferred = progress.ProcessedBytes;
                    TotalBytes = progress.TotalBytes;
                    TimeElapsed = progress.ElapsedTime;

                    // Calculate transfer speed
                    if (progress.ElapsedTime.TotalSeconds > 0)
                    {
                        TransferSpeed = progress.ProcessedBytes / progress.ElapsedTime.TotalSeconds / (1024 * 1024); // MB/s
                    }

                    // Calculate estimated time remaining
                    if (progress.PercentComplete > 0 && progress.PercentComplete < 100)
                    {
                        var totalEstimatedTime = progress.ElapsedTime.TotalSeconds * (100.0 / progress.PercentComplete);
                        TimeRemaining = TimeSpan.FromSeconds(totalEstimatedTime - progress.ElapsedTime.TotalSeconds);
                    }
                    else
                    {
                        TimeRemaining = TimeSpan.Zero;
                    }

                    // Update status message with detailed information
                    if (progress.TotalFiles > 0)
                    {
                        StatusMessage = $"Transferring... {progress.ProcessedFiles}/{progress.TotalFiles} files ({progress.SuccessfulFiles} successful, {progress.FailedFiles} failed)";
                    }
                    else
                    {
                        StatusMessage = "Preparing transfer...";
                    }

                    _logger?.LogDebug("Progress updated: {Percent}% - {Processed}/{Total} files", 
                        progress.PercentComplete, progress.ProcessedFiles, progress.TotalFiles);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error updating enhanced progress");
                }
            });
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            else if (time.TotalMinutes >= 1)
                return $"{(int)time.TotalMinutes}m {time.Seconds}s";
            else
                return $"{time.Seconds}s";
        }

        public void ResetState()
        {
            CurrentState = ProfileState.Idle;
            StatusMessage = "Ready";
            ProgressPercentage = 0;
            CurrentFile = string.Empty;
            BytesTransferred = 0;
            TotalBytes = 0;
            TransferSpeed = 0;
            TimeElapsed = TimeSpan.Zero;
            TimeRemaining = TimeSpan.Zero;
            ErrorMessage = string.Empty;
            TotalFiles = 0;
            ProcessedFiles = 0;
            SuccessfulFiles = 0;
            FailedFiles = 0;
            LastTransferResult = null;
        }

        public async Task<bool> ValidateStorageSpaceAsync()
        {
            if (_enhancedTransferService == null) return true;

            try
            {
                var validation = await _enhancedTransferService.ValidateStorageSpaceAsync(_profile);
                return validation.HasSufficientSpace;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to validate storage space for profile: {ProfileName}", _profile.Name);
                return true; // Assume sufficient space if validation fails
            }
        }

        public async Task<EnhancedTransferResult?> RetryFailedTransfersAsync()
        {
            if (_enhancedTransferService == null || LastTransferResult == null || !LastTransferResult.Errors.Any())
                return null;

            try
            {
                var progress = new Progress<EnhancedTransferProgress>(UpdateEnhancedProgress);
                var result = await _enhancedTransferService.RetryFailedTransfersAsync(LastTransferResult, progress);
                
                LastTransferResult = result;
                
                if (result.IsSuccess)
                {
                    CurrentState = ProfileState.Completed;
                    StatusMessage = $"Retry completed successfully - {result.SuccessfulFiles}/{result.TotalFiles} files transferred";
                }
                else
                {
                    StatusMessage = $"Retry completed with errors - {result.SuccessfulFiles}/{result.TotalFiles} files transferred";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to retry transfers for profile: {ProfileName}", _profile.Name);
                ErrorMessage = ex.Message;
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents the current state of a transfer profile
    /// </summary>
    public enum ProfileState
    {
        Idle,
        Running,
        Completed,
        Failed
    }
}

