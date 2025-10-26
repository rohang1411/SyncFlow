@echo off
echo ========================================
echo SyncFlow Release Build Script
echo ========================================

:: Set version (update this for each release)
set VERSION=1.0.0
set BUILD_DIR=build
set RELEASE_DIR=release

:: Clean previous builds
echo Cleaning previous builds...
if exist %BUILD_DIR% rmdir /s /q %BUILD_DIR%
if exist %RELEASE_DIR% rmdir /s /q %RELEASE_DIR%

mkdir %BUILD_DIR%
mkdir %RELEASE_DIR%

echo.
echo Building SyncFlow v%VERSION%...
echo.

:: Build Release Configuration
echo [1/4] Building Release Configuration...
dotnet build SyncFlow/SyncFlow.csproj -c Release -o %BUILD_DIR%/Release
if %ERRORLEVEL% neq 0 (
    echo ERROR: Release build failed!
    pause
    exit /b 1
)

:: Publish Self-Contained Executable
echo [2/4] Publishing Self-Contained Executable...
dotnet publish SyncFlow/SyncFlow.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o %BUILD_DIR%/Publish
if %ERRORLEVEL% neq 0 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

:: Create ZIP package
echo [3/4] Creating ZIP package...
powershell -Command "Compress-Archive -Path '%BUILD_DIR%/Publish/*' -DestinationPath '%RELEASE_DIR%/SyncFlow-v%VERSION%-win-x64.zip' -Force"
if %ERRORLEVEL% neq 0 (
    echo ERROR: ZIP creation failed!
    pause
    exit /b 1
)

:: Copy standalone executable
echo [4/4] Copying standalone executable...
copy "%BUILD_DIR%\Publish\SyncFlow.exe" "%RELEASE_DIR%\SyncFlow-v%VERSION%-win-x64.exe"
if %ERRORLEVEL% neq 0 (
    echo ERROR: EXE copy failed!
    pause
    exit /b 1
)

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Release files created:
echo - %RELEASE_DIR%/SyncFlow-v%VERSION%-win-x64.zip
echo - %RELEASE_DIR%/SyncFlow-v%VERSION%-win-x64.exe
echo.
echo File sizes:
for %%f in ("%RELEASE_DIR%\*") do echo %%~nxf: %%~zf bytes

echo.
echo Ready for GitHub release!
pause