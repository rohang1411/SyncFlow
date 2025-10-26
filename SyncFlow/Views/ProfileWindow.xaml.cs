using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using SyncFlow.Models;
using SyncFlow.Repositories;
using SyncFlow.Services;

namespace SyncFlow.Views;

public partial class ProfileWindow : FluentWindow
{
    private readonly IProfileRepository _repository;
    private readonly IDialogService _dialogService;

    public ProfileWindow(IProfileRepository repository, IDialogService dialogService)
    {
        InitializeComponent();
        _repository = repository;
        _dialogService = dialogService;
        LoadProfiles();
    }

    private async void LoadProfiles()
    {
        var profiles = await _repository.GetAllAsync();
        ProfileList.ItemsSource = profiles;
    }

    private void OnNewProfile(object sender, RoutedEventArgs e)
    {
        var profile = new TransferProfile { Id = Guid.NewGuid() };
        EditProfile(profile, true);
    }

    private void OnEditProfile(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is TransferProfile profile)
        {
            EditProfile(profile, false);
        }
    }

    private async void OnDeleteProfile(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is TransferProfile profile)
        {
            var result = _dialogService.ShowWarning(
                $"Are you sure you want to delete profile '{profile.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                await _repository.DeleteAsync(profile.Id);
                LoadProfiles();
            }
        }
    }

    private async void OnImportProfiles(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Import Profiles"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = await File.ReadAllTextAsync(dialog.FileName);
                await _repository.ImportProfilesAsync(json);
                LoadProfiles();
                _dialogService.ShowInfo("Profiles imported successfully.", "Import Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to import profiles: {ex.Message}", "Import Error");
            }
        }
    }

    private async void OnExportProfiles(object sender, RoutedEventArgs e)
    {
        if (ProfileList.SelectedItems is not null)
        {
            var selectedProfiles = ProfileList.SelectedItems.Cast<TransferProfile>().ToList();
            if (!selectedProfiles.Any())
            {
                _dialogService.ShowWarning("Please select profiles to export.", "Export Profiles");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Export Profiles",
                FileName = "syncflow_profiles.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = await _repository.ExportProfilesAsync(selectedProfiles);
                    await File.WriteAllTextAsync(dialog.FileName, json);
                    _dialogService.ShowInfo("Profiles exported successfully.", "Export Complete");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Failed to export profiles: {ex.Message}", "Export Error");
                }
            }
        }
    }

    private async void EditProfile(TransferProfile profile, bool isNew)
    {
        var window = new ProfileEditorWindow(profile, _dialogService)
        {
            Owner = this
        };

        if (window.ShowDialog() == true)
        {
            await _repository.SaveAsync(profile);
            LoadProfiles();
        }
    }

    private void OnDone(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}