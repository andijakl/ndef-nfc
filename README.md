NDEF Library for Proximity APIs / NFC
========

Easily parse and create NFC tags that contain standard-based NDEF messages.

Available in C# and JavaScript (for HTML5-based apps).

The library download comes with complete example apps that demonstrates reading and writing tags using the NDEF Library.



## Background - NFC and NDEF

NFC tags as well as the content sent in device-to-device communication when tapping two phones is based on certain standards by the [NFC Forum](http://www.nfc-forum.org/) (called NDEF – NFC Data Exchange format). Luckily, these standards were well received and nearly all manufacturers are part of the standardization body. This ensures that public NFC tags can actually be read by all mobile phones today.

When it comes to storing data on NFC tags that can have as little writable storage as around 40 bytes, very efficient and complex data storage schemes are necessary. The downside is that most operating systems do integrate the NFC data transmission at the base level, but offer developers very little support for the NDEF standards on top. Obviously, reading those technical documents isn’t generally too much fun – to create an own implementation of a message that stores a simple URL on a tag, a developer would need to read and understand 59 pages of specifications.

As this is a lot work, there is the risk of people creating own solutions, leading to a fragmented NFC ecosystem



## The NFC Library

The open source NFC / NDEF Library contains a large set of classes that take care of formatting your data according to NDEF standards, so that these can be directly written to NFC tags or sent to other devices.

In your app, you choose the corresponding record type (e.g., for URLs, emails or geo tags) and provide the necessary data. The library creates an NDEF message out of the data, which you can directly send to the NFC stack in your operating system as a byte array, which takes care of writing it to a tag or publishing it to another device (using the SNEP protocol).

Additionally, the library can parse NDEF byte arrays that you read from tags or receive from other devices and create a list (NDEF Message) of data classes (NDEF records) that you can easily analyze and use in your app.

For Windows (Phone) 8+, the NFC stack is represented through the Proximity APIs - they encapsulate NFC hardware communication and basic NDEF formatting for a very limited subset of the NDEF standards. This missing part is added by this NDEF library.



## Availability

The NFC / NDEF library is available in C# and JavaScript and can therefore be used on any operating system.

To keep up to date, either follow this project or [follow me on Twitter](https://twitter.com/andijakl).

### C# Version

The library is available as a ready-made portable class library, which can be used on the Windows 8(.1) (WinRT) platform, as well as on Windows Phone 8(.1). Both platforms provide support for interacting with the NFC hardware through the Proximity APIs.

Additional platform-specific functionality is added through the the separate extension library. It integrates with the platform APIs for WinRT / WP8.0 and allows real-life tasks like creating a business card record based on a Contact from the Windows 8 address book.

Two example apps are available - for Windows 8.1 as well as Windows Phone 8.0. In addition to the library download on this page, you can use the NuGet package manager of Visual Studio to easily integrate the library with your project.

### JavaScript / HTML5 Version

The new JavaScript port of the library provides the most important NDEF types also to HTML5 / JavaScript apps.



## Feature Overview

#### Reusable NDEF classes

* Parse NDEF message & records from raw byte arrays 
* Extract all information from the bits & bytes contained in the record 
* Create standard compliant records just by providing your data 
* Identify the exact record type when reading an NDEF message
* Records check their contents for validity according to standards
* Can throw NdefException in case of content validity issues, with translatable messages defined in a resource file
* Fully documented source code, following Doxygen standards

#### Supported NDEF records:
* URI: the most common type: any kind of URI, for example an Internet address, email link or any custom URI scheme.
* Smart Poster: combines a URL with textual descriptions in various languages (C# only) 
* Text records: contains text in a specific language
* Microsoft LaunchApp: launch a Windows (Phone) app just by tapping a tag (C# only) 
* Android Application Record (AAR): launch an Android app
* Bluetooth Secure Simple Pairing: connect to a Bluetooth device, contains information about the target device like the service class (C# only) 
* Handover Select: part of the Connection Handover specification, provides a list of alternative carriers to connect to. Used for example for NFC loudspeakers. Includes support for child records - Handover Alternative Carrier and Handover Error records (C# only) 

#### New and custom functionality (C#)

* Smart URI class: automatically represents itself as the smallest possible NDEF type (URI or Smart Poster), depending on supplied data 

#### Convenience classes extending the basic URI class for common use case scenarios

* Geo: longitude & latitude of a place, using different Geo URI schemes (more details) 
* Social: linking to social networks like Twitter, Facebook, Foursquare or Skype 
* SMS: defining number and body of the message (C# only) 
* Mailto: sending email messages with recipient address and optional subject and body (C# only) 
* Telephone call: defining the number to call
* Nokia Accessories: let the user choose an app to launch on his Nokia Lumia Windows Phone 8 device (C# only) 
* WpSettings: launch a settings page on Windows Phone 8 (e.g., Bluetooth settings, flight mode). Actually modifying these settings is not allowed by the security model of Windows Phone (C# only) 
  
#### Platform-specific extension library to enable real-life use cases (C#)

* Business card (vCard): convert a Contact from the user's address book directly to a vCard record (Windows 8, Windows Phone 8)
* iCalendar: store appointments and events on tags, integrates with WinRT calendar classes (Windows 8, alpha release)
* Image: images in various format on NFC tags or embedded in a Smart Poster. Includes de/encoding of JPEG, PNG, GIF and other file formats (Windows 8) 



## Example Apps

For C#, the library download comes with NdefDemo and NdefDemoWin: Windows Phone 8 and Windows 8.1 example apps that demonstrate some of the features of the NDEF Library. Both demos are available under GPL v3 license.

Another GPL-licensed example app is [NfcShare](http://www.nfcinteractor.com/developers/presentations/lumia-app-lab-nfc-webinar/), which is available together with accompanying webinar slides and a recording at the [NFC developer's section at NfcInteractor.com](http://www.nfcinteractor.com/developers/presentations/lumia-app-lab-nfc-webinar/).

Examples of apps currently using the NDEF Library and available in the public store:

* [NFC interactor](http://www.nfcinteractor.com/) for Windows Phone: powerful NFC tag reader / writer app
* [NearSpeak](http://www.nearspeak.at/): store voice messages on NFC tags
* [NFCsms](http://www.nfcinteractor.com/related-projects/nfcsms/): enables Windows Phone to send SMS messages from NFC tags 



## Usage example (C#)

### Reading & parsing a Smart Poster

``` 
private void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
{
    // Parse raw byte array to NDEF message
    var rawMsg = message.Data.ToArray();
	var ndefMessage = NdefMessage.FromByteArray(rawMsg);

	// Loop over all records contained in the NDEF message
	foreach (NdefRecord record in ndefMessage) 
	{
		Debug.WriteLine("Record type: " + Encoding.UTF8.GetString(record.Type, 0, record.Type.Length));
		// Go through each record, check if it's a Smart Poster
		if (record.CheckSpecializedType(false) == typeof (NdefSpRecord))
		{
			// Convert and extract Smart Poster info
			var spRecord = new NdefSpRecord(record);
			Debug.WriteLine("URI: " + spRecord.Uri);
			Debug.WriteLine("Titles: " + spRecord.TitleCount());
			Debug.WriteLine("1. Title: " + spRecord.Titles[0].Text);
			Debug.WriteLine("Action set: " + spRecord.ActionInUse());
		}
	}
}
``` 

### Writing a Smart Poster

``` 
// Initialize Smart Poster record with URI, Action + 1 Title
var spRecord = new NdefSpRecord {
                  Uri = "http://www.nfcinteractor.com", 
                  NfcAction = NdefSpActRecord.NfcActionType.DoAction };
spRecord.AddTitle(new NdefTextRecord { 
                  Text = "Nfc Interactor", LanguageCode = "en" });

// Add record to NDEF message
var msg = new NdefMessage { spRecord };

// Publish NDEF message to a tag
// AsBuffer(): add -> using System.Runtime.InteropServices.WindowsRuntime;
_device.PublishBinaryMessage("NDEF:WriteTag", msg.ToByteArray().AsBuffer());

// Alternative: send NDEF message to another NFC device
_device.PublishBinaryMessage("NDEF", msg.ToByteArray().AsBuffer());
``` 



## Usage Example (JavaScript)

### Create a URI Record:

``` 
// Create NDEF Message
var ndefMessage = new NdefLibrary.NdefMessage();
// Create NDEF Uri Record
var ndefUriRecord = new NdefLibrary.NdefUriRecord();
// Set Uri in record
ndefUriRecord.setUri("https://www.mobilefactory.at");
// Add record to message
ndefMessage.push(ndefUriRecord);
// Get byte array for NFC tag
var byteArray = ndefMessage.toByteArray();
``` 


### Create a raw NDEF Message by defined input:

``` 
var recordType = new Array(1,3,1,3,5,6,7);
var recordPayload = new Array(1,2,1);
var id = new Array(3,3);
var ndefRecord2 = new NdefLibrary.NdefRecord(NdefLibrary.NdefRecord.TypeNameFormatType.NfcRtd, recordType);
ndefRecord2.setPayload(recordPayload);
ndefRecord2.setId(id);

var ndefMessage = new NdefLibrary.NdefMessage();
ndefMessage.push(ndefRecord2);
var byteArray = ndefMessage.toByteArray();
``` 


### Create a raw NDEF Message by byte array from NFC tag:

``` 
var ndefMessage = NdefLibrary.NdefMessage.fromByteArray(byteArray);
``` 



## Installation (C#)

To try the library, you can download the complete library package from this site and test the included NdefDemo example app (currently available for Windows Phone 8). Note that the Windows 8.1 version of the NdefDemoWin example app requires the [Microsoft Multilingual App Toolkit](http://msdn.microsoft.com/en-us/windows/apps/bg127574).

If you want to use the Ndef Library from your own app, the easiest option is to use the NuGet package manager in Visual Studio 2012/2013 to automatically download & integrate the portable library:

* Ensure you have Nuget version >= 2.8.1. Update through: Tools -> Extensions and Updates... -> Updates (left sidebar) -> Visual Studio Gallery _(Otherwise, you will get an error message like this during installation: Install failed. Rolling back... Could not install package 'NdefLibrary'. You are trying to install this package into a project that targets 'WindowsPhone,Version=v8.0', but the package does not contain any assembly references that are compatible with that framework. For more information, contact the package author.")_

1. Tools -> Library Package Manager -> Manage NuGet Packages for Solution...
2. Search "Online" for "NDEF"
3. Install: "NFC / NDEF Library for Proximity APIs" 
4. To use the platform extension library, also install: "NFC / NDEF Library Platform Extensions for Proximity APIs"

More instructions: https://github.com/andijakl/ndef-nfc/wiki

#### Core NFC / NDEF Library

* More information: https://nuget.org/packages/NdefLibrary
* Debug symbols: http://www.symbolsource.org/Public/Metadata/NuGet/Project/NdefLibrary

#### NFC / NDEF Library Platform-Specific Extension Library

* More information: https://www.nuget.org/packages/NdefLibraryExtension
* Debug symbols: http://www.symbolsource.org/Public/Metadata/NuGet/Project/NdefLibraryExtension

You can also download the complete portable library project from the source control server of this project, and build the library yourself, or directly integrate the relevant class files.



## Installation (JavaScript)

The JavaScript library is available in two versions, both are available in the "dist" folder of the JavaScript project:

* _ndeflibrary.js:_ complete version of the library, use for debugging & development
* _ndeflibrary.min.js:_ minified version of the library, use for release 



## Version History (C#)

### Latest changes
* Rebased library from Codeplex to Github

### 3.0.2 - July 2014
* New Windows Phone 8.1 settings schemes
* More flexibility when setting properties of the Bluetooth Secure Simple Pairing record from enum values

### 3.0.1-alpha - July 2014
* Compatible to Windows Phone 8.1 WinRT-based Apps and Universal Apps
* New Bluetooth and Connection Handover classes. Enables full support for writing Bluetooth handover NFC tags as found in accessories like loudspeakers and headsets.
	* Bluetooth Secure Simple Pairing record, including extensive ready made type definitions according to Bluetooth Core specification 4.1 from December 2013
	* Connection Handover standard by the NFC Forum, version 1.3 (January 2014)
		* Handover Select record
		* Handover Alternative Carrier record
		* Handover Error record

### 2.0.0.2 - April 2014
* New: NDEF record classes
	* Business card records (vCard): Windows 8 (depends on integrated vCard library) / Windows Phone 8 (no 3rd party dependencies)
	* iCalendar records: Windows 8 (alpha version, depends on 3rd party DDay.iCal library)
	* MIME / Image records: Windows 8
* New: Windows 8.1 Demo app
* New: TagGenerator utility app (console-based) to create files containing NDEF messages
* Changed structure: core NDEF library + platform-specific extension library to enable real-life use cases (e.g., converting a WinRT Contact to a vCard record)
* Added: HERE Maps navigation schemes for Geo records
* Added: Power and screen rotation for WP Settings records
	
### 1.1.0.0 - July 2013
* SMS handling improved: allows wrong sms:// scheme, parses URLs without body text and/or number
* Social record adds Google+ and the FourSquare protocol
* Adds dictionary of available Nokia Accessories including their product names
* NearSpeak record now understands cloud-based tags
* Improved comments for NDEF message, removed debug output



## Version History (JavaScript)

### Known issues and limitations:
* Text record does not support UTF-16 encoding yet.
* Does not identify specialized types for URL records (e.g., Tel record)
* Unit test issue: URL encoding of special characters not equivalent to C# output.
* Unit test issue: UTF-16 text comparison with string.

### 1.0.0 - Work in progress
* Fixes for the compiled JavaScript library (include const to compiled version)
* Use var instead of const for better JavaScript compatibility
* New JavaScript demo app using Apache Cordova for Android, Windows Phone and Windows
* Rebased library from Codeplex to Github

### 0.0.1 - March 2014
* Initial Version



## Status & Roadmap

The NDEF library is classified as stable release and is in use in several projects, most importantly NFC interactor for Windows Phone (http://www.nfcinteractor.com/).

Any open issues as well as planned features are tracked online:
https://github.com/andijakl/ndef-nfc/issues



## Related Information

Released under the LGPL 2.1 license - see the LICENSE.LGPL file for details.

Developed by Andreas Jakl, https://twitter.com/andijakl
Ported to Javascript by Sebastian Höbarth, http://www.mobilefactory.at/

Parts of this library are based on the respective code of the Connectivity Module of Qt Mobility (NdefMessage, NdefRecord, NdefUriRecord and NdefTextRecord classes. Original source code: http://qt.gitorious.org/qt-mobility).

Library homepage on GitHub:
http://andijakl.github.io/ndef-nfc/
https://github.com/andijakl/ndef-nfc/

