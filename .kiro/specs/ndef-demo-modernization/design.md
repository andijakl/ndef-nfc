# Design Document

## Overview

This design outlines the modernization of the NdefDemo application from UWP (Universal Windows Platform) to WinUI 3 with .NET 8.0. The modernization will maintain all existing NFC functionality while adopting modern Windows development practices, improved performance, and better integration with Windows 11.

The current application demonstrates NFC tag reading/writing capabilities using the NDEF Library. It includes features for creating and reading various NDEF record types including business cards, URIs, images, maps, and custom application launch records.

## Architecture

### Current Architecture (UWP)
- **Framework**: UWP with .NET Core UWP
- **UI Framework**: UWP XAML
- **Project Format**: Legacy .csproj with PackageReference
- **NFC APIs**: Windows.Networking.Proximity
- **Packaging**: APPX with UWP manifest
- **Target Platform**: Windows 10 Universal

### Target Architecture (WinUI 3)
- **Framework**: .NET 8.0
- **UI Framework**: WinUI 3 (Microsoft.WindowsAppSDK)
- **Project Format**: Modern SDK-style .csproj
- **NFC APIs**: Windows.Networking.Proximity (maintained for compatibility)
- **Packaging**: MSIX with Windows App SDK manifest
- **Target Platform**: Windows 11 with Windows App SDK

### Key Architectural Changes

1. **Project Structure Migration**
   - Convert from UWP project template to WinUI 3 project template
   - Update project file to use modern SDK-style format
   - Replace UWP-specific project properties with WinUI 3 equivalents

2. **Framework Migration**
   - Upgrade from .NET Core UWP to .NET 8.0
   - Replace Microsoft.NETCore.UniversalWindowsPlatform with Microsoft.WindowsAppSDK
   - Update all NuGet package references to compatible versions

3. **Library Dependencies**
   - Replace NuGet package references to NdefLibrary with local project references
   - Update to use NdefLibrary.NET (.NET 8.0) and VcardLibrary.NET projects
   - Maintain Ical.Net dependency for calendar functionality

## Components and Interfaces

### Core Components

#### 1. Application Entry Point (App.xaml/App.xaml.cs)
**Current Implementation**: UWP Application class
**Target Implementation**: WinUI 3 Application class

**Changes Required**:
- Update XAML namespace declarations from UWP to WinUI 3
- Modify App.xaml.cs to inherit from Microsoft.UI.Xaml.Application
- Update Window management to use Microsoft.UI.Xaml.Window
- Replace Windows.UI.Xaml references with Microsoft.UI.Xaml

#### 2. Main Page (MainPage.xaml/MainPage.xaml.cs)
**Current Implementation**: UWP Page with SplitView layout
**Target Implementation**: WinUI 3 Page with maintained layout

**Changes Required**:
- Update XAML namespace declarations
- Replace Windows.UI.Xaml references with Microsoft.UI.Xaml
- Update color and styling resources to WinUI 3 equivalents
- Maintain existing UI layout and functionality
- Update dispatcher usage from CoreDispatcher to DispatcherQueue

#### 3. NFC Service Layer
**Current Implementation**: Direct ProximityDevice usage
**Target Implementation**: Maintained with updated threading model

**Changes Required**:
- Update threading model to use DispatcherQueue instead of CoreDispatcher
- Maintain Windows.Networking.Proximity API usage (still supported)
- Update async/await patterns to modern standards
- Implement proper nullable reference type handling

#### 4. NDEF Record Handlers
**Current Implementation**: Uses NdefLibraryUwp namespace
**Target Implementation**: Uses NdefLibrary.NET namespace

**Changes Required**:
- Replace `using NdefLibraryUwp.Ndef;` with `using NdefLibrary.Ndef;`
- Update image handling from WriteableBitmap to compatible WinUI 3 approach
- Maintain all existing record type support (URI, vCard, Image, LaunchApp, etc.)

### Interface Compatibility Matrix

| Component | UWP API | WinUI 3 API | Migration Strategy |
|-----------|---------|-------------|-------------------|
| Application | Windows.UI.Xaml.Application | Microsoft.UI.Xaml.Application | Direct replacement |
| Window | Windows.UI.Xaml.Window | Microsoft.UI.Xaml.Window | Direct replacement |
| Page | Windows.UI.Xaml.Controls.Page | Microsoft.UI.Xaml.Controls.Page | Direct replacement |
| Dispatcher | Windows.UI.Core.CoreDispatcher | Microsoft.UI.Dispatching.DispatcherQueue | API update required |
| ProximityDevice | Windows.Networking.Proximity | Windows.Networking.Proximity | No change |
| Contact APIs | Windows.ApplicationModel.Contacts | Windows.ApplicationModel.Contacts | No change |
| File Picker | Windows.Storage.Pickers | Windows.Storage.Pickers | No change |

## Data Models

### NDEF Record Types (Maintained)
All existing NDEF record types will be preserved:

1. **NdefUriRecord** - URI/URL records
2. **NdefMailtoRecord** - Email records
3. **NdefVcardRecord** - Business card records
4. **NdefMimeImageRecord** - Image records
5. **NdefLaunchAppRecord** - Application launch records
6. **NdefGeoRecord** - Geographic location records
7. **NdefWindowsSettingsRecord** - Windows settings records
8. **NdefSpRecord** - Smart Poster records

### Application State Model
```csharp
public class NfcApplicationState
{
    public ProximityDevice Device { get; set; }
    public long SubscriptionId { get; set; }
    public long PublishingMessageId { get; set; }
    public bool IsInitialized => Device != null;
    public bool IsSubscribed => SubscriptionId != 0;
    public bool IsPublishing => PublishingMessageId != 0;
}
```

## Error Handling

### Current Error Handling Patterns (Maintained)
- Try-catch blocks around NFC operations
- User-friendly error messages via ResourceLoader
- Graceful degradation when NFC is not available
- Proper cleanup of subscriptions and publications

### Enhanced Error Handling for WinUI 3
- Implement nullable reference types for better null safety
- Add structured logging for debugging
- Improve exception handling for Windows App SDK specific scenarios
- Maintain backward compatibility for error scenarios

### Error Scenarios
1. **NFC Hardware Not Available**: Display appropriate message, disable NFC features
2. **Permission Denied**: Guide user to enable NFC in Windows settings
3. **Tag Read/Write Failures**: Provide specific error messages and retry options
4. **Library Loading Issues**: Handle missing dependencies gracefully

## Testing Strategy

### Unit Testing Approach
- **Framework**: MSTest or xUnit with .NET 8.0
- **Mocking**: Moq for ProximityDevice and other external dependencies
- **Coverage**: Focus on NDEF record creation and parsing logic

### Integration Testing
- **NFC Simulation**: Use test NDEF messages for validation
- **UI Testing**: WinUI 3 compatible UI automation tests
- **Cross-platform**: Ensure compatibility across Windows 11 versions

### Manual Testing Scenarios
1. **NFC Tag Reading**: Test with various NDEF tag types
2. **NFC Tag Writing**: Verify all record types write correctly
3. **UI Responsiveness**: Ensure smooth operation during NFC operations
4. **Error Handling**: Test scenarios with no NFC hardware
5. **Windows 11 Integration**: Verify proper system integration

### Performance Testing
- **Startup Time**: Measure application launch performance
- **Memory Usage**: Monitor memory consumption during NFC operations
- **Battery Impact**: Assess power consumption with continuous NFC monitoring

## Migration Strategy

### Phase 1: Project Structure Update
1. Create new WinUI 3 project structure
2. Migrate project files and assets
3. Update project references and dependencies
4. Configure build and packaging settings

### Phase 2: Code Migration
1. Update namespace declarations and using statements
2. Migrate XAML files to WinUI 3 format
3. Update C# code to use WinUI 3 APIs
4. Implement nullable reference types
5. Update async/await patterns

### Phase 3: Library Integration
1. Remove NuGet package references to NdefLibrary
2. Add project references to local NdefLibrary.NET
3. Update using statements to use .NET version
4. Test all NDEF record functionality

### Phase 4: Testing and Validation
1. Comprehensive testing of all features
2. Performance validation
3. Windows 11 compatibility verification
4. Documentation updates

## Deployment and Packaging

### Current Packaging (UWP)
- **Format**: APPX package
- **Manifest**: Package.appxmanifest with UWP schema
- **Distribution**: Microsoft Store or sideloading
- **Capabilities**: proximity, internetClient

### Target Packaging (WinUI 3)
- **Format**: MSIX package
- **Manifest**: Package.appxmanifest with Windows App SDK schema
- **Distribution**: Microsoft Store, winget, or direct distribution
- **Capabilities**: Maintained proximity and internetClient

### Packaging Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <UseWinUI>true</UseWinUI>
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
  </PropertyGroup>
</Project>
```

## Security Considerations

### NFC Security (Maintained)
- Validate all incoming NDEF data
- Sanitize user input for outgoing records
- Handle malformed NDEF messages gracefully
- Implement proper permission handling

### Windows App SDK Security
- Follow Windows App SDK security best practices
- Implement proper capability declarations
- Ensure secure handling of file operations
- Validate all external data sources

## Performance Optimizations

### WinUI 3 Specific Optimizations
- Leverage improved XAML compilation
- Utilize native performance improvements
- Implement efficient memory management
- Optimize startup time with lazy loading

### NFC Operation Optimizations
- Implement proper async patterns for NFC operations
- Cache frequently used NDEF records
- Optimize image handling for NFC tags
- Implement efficient UI updates during NFC events

## Compatibility and Dependencies

### Minimum System Requirements
- **OS**: Windows 11 version 22H2 or later
- **Runtime**: .NET 8.0 Runtime
- **Hardware**: NFC-capable device
- **Windows App SDK**: Latest stable version

### Dependency Management
- **Local Projects**: NdefLibrary.NET, VcardLibrary.NET
- **NuGet Packages**: Microsoft.WindowsAppSDK, Ical.Net
- **System APIs**: Windows.Networking.Proximity, Windows.ApplicationModel.Contacts

### Backward Compatibility
- Maintain NDEF message format compatibility
- Preserve user data and settings migration path
- Support existing NFC tag formats
- Ensure feature parity with UWP version