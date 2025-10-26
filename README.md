# 🚀 SyncFlow

**Modern Windows File Transfer Utility with Enhanced Progress Tracking**

SyncFlow is a professional-grade file synchronization application built with WPF and .NET 8, featuring a modern Fluent Design interface, comprehensive transfer validation, and detailed progress reporting.

![SyncFlow](https://img.shields.io/badge/version-1.0.0-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

## ✨ Features

### 🎯 Core Functionality
- **Multi-Folder Mapping**: Configure multiple source → destination folder pairs
- **Profile Management**: Save, edit, and organize transfer profiles
- **Enhanced Progress Tracking**: Real-time file counts, transfer speeds, and detailed statistics
- **Storage Validation**: Pre-transfer disk space checking with user warnings
- **Error Handling**: Comprehensive error reporting with retry functionality
- **Transfer Verification**: Post-transfer validation to ensure data integrity

### 🎨 Modern Interface
- **Fluent Design**: Modern Windows 11-style interface with Mica backdrop effects
- **Dark Theme**: Professional dark theme with smooth animations
- **Responsive Layout**: Adaptive UI that works on different screen sizes
- **Glass Effects**: Beautiful visual effects with graceful fallback support

### 🔧 Advanced Features
- **Self-Contained Deployment**: Single executable with no dependencies
- **Configurable Settings**: Customizable themes, storage locations, and preferences
- **Import/Export**: Backup and restore profile configurations
- **Detailed Logging**: Comprehensive error logging and debugging information

## 📥 Download

### Latest Release: v1.0.0

| Package | Size | Description |
|---------|------|-------------|
| [📦 SyncFlow-v1.0.0-win-x64.zip](../../releases/latest) | ~20 MB | Complete package with all files |
| [🚀 SyncFlow-v1.0.0-win-x64.exe](../../releases/latest) | ~50 MB | Standalone executable |

## 🔧 System Requirements

- **Operating System**: Windows 10 version 1809 or later
- **Recommended**: Windows 11 (for best visual effects)
- **Architecture**: x64 (64-bit)
- **Runtime**: .NET 8.0 (included in standalone executable)
- **Memory**: 512 MB RAM minimum, 1 GB recommended
- **Storage**: 100 MB free space

## 🚀 Quick Start

### Installation
1. Download the standalone EXE or ZIP package from [Releases](../../releases/latest)
2. For ZIP: Extract and run `SyncFlow.exe`
3. For EXE: Run directly (first launch may take a moment to extract)

### Basic Usage
1. **Create Profile**: Click "Create New Profile" and give it a name
2. **Add Mappings**: Click "Add Folder Mapping" to set source → destination pairs
3. **Configure Options**: Set overwrite preferences and other settings
4. **Start Transfer**: Click "Start Transfer" to begin file synchronization
5. **Monitor Progress**: Watch real-time progress with detailed statistics

## 🎯 What's New in v1.0.0

### 🚀 Enhanced Transfer System
- **Accurate File Counting**: Recursive scanning ensures all files are counted correctly
- **Storage Space Validation**: Pre-transfer checks prevent partial transfers due to insufficient space
- **Detailed Progress Tracking**: See exact file counts, success/failure rates, and transfer speeds
- **Retry Functionality**: Automatically retry failed transfers with detailed error reporting

### 🎨 UI Improvements
- **Fixed Resource Bindings**: Resolved Settings window and Profile Editor crashes
- **Backdrop Effects**: Proper Mica/Acrylic effects with graceful fallback
- **Enhanced Error Handling**: Better user feedback and error recovery
- **Improved Responsiveness**: Smoother animations and faster UI updates

### 🔧 Technical Enhancements
- **Comprehensive Testing**: Full test suite for transfer functionality
- **Better Architecture**: Modular design with dependency injection
- **Enhanced Logging**: Detailed logging for debugging and troubleshooting
- **Performance Optimizations**: Faster file operations and reduced memory usage

## 🛠️ Development

### Prerequisites
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Windows 10/11 SDK

### Building from Source
```bash
# Clone the repository
git clone https://github.com/yourusername/syncflow.git
cd syncflow

# Restore dependencies
dotnet restore

# Build the application
dotnet build SyncFlow/SyncFlow.csproj -c Release

# Run the application
dotnet run --project SyncFlow/SyncFlow.csproj
```

### Running Tests
```bash
# Run all tests
dotnet test SyncFlow.Tests/SyncFlow.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📖 Documentation

- [🚀 Release Guide](RELEASE.md) - Complete guide for creating releases
- [🔧 Build Scripts](Build-Release.ps1) - Automated build and packaging
- [📋 Version Management](Update-Version.ps1) - Version update utilities

## 🤝 Contributing

We welcome contributions! Please feel free to:

1. **Report Issues**: Use the [Issues](../../issues) tab to report bugs or request features
2. **Submit Pull Requests**: Fork the repository and submit your improvements
3. **Improve Documentation**: Help us make the documentation better
4. **Share Feedback**: Let us know how you're using SyncFlow

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes and add tests
4. Commit your changes (`git commit -m 'Add amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **WPF-UI**: Modern WPF controls and styling
- **Microsoft**: .NET 8 and WPF framework
- **Community**: Feedback and contributions from users

## 📞 Support

- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Documentation**: [Release Guide](RELEASE.md)

---

**Made with ❤️ for the Windows community**

*SyncFlow - Making file transfers simple, reliable, and beautiful.*