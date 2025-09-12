# NDEF Library for .NET

This is a modernized version of the NDEF Library, updated to target .NET 8. The original library was targeted for .NET Standard 1.4 and had dependencies on UWP. This version removes all UWP dependencies and is fully cross-platform.

## Changes

*   **Upgraded to .NET 8:** The library now targets .NET 8, which provides access to the latest C# features and performance improvements.
*   **Removed UWP Dependencies:** All dependencies on UWP APIs have been removed. This includes the `Windows.ApplicationModel.Contacts` and `Windows.ApplicationModel.Appointments` namespaces, as well as the `Windows.UI.Xaml.Media.Imaging` namespace.
*   **Replaced with Cross-Platform Alternatives:**
    *   The UWP `Contact` and `Appointment` classes have been replaced with custom DTOs (`NdefContact` and `NdefAppointment`).
    *   Image manipulation is now handled by the `SixLabors.ImageSharp` library, which is a cross-platform, open-source library.
*   **New Project Structure:** The library is now split into two projects:
    *   `NdefLibrary.NET`: The main library.
    *   `VcardLibrary.NET`: A helper library for working with vCards.

## Usage

To use the library, simply add a reference to the `NdefLibrary.NET.csproj` project. You can then use the classes in the `NdefLibrary.Ndef` namespace to work with NDEF records.

```csharp
using NdefLibrary.Ndef;

// Create a new NDEF message
var message = new NdefMessage();

// Add a text record
var textRecord = new NdefTextRecord
{
    Text = "Hello, World!",
    Language = "en"
};
message.Add(textRecord);

// Encode the message to a byte array
byte[] encodedMessage = message.ToByteArray();
```
