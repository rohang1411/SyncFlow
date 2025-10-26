using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SyncFlow.Services
{
    /// <summary>
    /// Enhanced folder browser service that can access MTP devices (smartphones)
    /// </summary>
    public class EnhancedFolderBrowserService : IEnhancedFolderBrowserService
    {
        private readonly ILogger<EnhancedFolderBrowserService>? _logger;

        public EnhancedFolderBrowserService(ILogger<EnhancedFolderBrowserService>? logger = null)
        {
            _logger = logger;
        }

        public string? SelectFolder(string title = "Select Folder", string? initialPath = null)
        {
            try
            {
                // Try to use the enhanced folder browser that can see MTP devices
                using var dialog = new FolderBrowserDialog
                {
                    Description = title,
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = false
                };

                if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
                {
                    dialog.SelectedPath = initialPath;
                }

                // Enable showing MTP devices by setting root folder to MyComputer
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    _logger?.LogInformation("Selected folder: {Path}", dialog.SelectedPath);
                    return dialog.SelectedPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error selecting folder");
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
                        drives.Add($"{drive.Name} ({drive.DriveType})");
                    }
                }

                // Try to detect MTP devices using Shell32
                var mtpDevices = GetMtpDevices();
                drives.AddRange(mtpDevices);

                _logger?.LogInformation("Found {Count} available drives/devices", drives.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting available drives");
            }

            return drives;
        }

        private List<string> GetMtpDevices()
        {
            var mtpDevices = new List<string>();

            try
            {
                // Use Shell32 to enumerate portable devices
                var shell = new Shell32.Shell();
                var folder = shell.NameSpace(Shell32.ShellSpecialFolderConstants.ssfDRIVES);

                if (folder != null)
                {
                    foreach (Shell32.FolderItem item in folder.Items())
                    {
                        if (item != null)
                        {
                            var path = item.Path;
                            var name = item.Name;

                            // Check if this looks like a portable device
                            if (IsPortableDevice(item))
                            {
                                mtpDevices.Add($"{name} (Portable Device)");
                                _logger?.LogInformation("Found MTP device: {Name} at {Path}", name, path);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not enumerate MTP devices using Shell32");
            }

            return mtpDevices;
        }

        private bool IsPortableDevice(Shell32.FolderItem item)
        {
            try
            {
                // Check various properties to identify portable devices
                var path = item.Path;
                var name = item.Name;

                // MTP devices typically have paths starting with "::" or contain specific patterns
                if (path.StartsWith("::") || 
                    name.Contains("Phone") || 
                    name.Contains("Android") ||
                    name.Contains("iPhone") ||
                    name.Contains("iPad") ||
                    name.Contains("Galaxy") ||
                    name.Contains("Pixel"))
                {
                    return true;
                }

                // Check if it's a portable device type
                var folderItem2 = item as Shell32.FolderItem2;
                if (folderItem2 != null)
                {
                    var type = folderItem2.ExtendedProperty("System.Devices.InterfaceClassGuid");
                    if (type != null && type.ToString().Contains("6ac27878-a6fa-4155-ba85-f98f491d4f33"))
                    {
                        return true; // WPD (Windows Portable Device) interface
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Error checking if item is portable device: {Name}", item.Name);
            }

            return false;
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
                var shell = new Shell32.Shell();
                var folder = shell.NameSpace(mtpPath);
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
                        folders = Directory.GetDirectories(path).ToList();
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
                var shell = new Shell32.Shell();
                var folder = shell.NameSpace(mtpPath);

                if (folder != null)
                {
                    foreach (Shell32.FolderItem item in folder.Items())
                    {
                        if (item != null && item.IsFolder)
                        {
                            folders.Add(item.Path);
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

    public interface IEnhancedFolderBrowserService
    {
        string? SelectFolder(string title = "Select Folder", string? initialPath = null);
        List<string> GetAvailableDrives();
        bool IsPathAccessible(string path);
        List<string> GetFoldersInPath(string path);
    }
}

// Shell32 COM interop classes
namespace Shell32
{
    [ComImport]
    [Guid("13709620-C279-11CE-A49E-444553540000")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Shell
    {
        [DispId(0x60020000)]
        public extern Folder NameSpace([In] object vDir);
    }

    [ComImport]
    [Guid("BBCBDE60-C3FF-11CE-8350-444553540000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface Folder
    {
        [DispId(0x60020000)]
        string Title { [return: MarshalAs(UnmanagedType.BStr)] get; }

        [DispId(0x60020001)]
        object Application { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(0x60020002)]
        object Parent { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(0x60020003)]
        Folder ParentFolder { [return: MarshalAs(UnmanagedType.Interface)] get; }

        [DispId(0x60020004)]
        FolderItems Items();
    }

    [ComImport]
    [Guid("744129E0-CBE5-11CE-8350-444553540000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface FolderItems : IEnumerable
    {
        [DispId(0x60020000)]
        int Count { get; }

        [DispId(0x60020001)]
        object Application { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(0x60020002)]
        object Parent { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(0x60020003)]
        FolderItem Item([In, Optional, MarshalAs(UnmanagedType.Struct)] object index);
    }

    [ComImport]
    [Guid("FAC32C80-CBE4-11CE-8350-444553540000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface FolderItem
    {
        [DispId(0)]
        object Application { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(1)]
        object Parent { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(2)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] get; set; }

        [DispId(3)]
        string Path { [return: MarshalAs(UnmanagedType.BStr)] get; }

        [DispId(4)]
        object GetLink { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(5)]
        object GetFolder { [return: MarshalAs(UnmanagedType.IDispatch)] get; }

        [DispId(6)]
        bool IsLink { get; }

        [DispId(7)]
        bool IsFolder { get; }

        [DispId(8)]
        bool IsFileSystem { get; }

        [DispId(9)]
        bool IsBrowsable { get; }
    }

    [ComImport]
    [Guid("edc817aa-92b8-11d1-b075-00c04fc33aa5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface FolderItem2 : FolderItem
    {
        [DispId(10)]
        object ExtendedProperty([In, MarshalAs(UnmanagedType.BStr)] string bstrPropName);
    }

    public enum ShellSpecialFolderConstants
    {
        ssfDRIVES = 0x11
    }
}