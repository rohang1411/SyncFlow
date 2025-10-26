# SyncFlow Release Build Script
param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "2.0.0",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$CreateTag = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SyncFlow Release Build Script v1.0" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$BuildDir = "build"
$ReleaseDir = "release"
$ProjectFile = "SyncFlow/SyncFlow.csproj"
$TestProject = "SyncFlow.Tests/SyncFlow.Tests.csproj"

# Clean previous builds
Write-Host "[CLEAN] Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $BuildDir) { Remove-Item $BuildDir -Recurse -Force }
if (Test-Path $ReleaseDir) { Remove-Item $ReleaseDir -Recurse -Force }

New-Item -ItemType Directory -Path $BuildDir -Force | Out-Null
New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null

Write-Host "[INFO] Building SyncFlow v$Version" -ForegroundColor Green
Write-Host ""

try {
    # Run tests (optional)
    if (-not $SkipTests -and (Test-Path $TestProject)) {
        Write-Host "[TEST] Running unit tests..." -ForegroundColor Yellow
        dotnet test $TestProject --configuration Release --logger "console;verbosity=minimal"
        if ($LASTEXITCODE -ne 0) {
            throw "Tests failed!"
        }
        Write-Host "[TEST] All tests passed!" -ForegroundColor Green
        Write-Host ""
    }

    # Update version in project file
    Write-Host "[VERSION] Updating version to $Version..." -ForegroundColor Yellow
    $projectContent = Get-Content $ProjectFile -Raw
    $projectContent = $projectContent -replace '<Version>[\d\.]+</Version>', "<Version>$Version</Version>"
    $projectContent = $projectContent -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$Version.0</AssemblyVersion>"
    $projectContent = $projectContent -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$Version.0</FileVersion>"
    $projectContent = $projectContent -replace '<InformationalVersion>[\d\.]+</InformationalVersion>', "<InformationalVersion>$Version</InformationalVersion>"
    Set-Content $ProjectFile -Value $projectContent
    Write-Host "[VERSION] Version updated successfully!" -ForegroundColor Green

    # Build Release Configuration
    Write-Host "[BUILD] Building Release configuration..." -ForegroundColor Yellow
    dotnet build $ProjectFile --configuration Release --output "$BuildDir/Release" --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed!"
    }
    Write-Host "[BUILD] Release build completed!" -ForegroundColor Green

    # Publish Self-Contained Executable
    Write-Host "[PUBLISH] Creating self-contained executable..." -ForegroundColor Yellow
    dotnet publish $ProjectFile `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        --output "$BuildDir/Publish" `
        -p:PublishSingleFile=true `
        -p:PublishReadyToRun=true `
        -p:PublishTrimmed=false `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        --verbosity minimal
    
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed!"
    }
    Write-Host "[PUBLISH] Self-contained executable created!" -ForegroundColor Green

    # Create ZIP package
    Write-Host "[PACKAGE] Creating ZIP package..." -ForegroundColor Yellow
    $zipPath = "$ReleaseDir/SyncFlow-v$Version-win-x64.zip"
    Compress-Archive -Path "$BuildDir/Publish/*" -DestinationPath $zipPath -Force
    Write-Host "[PACKAGE] ZIP package created: $zipPath" -ForegroundColor Green

    # Copy standalone executable
    Write-Host "[PACKAGE] Creating standalone executable..." -ForegroundColor Yellow
    $exePath = "$ReleaseDir/SyncFlow-v$Version-win-x64.exe"
    Copy-Item "$BuildDir/Publish/SyncFlow.exe" $exePath
    Write-Host "[PACKAGE] Standalone executable created: $exePath" -ForegroundColor Green

    # Get file information
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Release files created:" -ForegroundColor White
    
    $zipInfo = Get-Item $zipPath
    $exeInfo = Get-Item $exePath
    
    Write-Host "📦 ZIP Package: $($zipInfo.Name) ($([math]::Round($zipInfo.Length / 1MB, 2)) MB)" -ForegroundColor Cyan
    Write-Host "🚀 Executable: $($exeInfo.Name) ($([math]::Round($exeInfo.Length / 1MB, 2)) MB)" -ForegroundColor Cyan
    
    # Create Git tag (optional)
    if ($CreateTag) {
        Write-Host ""
        Write-Host "[GIT] Creating git tag v$Version..." -ForegroundColor Yellow
        git tag -a "v$Version" -m "Release version $Version"
        Write-Host "[GIT] Tag created: v$Version" -ForegroundColor Green
        Write-Host "[GIT] Push tag with: git push origin v$Version" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "🎉 Ready for GitHub release!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor White
    Write-Host "1. Commit and push your changes" -ForegroundColor Gray
    Write-Host "2. Create a new release on GitHub" -ForegroundColor Gray
    Write-Host "3. Upload the ZIP and EXE files" -ForegroundColor Gray
    Write-Host "4. Tag the release as v$Version" -ForegroundColor Gray

} catch {
    Write-Host ""
    Write-Host "❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}