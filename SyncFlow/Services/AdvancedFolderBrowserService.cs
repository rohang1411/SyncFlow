using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SyncFlow.Services
{
    /// <summary>
    /// Advanced folder browser service with full MTP device support
    /// </summary>
    public class AdvancedFolderBrowserService : IEnhancedFolderBrowserService
    {
        private readonly ILogger<AdvancedFolderBrowserService>? _logger;

        public AdvancedFolderBrowserService(ILogger<AdvancedFolderBrowserService>? logger = null)
        {
            _logger = logger;
        }

        public string? SelectFolder(string title = "Select Folder", string? initialPath = null)
        {
            try
            {
                // Use Windows Vista+ folder browser dialog which supports MTP devices
                var dialog = new OpenFileDialog
                {
                    Title = title,
                    CheckFileExists = false,
                    CheckPathExists = false,
                    ValidateNames = false,
                    FileName = "Select Folder",
                    Filter = "Folders|*.folder",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    DereferenceLinks = false
                };

                // Set custom flags to show folders and MTP devices
                var dialogType = dialog.GetType();
                var optionsField = dialogType.GetField("options", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (optionsField != null)
                {
                    var optionsValue = optionsField.GetValue(dialog);
                    if (optionsValue != null)
                    {
                        var options = (int)optionsValue;
                        options |= 0x00000020; // FOS_PICKFOLDERS
                        optionsField.SetValue(dialog, options);
                    }
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedPath = Path.GetDirectoryName(dialog.FileName);
                    _logger?.LogInformation("Selected folder: {Path}", selectedPath);
                    return selectedPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Advanced folder browser failed, falling back to standard");
                return FallbackFolderBrowser(title, initialPath);
            }
        }

        private string? FallbackFolderBrowser(string title, string? initialPath)
        {
            try
            {
                // Fallback to Windows Shell folder browser with helpful instructions
                var folderBrowser = new FolderBrowserDialog
                {
                    Description = title + "\n\nNote: If you don't see your smartphone, try:\n1. Enable 'File Transfer' mode on your phone\n2. Unlock your phone screen\n3. Trust this computer if prompted",
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = false,
                    RootFolder = Environment.SpecialFolder.MyComputer
                };

                if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
                {
                    folderBrowser.SelectedPath = initialPath;
                }

                var result = folderBrowser.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrEmpty(folderBrowser.SelectedPath))
                {
                    _logger?.LogInformation("Selected folder (fallback): {Path}", folderBrowser.SelectedPath);
                    return folderBrowser.SelectedPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Fallback folder browser also failed");
                return null;
            }
        }

        public List<string> GetAvailableDrives()
        {
            var drives = new List<string>();
            
            try
            {
                // Get standard drives
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        var label = string.IsNullOrEmpty(drive.VolumeLabel) ? 
                            drive.DriveType.ToString() : drive.VolumeLabel;
                        drives.Add($"{drive.Name} ({label})");
                    }
                }

                // Try to detect portable devices using WMI
                var portableDevices = GetPortableDevicesViaWMI();
                drives.AddRange(portableDevices);

                _logger?.LogInformation("Found {Count} available drives/devices", drives.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting available drives");
            }

            return drives;
        }

        private List<string> GetPortableDevicesViaWMI()
        {
            var devices = new List<string>();
            
            try
            {
                // Check for MTP devices in registry (WMI requires additional package)
                var mtpDevices = GetMTPDevicesFromRegistry();
                devices.AddRange(mtpDevices);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not enumerate portable devices");
            }

            return devices;
        }

        private List<string> GetMTPDevicesFromRegistry()
        {
            var devices = new List<string>();
            
            try
            {
                // Check Windows registry for MTP devices
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Enum\USB");
                
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        if (subKeyName.Contains("VID_") && subKeyName.Contains("PID_"))
                        {
                            using var deviceKey = key.OpenSubKey(subKeyName);
                            if (deviceKey != null)
                            {
                                foreach (var instanceName in deviceKey.GetSubKeyNames())
                                {
                                    using var instanceKey = deviceKey.OpenSubKey(instanceName);
                                    var friendlyName = instanceKey?.GetValue("FriendlyName")?.ToString();
                                    
                                    if (!string.IsNullOrEmpty(friendlyName) && 
                                        (friendlyName.Contains("Phone") || 
                                         friendlyName.Contains("Android") ||
                                         friendlyName.Contains("MTP")))
                                    {
                                        devices.Add($"{friendlyName} (MTP Device)");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Could not read MTP devices from registry");
            }

            return devices;
        }

        public bool IsPathAccessible(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return false;

                // For MTP devices, we need special handling
                if (path.StartsWith("::"))
                {
                    return IsMtpPathAccessible(path);
                }

                // For regular paths
                return Directory.Exists(path);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Error checking path accessibility: {Path}", path);
                return false;
            }
        }

        private bool IsMtpPathAccessible(string mtpPath)
        {
            try
            {
                // Try to access the path using Shell32
                var shellType = Type.GetTypeFromProgID("Shell.Application");
                if (shellType == null) return false;
                
                var shell = Activator.CreateInstance(shellType);
                if (shell == null) return false;
                
                var folder = shell.GetType().InvokeMember("NameSpace", 
                    System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { mtpPath });
                
                return folder != null;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "MTP path not accessible: {Path}", mtpPath);
                return false;
            }
        }

        public List<string> GetFoldersInPath(string path)
        {
            var folders = new List<string>();
            
            try
            {
                if (path.StartsWith("::"))
                {
                    // Handle MTP device paths
                    folders = GetMtpFolders(path);
                }
                else
                {
                    // Handle regular file system paths
                    if (Directory.Exists(path))
                    {
                        folders = new List<string>(Directory.GetDirectories(path));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting folders in path: {Path}", path);
            }

            return folders;
        }

        private List<string> GetMtpFolders(string mtpPath)
        {
            var folders = new List<string>();
            
            try
            {
                // Use Shell32 COM to enumerate MTP folders
                var shellType = Type.GetTypeFromProgID("Shell.Application");
                if (shellType == null) return folders;
                
                var shell = Activator.CreateInstance(shellType);
                if (shell == null) return folders;
                
                var folder = shell.GetType().InvokeMember("NameSpace", 
                    System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { mtpPath });
                
                if (folder != null)
                {
                    var items = folder.GetType().InvokeMember("Items", 
                        System.Reflection.BindingFlags.InvokeMethod, null, folder, null);
                    
                    if (items != null)
                    {
                        var countValue = items.GetType().InvokeMember("Count", 
                            System.Reflection.BindingFlags.GetProperty, null, items, null);
                        
                        if (countValue != null)
                        {
                            var count = (int)countValue;
                            
                            for (int i = 0; i < count; i++)
                            {
                                var item = items.GetType().InvokeMember("Item", 
                                    System.Reflection.BindingFlags.InvokeMethod, null, items, new object[] { i });
                                
                                if (item != null)
                                {
                                    var isFolderValue = item.GetType().InvokeMember("IsFolder", 
                                        System.Reflection.BindingFlags.GetProperty, null, item, null);
                                    
                                    if (isFolderValue != null && (bool)isFolderValue)
                                    {
                                        var itemPath = item.GetType().InvokeMember("Path", 
                                            System.Reflection.BindingFlags.GetProperty, null, item, null)?.ToString();
                                        
                                        if (!string.IsNullOrEmpty(itemPath))
                                        {
                                            folders.Add(itemPath);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting MTP folders: {Path}", mtpPath);
            }

            return folders;
        }
    }
}
