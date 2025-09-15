# NdefDemoWinUI3 Test Suite

This test project provides comprehensive testing for the NFC functionality in the NdefDemoWinUI3 application.

## Test Coverage

The test suite covers the following areas as specified in task 12 of the modernization requirements:

### 1. NDEF Record Creation and Parsing Tests (`NdefRecordTests.cs`)
- **URI Records**: Creation and parsing of URI/URL records
- **Mailto Records**: Email record creation with address, subject, and body
- **vCard Records**: Business card records with contact information
- **Launch App Records**: Application launch records with platform-specific app IDs
- **Geo Records**: Geographic location records for maps integration
- **Windows Settings Records**: Windows-specific settings records
- **Smart Poster Records**: Complex records with multiple data elements
- **Text Records**: Simple text records with language codes
- **Message Serialization**: NDEF message serialization and deserialization
- **Error Handling**: Invalid NDEF data parsing and error scenarios

### 2. NFC Device Initialization and Subscription Management Tests (`NfcDeviceTests.cs`)
- **Device Initialization**: Testing NFC hardware detection and initialization
- **Subscription Management**: NDEF message subscription and unsubscription
- **Event Handling**: Device arrival and departure events
- **Message Reception**: NDEF message parsing from received data
- **Resource Cleanup**: Proper disposal and resource management
- **Thread Safety**: DispatcherQueue usage for UI thread marshaling

### 3. NFC Publishing Scenarios Tests (`NfcPublishingTests.cs`)
- **Tag Writing**: Writing various NDEF record types to NFC tags
- **Device-to-Device**: Publishing records for device-to-device transfer
- **Multiple Records**: Publishing complex messages with multiple records
- **Tag Locking**: Publishing tag lock commands
- **Message Size Validation**: Validating message sizes against tag capacity
- **Publication Management**: Starting and stopping publication operations
- **Transmission Events**: Handling message transmission completion

### 4. Error Handling Tests (`NfcErrorHandlingTests.cs`)
- **Missing Hardware**: Graceful handling when NFC hardware is not available
- **Permission Errors**: Handling access denied scenarios
- **Device Disconnection**: Managing device disconnection during operations
- **Corrupted Data**: Handling malformed NDEF data
- **Tag Errors**: Read-only tags, insufficient space, and other tag-related errors
- **User-Friendly Messages**: Converting system exceptions to readable error messages
- **Operation Validation**: Pre-validating operations before execution

### 5. Integration Tests (`MainPageIntegrationTests.cs`)
- **MainPage Construction**: Verifying MainPage can be instantiated
- **UI Integration**: Testing integration with WinUI 3 framework
- **Resource Loading**: Localized string resource loading
- **End-to-End Scenarios**: Complete workflow testing specifications

## Requirements Coverage

This test suite addresses the following requirements from the modernization specification:

- **Requirement 3.1**: NFC tag reading functionality works identically to UWP version
- **Requirement 3.2**: NFC tag writing functionality works identically to UWP version  
- **Requirement 3.3**: NDEF record creation features function as expected
- **Requirement 6.1**: Build process compiles without warnings related to deprecated APIs

## Running the Tests

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- Windows 11 with Windows App SDK
- NFC hardware (optional - tests will adapt based on availability)

### Command Line
```bash
# Navigate to the test project directory
cd NdefDemo/NdefDemoWinUI3.Tests

# Restore packages
dotnet restore

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio
1. Open the `NdefDemo.sln` solution
2. Build the solution (Ctrl+Shift+B)
3. Open Test Explorer (Test → Test Explorer)
4. Run all tests or select specific test categories

### Test Categories

Tests are organized into the following categories:

- **Unit Tests**: Fast, isolated tests that don't require hardware
- **Integration Tests**: Tests that verify component interaction
- **Hardware Tests**: Tests that require actual NFC hardware (marked as inconclusive if hardware unavailable)
- **End-to-End Specifications**: Test specifications for manual verification with hardware

## Test Environment Considerations

### NFC Hardware Availability
- Tests automatically detect NFC hardware availability
- Hardware-dependent tests are marked as inconclusive when hardware is not available
- Mock objects are used to simulate NFC device behavior in unit tests

### UI Context
- Some integration tests require a WinUI 3 UI context
- Tests gracefully handle scenarios where UI context is not available
- Mock implementations are provided for testing without full UI framework

### Performance
- Performance measurement utilities are included for timing-sensitive operations
- Tests include assertions for reasonable execution times
- Memory usage patterns are validated where applicable

## Test Data

The test suite includes:
- **Valid NDEF Messages**: Pre-created valid NDEF data for testing
- **Invalid Data**: Malformed data to test error handling
- **Large Messages**: Data near tag capacity limits
- **Edge Cases**: Boundary conditions and unusual scenarios

## Continuous Integration

The test suite is designed to run in CI/CD environments:
- No external dependencies beyond .NET and Windows APIs
- Graceful handling of missing hardware
- Detailed logging for troubleshooting
- Exit codes that indicate test success/failure

## Extending the Tests

To add new tests:

1. **Create new test class** following the naming convention `*Tests.cs`
2. **Add appropriate test attributes** (`[TestClass]`, `[TestMethod]`)
3. **Use test utilities** from `TestConfiguration.cs` for common operations
4. **Follow AAA pattern** (Arrange, Act, Assert) in test methods
5. **Add appropriate documentation** explaining what the test validates

## Known Limitations

- Integration tests require WinUI 3 test framework for full functionality
- Hardware-dependent tests require actual NFC hardware
- Some Windows-specific APIs may not be available in all test environments
- Performance tests may vary based on hardware capabilities

## Troubleshooting

### Common Issues

1. **Tests marked as inconclusive**: Usually indicates missing NFC hardware or UI context
2. **Build errors**: Ensure all NuGet packages are restored and .NET 8.0 SDK is installed
3. **Permission errors**: Run tests with appropriate permissions for NFC access
4. **Timeout errors**: May indicate hardware communication issues

### Debug Information

Tests include extensive logging through the `TestConfiguration.TestUtilities` class. Check test output for detailed information about test execution and any issues encountered.