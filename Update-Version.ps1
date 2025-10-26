# SyncFlow Version Management Script
param(
    [Parameter(Mandatory=$true)]
    [string]$NewVersion,
    
    [Parameter(Mandatory=$false)]
    [switch]$CreateTag = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SyncFlow Version Update Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Validate version format
if ($NewVersion -notmatch '^\d+\.\d+\.\d+$') {
    Write-Host "❌ Invalid version format. Use semantic versioning (e.g., 2.1.0)" -ForegroundColor Red
    exit 1
}

$ProjectFile = "SyncFlow/SyncFlow.csproj"

if (-not (Test-Path $ProjectFile)) {
    Write-Host "❌ Project file not found: $ProjectFile" -ForegroundColor Red
    exit 1
}

Write-Host "📝 Updating version to: $NewVersion" -ForegroundColor Green

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

try {
    # Read current project file
    $projectContent = Get-Content $ProjectFile -Raw
    
    # Extract current version
    if ($projectContent -match '<Version>([\d\.]+)</Version>') {
        $currentVersion = $matches[1]
        Write-Host "📋 Current version: $currentVersion" -ForegroundColor Gray
    } else {
        Write-Host "⚠️  Could not find current version in project file" -ForegroundColor Yellow
    }
    
    if (-not $DryRun) {
        # Update version numbers
        $projectContent = $projectContent -replace '<Version>[\d\.]+</Version>', "<Version>$NewVersion</Version>"
        $projectContent = $projectContent -replace '<AssemblyVersion>[\d\.]+\.0</AssemblyVersion>', "<AssemblyVersion>$NewVersion.0</AssemblyVersion>"
        $projectContent = $projectContent -replace '<FileVersion>[\d\.]+\.0</FileVersion>', "<FileVersion>$NewVersion.0</FileVersion>"
        $projectContent = $projectContent -replace '<InformationalVersion>[\d\.]+</InformationalVersion>', "<InformationalVersion>$NewVersion</InformationalVersion>"
        
        # Write updated content
        Set-Content $ProjectFile -Value $projectContent
        Write-Host "✅ Project file updated successfully!" -ForegroundColor Green
        
        # Update build scripts
        $buildScript = "build-release.bat"
        if (Test-Path $buildScript) {
            $buildContent = Get-Content $buildScript -Raw
            $buildContent = $buildContent -replace 'set VERSION=[\d\.]+', "set VERSION=$NewVersion"
            Set-Content $buildScript -Value $buildContent
            Write-Host "✅ Build script updated!" -ForegroundColor Green
        }
        
        # Create git commit
        Write-Host ""
        Write-Host "📝 Creating git commit..." -ForegroundColor Yellow
        git add $ProjectFile
        if (Test-Path $buildScript) { git add $buildScript }
        git commit -m "chore: bump version to $NewVersion"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Git commit created!" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Git commit failed or no changes to commit" -ForegroundColor Yellow
        }
        
        # Create git tag
        if ($CreateTag) {
            Write-Host "🏷️  Creating git tag..." -ForegroundColor Yellow
            git tag -a "v$NewVersion" -m "Release version $NewVersion"
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Git tag v$NewVersion created!" -ForegroundColor Green
                Write-Host "📤 Push with: git push origin main && git push origin v$NewVersion" -ForegroundColor Cyan
            } else {
                Write-Host "❌ Git tag creation failed!" -ForegroundColor Red
            }
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Version update completed!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📋 Summary:" -ForegroundColor White
    Write-Host "  • New version: $NewVersion" -ForegroundColor Gray
    Write-Host "  • Project file: Updated" -ForegroundColor Gray
    Write-Host "  • Build script: Updated" -ForegroundColor Gray
    if ($CreateTag -and -not $DryRun) {
        Write-Host "  • Git tag: v$NewVersion created" -ForegroundColor Gray
    }
    
    if (-not $DryRun) {
        Write-Host ""
        Write-Host "🚀 Next steps:" -ForegroundColor White
        Write-Host "1. Review changes: git diff HEAD~1" -ForegroundColor Gray
        Write-Host "2. Push changes: git push origin main" -ForegroundColor Gray
        if ($CreateTag) {
            Write-Host "3. Push tag: git push origin v$NewVersion" -ForegroundColor Gray
            Write-Host "4. GitHub Actions will automatically create a release" -ForegroundColor Gray
        } else {
            Write-Host "3. Create tag: git tag -a v$NewVersion -m 'Release version $NewVersion'" -ForegroundColor Gray
            Write-Host "4. Push tag: git push origin v$NewVersion" -ForegroundColor Gray
        }
    }
    
} catch {
    Write-Host ""
    Write-Host "❌ Error updating version: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}