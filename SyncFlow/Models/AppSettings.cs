using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SyncFlow.Models
{
    /// <summary>
    /// Application settings model
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        private bool _isDarkMode = true;
        private bool _isTransparencyEnabled = false;
        private bool _enableGlassEffect = true;
        private double _transparencyAmount = 80.0;
        private bool _enableAnimations = true;
        private string _profileStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SyncFlow", "Profiles");

        /// <summary>
        /// Whether dark theme is enabled
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether window transparency/acrylic effect is enabled
        /// </summary>
        public bool IsTransparencyEnabled
        {
            get => _isTransparencyEnabled;
            set
            {
                if (_isTransparencyEnabled != value)
                {
                    _isTransparencyEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether glass/liquid glass effect is enabled
        /// </summary>
        public bool EnableGlassEffect
        {
            get => _enableGlassEffect;
            set
            {
                if (_enableGlassEffect != value)
                {
                    _enableGlassEffect = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Transparency amount (0-100)
        /// </summary>
        public double TransparencyAmount
        {
            get => _transparencyAmount;
            set
            {
                var clampedValue = Math.Max(10, Math.Min(100, value));
                if (Math.Abs(_transparencyAmount - clampedValue) > 0.1)
                {
                    _transparencyAmount = clampedValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether animations are enabled
        /// </summary>
        public bool EnableAnimations
        {
            get => _enableAnimations;
            set
            {
                if (_enableAnimations != value)
                {
                    _enableAnimations = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Path where profile data is stored
        /// </summary>
        public string ProfileStoragePath
        {
            get => _profileStoragePath;
            set
            {
                if (_profileStoragePath != value)
                {
                    _profileStoragePath = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

