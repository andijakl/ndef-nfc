# NFC / NDEF Library

Easily parse and create standardized NDEF (NFC Data Exchange Format) records for NFC tags and device-to-device communication.

* **NdefLibrary**: platform independent core library (.NET Standard 2.0 and .NET 10). Supports URI, Text, Smart Poster, Geo, Social, Telephone, SMS, Mailto, LaunchApp, Android Application Record, Windows Settings, Bluetooth Secure Simple Pairing and Handover records.
* **NdefLibraryExtension**: Windows platform integration (WinUI 3 / Windows App SDK) to create vCard records from Windows Contacts, iCalendar records from Appointments and Mime/Image records from bitmaps.

## Reading & parsing a Smart Poster

```csharp
// Parse raw byte array (e.g., from the Windows Proximity APIs) to an NDEF message
var ndefMessage = NdefMessage.FromByteArray(rawMessage);

foreach (NdefRecord record in ndefMessage)
{
    if (record.CheckSpecializedType(false) == typeof(NdefSpRecord))
    {
        var spRecord = new NdefSpRecord(record);
        Console.WriteLine("URI: " + spRecord.Uri);
        Console.WriteLine("Titles: " + spRecord.TitleCount());
        Console.WriteLine("Action set: " + spRecord.ActionInUse());
    }
}
```

## Writing a Smart Poster

```csharp
var spRecord = new NdefSpRecord
{
    Uri = "https://www.nfcinteractor.com",
    NfcAction = NdefSpActRecord.NfcActionType.DoAction
};
spRecord.AddTitle(new NdefTextRecord { Text = "NFC Interactor", LanguageCode = "en" });

var message = new NdefMessage { spRecord };
byte[] rawMessage = message.ToByteArray();

// Windows: write to a tag with the Proximity APIs
// device.PublishBinaryMessage("NDEF:WriteTag", rawMessage.AsBuffer());
```

More information, documentation and example apps: https://andijakl.github.io/ndef-nfc/

## License

Dual licensed under LGPL v3 and GPL v3. The NdefLibraryExtension package bundles the vCard library for .NET by David Pinch (LGPL v2.1).
