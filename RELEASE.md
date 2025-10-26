# 🚀 SyncFlow Release Guide

This guide covers the complete process for creating and publishing SyncFlow releases.

## 📋 Prerequisites

- Windows 10/11 with PowerShell 5.1+
- .NET 8.0 SDK installed
- Git configured with GitHub access
- Visual Studio 2022 or VS Code (optional)

## 🔄 Version Management

SyncFlow uses [Semantic Versioning](https://semver.org/):
- **MAJOR.MINOR.PATCH** (e.g., 2.1.0)
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Current Version: 2.0.0

## 🛠️ Release Process

### Method 1: Automated Release (Recommended)

1. **Update Version**
   ```powershell
   # Update to new version and create tag
   .\Update-Version.ps1 -NewVersion "2.1.0" -CreateTag
   ```

2. **Push Changes**
   ```bash
   git push origin main
   git push origin v2.1.0
   ```

3. **Automatic Build**
   - GitHub Actions automatically builds and creates release
   - ZIP and EXE files are generated and uploaded
   - Release notes are auto-generated

### Method 2: Manual Release

1. **Update Version**
   ```powershell
   .\Update-Version.ps1 -NewVersion "2.1.0"
   ```

2. **Build Release**
   ```powershell
   # Using PowerShell script (recommended)
   .\Build-Release.ps1 -Version "2.1.0" -CreateTag
   
   # Or using batch file
   .\build-release.bat
   ```

3. **Create GitHub Release**
   - Go to GitHub repository
   - Click "Releases" → "Create a new release"
   - Tag: `v2.1.0`
   - Title: `SyncFlow v2.1.0`
   - Upload files from `release/` folder
   - Publish release

## 📁 Build Outputs

After building, you'll find these files in the `release/` folder:

```
release/
├── SyncFlow-v2.1.0-win-x64.zip    # Complete package (~15-25 MB)
└── SyncFlow-v2.1.0-win-x64.exe    # Standalone executable (~40-60 MB)
```

### File Descriptions

- **ZIP Package**: Contains all application files, smaller download
- **Standalone EXE**: Self-contained executable, no installation required

## 🔧 Build Scripts

### PowerShell Script (`Build-Release.ps1`)
```powershell
# Basic build
.\Build-Release.ps1

# Build with specific version
.\Build-Release.ps1 -Version "2.1.0"

# Skip tests
.\Build-Release.ps1 -SkipTests

# Create git tag
.\Build-Release.ps1 -CreateTag
```

### Batch Script (`build-release.bat`)
- Simple double-click execution
- Hardcoded version (edit script to change)
- Creates both ZIP and EXE files

### Version Update Script (`Update-Version.ps1`)
```powershell
# Update version only
.\Update-Version.ps1 -NewVersion "2.1.0"

# Update version and create tag
.\Update-Version.ps1 -NewVersion "2.1.0" -CreateTag

# Dry run (preview changes)
.\Update-Version.ps1 -NewVersion "2.1.0" -DryRun
```

## 🤖 GitHub Actions

The repository includes automated CI/CD:

### Triggers
- **Tag Push**: `git push origin v2.1.0`
- **Manual Dispatch**: GitHub Actions tab → "Build and Release"

### Workflow Steps
1. Checkout code
2. Setup .NET 8.0
3. Update project version
4. Run tests
5. Build release configuration
6. Publish self-contained executable
7. Create ZIP and EXE artifacts
8. Generate release notes
9. Create GitHub release

## 📝 Release Checklist

### Pre-Release
- [ ] All tests passing
- [ ] Code reviewed and merged
- [ ] Version number decided
- [ ] Release notes prepared
- [ ] Breaking changes documented

### Release Process
- [ ] Version updated in project file
- [ ] Build completed successfully
- [ ] Both ZIP and EXE files created
- [ ] Files tested on clean Windows machine
- [ ] Git tag created and pushed
- [ ] GitHub release published

### Post-Release
- [ ] Release announcement posted
- [ ] Documentation updated
- [ ] Next version planning
- [ ] Issues/feedback monitoring

## 🐛 Troubleshooting

### Build Failures

**"dotnet command not found"**
- Install .NET 8.0 SDK from Microsoft

**"Access denied" errors**
- Run PowerShell as Administrator
- Check antivirus software

**"Git tag already exists"**
```bash
# Delete local tag
git tag -d v2.1.0

# Delete remote tag
git push origin --delete v2.1.0
```

### File Size Issues

**EXE file too large (>100MB)**
- Enable trimming: Set `<PublishTrimmed>true</PublishTrimmed>`
- Review dependencies in project file

**ZIP file corruption**
- Use PowerShell compression instead of Windows built-in
- Check available disk space

## 📊 Release History

| Version | Date | Size (ZIP/EXE) | Key Features |
|---------|------|----------------|--------------|
| 2.0.0   | 2024 | ~20MB/~50MB   | Enhanced transfers, storage validation, UI fixes |
| 1.0.0   | 2024 | ~15MB/~40MB   | Initial release |

## 🔗 Useful Commands

```powershell
# Check current version
Select-String -Path "SyncFlow/SyncFlow.csproj" -Pattern "<Version>"

# Build and test locally
dotnet build SyncFlow/SyncFlow.csproj -c Release
dotnet test SyncFlow.Tests/SyncFlow.Tests.csproj

# Create release manually
dotnet publish SyncFlow/SyncFlow.csproj -c Release -r win-x64 --self-contained -o publish

# Check git status
git status
git log --oneline -10
git tag -l
```

## 📞 Support

For build issues or questions:
1. Check this guide first
2. Review GitHub Actions logs
3. Create an issue in the repository
4. Include build logs and error messages

---

**Happy Releasing! 🎉**