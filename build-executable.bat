@echo off
echo ========================================
echo Building SyncFlow Executable
echo ========================================
echo.

echo Creating self-contained executable...
dotnet publish SyncFlow -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./dist

if %errorlevel% neq 0 (
    echo Build failed.
    pause
    exit /b 1
)

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Executable location: ./dist/SyncFlow.exe
echo File size: 
dir /s "dist\SyncFlow.exe" | find "SyncFlow.exe"
echo.
echo You can now distribute the SyncFlow.exe file.
echo It will run on any Windows 10/11 machine without requiring .NET installation.
echo.
pause