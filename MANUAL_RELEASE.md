# 🛠️ Manual Release Guide

If GitHub Actions fails, you can create a release manually using these steps:

## 📋 **Step 1: Build Locally**

```powershell
# Build the release files
.\Build-Release.ps1 -Version "1.0.0"
```

This will create:
- `release/SyncFlow-v1.0.0-win-x64.zip`
- `release/SyncFlow-v1.0.0-win-x64.exe`

## 📋 **Step 2: Create GitHub Release Manually**

1. **Go to your GitHub repository**: https://github.com/rohang1411/SyncFlow
2. **Click "Releases"** in the right sidebar
3. **Click "Create a new release"**
4. **Fill in the details**:
   - **Tag**: `v1.0.0` (select existing tag)
   - **Title**: `SyncFlow v1.0.0`
   - **Description**: Copy the content below

### **Release Description Template:**

```markdown
# SyncFlow v1.0.0

## 🚀 What's New
- Enhanced file transfer system with comprehensive progress tracking
- Storage space validation before transfers begin
- Improved error handling and retry functionality
- Modern UI with backdrop effects and better resource management
- Detailed transfer statistics and file counting

## 📦 Downloads
- **SyncFlow-v1.0.0-win-x64.zip** (~20 MB) - Complete package with all dependencies
- **SyncFlow-v1.0.0-win-x64.exe** (~50 MB) - Standalone executable

## 🔧 System Requirements
- Windows 10 version 1809 or later
- Windows 11 (recommended for best visual effects)
- .NET 8.0 Runtime (included in standalone executable)

## 🛠️ Installation
1. Download either the ZIP package or standalone EXE
2. For ZIP: Extract and run SyncFlow.exe
3. For EXE: Run directly (first launch may take a moment to extract)

## 🐛 Bug Fixes
- Fixed file transfer counting accuracy
- Resolved Settings window resource binding errors
- Fixed Profile Editor backdrop effect issues
- Improved error handling throughout the application

---

**Full Changelog**: https://github.com/rohang1411/SyncFlow/commits/v1.0.0
```

5. **Upload Files**:
   - Drag and drop `SyncFlow-v1.0.0-win-x64.zip`
   - Drag and drop `SyncFlow-v1.0.0-win-x64.exe`

6. **Publish Release**:
   - ✅ Check "Set as the latest release"
   - ❌ Uncheck "Set as a pre-release"
   - Click **"Publish release"**

## 🎯 **Alternative: Use GitHub CLI**

If you have GitHub CLI installed:

```bash
# Create release with files
gh release create v1.0.0 \
  release/SyncFlow-v1.0.0-win-x64.zip \
  release/SyncFlow-v1.0.0-win-x64.exe \
  --title "SyncFlow v1.0.0" \
  --notes-file release-notes.md
```

## 🔧 **Troubleshooting GitHub Actions**

If you want to fix the automated approach:

### **Common Issues:**
1. **Permissions**: Repository Settings → Actions → General → Workflow permissions → "Read and write permissions"
2. **Token**: Make sure `GITHUB_TOKEN` has `contents: write` permission
3. **Branch Protection**: Disable branch protection rules temporarily

### **Check Repository Settings:**
1. Go to **Settings** → **Actions** → **General**
2. Under **Workflow permissions**, select:
   - ✅ **"Read and write permissions"**
   - ✅ **"Allow GitHub Actions to create and approve pull requests"**
3. Click **"Save"**

### **Re-run Failed Action:**
1. Go to **Actions** tab
2. Click on the failed workflow
3. Click **"Re-run all jobs"**

## 📞 **Need Help?**

If you're still having issues:
1. Check the **Actions** tab for detailed error logs
2. Try the manual release process above
3. The build files should work regardless of how you create the release

---

**The important thing is that your application builds successfully - the release creation is just packaging! 🎉**