# NdefDemo WinUI 3 Project

This is the modernized WinUI 3 version of the NdefDemo application, targeting .NET 8.0 and Windows 11.

## Project Configuration

- **Target Framework**: .NET 8.0 with Windows 11 SDK (10.0.22621.0)
- **Minimum Platform Version**: Windows 10 version 2004 (10.0.19041.0)
- **UI Framework**: WinUI 3 via Windows App SDK 1.5.240311000 (latest stable)
- **Platforms**: x86, x64, ARM64
- **Package Type**: Unpackaged (WindowsPackageType=None)
- **Self-Contained**: Disabled for broader compatibility

## Project Structure

```
NdefDemoWinUI3/
├── App.xaml                    # Application definition
├── App.xaml.cs                 # Application code-behind
├── MainWindow.xaml             # Main window definition
├── MainWindow.xaml.cs          # Main window code-behind
├── MainPage.xaml               # Main page (placeholder)
├── MainPage.xaml.cs            # Main page code-behind
├── Package.appxmanifest        # Application manifest for packaging
├── app.manifest                # Win32 application manifest
├── NdefDemoWinUI3.csproj       # Project file
├── Assets/                     # Application assets (icons, images)
└── Strings/                    # Localization resources
    └── en-US/
        └── Resources.resw
```

## Key Features

1. **Modern SDK-style project file** with simplified configuration
2. **Windows 11 targeting** with backward compatibility to Windows 10 2004
3. **WinUI 3 framework** for modern Windows UI
4. **Unpackaged deployment** for easier development and distribution
5. **Multi-platform support** (x86, x64, ARM64)

## Dependencies

- Microsoft.WindowsAppSDK (1.5.240311000) - Latest stable Windows App SDK
- Microsoft.Windows.SDK.BuildTools (10.0.22621.2428) - Windows 11 SDK build tools
- Ical.Net (4.2.0) - for calendar functionality

## Build Notes

The project is configured as an unpackaged WinUI 3 application using .NET 8.0 with the latest stable Windows App SDK (1.5.240311000). This provides:

- **Stable .NET 8.0 runtime** - Mature and well-supported platform
- **Latest Windows App SDK** - Modern WinUI 3 features and bug fixes
- **Unpackaged deployment** - Simplified distribution without MSIX complexity
- **Cross-platform compilation** - Support for x86, x64, and ARM64 architectures
- **Windows 11 optimized** - Targeting latest Windows features with Windows 10 compatibility

**Note**: The project requires Visual Studio 2022 or .NET 8.0 SDK for building. The .NET 9 SDK may have compatibility issues with the Windows App SDK packaging tools.

## Next Steps

This project structure provides the foundation for migrating the UWP NdefDemo functionality to WinUI 3. The next tasks will involve:

1. Migrating the UI components from UWP to WinUI 3
2. Updating NFC/proximity API usage for the new platform
3. Implementing the NDEF library integration
4. Adding proper error handling and modern async patterns