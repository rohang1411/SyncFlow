using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SyncFlow.Repositories;
using SyncFlow.Services;
using SyncFlow.Views;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SyncFlow;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Build the host with dependency injection
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .Build();

            // Start the host
            await _host.StartAsync();

            // Create and show main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to start application: {ex.Message}", "Startup Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
        catch (Exception ex)
        {
            // Log error but don't prevent shutdown
            System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex.Message}");
        }

        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register models
        services.AddSingleton<Models.AppSettings>();
        
        // Register services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<IFileOperations, WindowsFileOperations>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ITransferService, TransferService>();
        services.AddSingleton<IVerificationService, VerificationService>();
        
        // Register enhanced transfer services
        services.AddSingleton<IFileInventoryService, FileInventoryService>();
        services.AddSingleton<IStorageValidationService, StorageValidationService>();
        services.AddSingleton<IEnhancedTransferService, EnhancedTransferService>();
        services.AddSingleton<IWindowConfigurationService, WindowConfigurationService>();
        
        // Register repositories
        services.AddSingleton<IProfileRepository, ProfileRepository>();
        
        // Register views
        services.AddSingleton<MainWindow>();
    }

    private static void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        
        // Set minimum log level
        logging.SetMinimumLevel(LogLevel.Information);
    }
}

