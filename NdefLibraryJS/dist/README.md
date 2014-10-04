# NDEF Library for Proximity APIs (NFC)
https://github.com/mopius/ndef-nfc

Easily parse and create NDEF records with JavaScript.


# Background - NFC and NDEF

NFC tags as well as the content sent in device-to-device communication when tapping two phones is based on certain standards by the NFC Forum (called NDEF – NFC Data Exchange format). Luckily, these standards were well received and nearly all manufacturers are part of the standardization body. This ensures that public NFC tags can actually be read by all mobile phones today.

When it comes to storing data on NFC tags that can have as little writable storage as around 40 bytes, very efficient and complex data storage schemes are necessary. The downside is that most operating systems do integrate the NFC data transmission at the base level, but offer developers very little support for the NDEF standards on top. Obviously, reading those technical documents isn’t generally too much fun – to create an own implementation of a message that stores a simple URL on a tag, a developer would need to read and understand 59 pages of specifications.

As this is a lot work, there is the risk of people creating own solutions, leading to a fragmented NFC ecosystem


# The NFC Library

The open source NFC / NDEF Library contains a large set of classes that take care of formatting your data according to NDEF standards, so that these can be directly written to NFC tags or sent to other devices.

In your app, you choose the corresponding record type (e.g., for URLs, emails or geo tags) and provide the necessary data. The library creates an NDEF message out of the data, which you can directly send to the NFC stack in your operating system as a byte array, which takes care of writing it to a tag or publishing it to another device (using the SNEP protocol).

Additionally, the library can parse NDEF byte arrays that you read from tags or receive from other devices and create a list (NDEF Message) of data classes (NDEF records) that you can easily analyze and use in your app.

For Windows (Phone) 8, the NFC stack is represented through the Proximity APIs - they encapsulate NFC hardware communication and basic NDEF formatting for a very limited subset of the NDEF standards. This missing part is added by this NDEF library.


# Reusable NDEF classes

* Parse NDEF message & records from raw byte arrays 
* Extract all information from the bits & bytes contained in the record 
* Create standard compliant records just by providing your data 
* Identify the exact record type when reading an NDEF message
* Records check their contents for validity according to standards
* Can throw NdefException in case of content validity issues, with translatable messages defined in a resource file
* Fully documented source code, following Doxygen standards

* Supported NDEF records:
	* URI: the most common type: any kind of URI, for example an Internet address, email link or any custom URI scheme.
	* Smart Poster: combines a URL with textual descriptions in various languages
	* Text records: contains text in a specific language
	* Microsoft LaunchApp: launch a Windows (Phone) app just by tapping a tag
	* Android Application Record (AAR): launch an Android app

* Convenience classes extending the basic URI class for common use case scenarios:
	* Geo: longitude & latitude of a place, using different Geo URI schemes (more details) 
	* Social: linking to social networks like Twitter, Facebook, Foursquare or Skype 
	* Telephone call: defining the number to call


# Usage example

## Create a URI Record:

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


## Create a raw NDEF Message by defined input:

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


## Create a raw NDEF Message by byte array from NFC tag:

``` 
var ndefMessage = NdefLibrary.NdefMessage.fromByteArray(byteArray);
``` 


# Building the library on Windows

1. Install node.js from: http://nodejs.org/
2. Start "node.js command prompt" as administrator
3. Install grunt CLI: > npm install -g grunt-cli
4. Go to project root directory and download & install project dependencies: > npm install
5. Run grunt build: > grunt build


# Status & Roadmap

Any open issues as well as planned features are tracked online:
https://ndef.codeplex.com/workitem/list/basic

Known issues and limitations:
* Text record does not support UTF-16 encoding yet.
* Does not identify specialized types for URL records (e.g., Tel record)
* Unit test issue: URL encoding of special characters not equivalent to C# output.
* Unit test issue: UTF-16 text comparison with string.

# Version History

## 0.0.2
* Fixes for the compiled JavaScript library (include const to compiled version)
* Use var instead of const for better JavaScript compatibility
* New JavaScript demo app using Apache Cordova for Android, Windows Phone and Windows
* Rebased library from Codeplex to Github

## 0.0.1 - March 2014
* Initial Version


# Related Information

Released under the LGPL license - see the LICENSE.LGPL file for details.

Ported to Javascript by Sebastian Höbarth, http://www.mobilefactory.at/
Developed by Andreas Jakl, https://twitter.com/andijakl

Parts of this library are based on the respective code of the Connectivity Module of Qt Mobility (NdefMessage, NdefRecord, NdefUriRecord and NdefTextRecord classes. Original source code: http://qt.gitorious.org/qt-mobility).

More information about the library:
http://ndef.mopius.com/

Library homepage:
https://github.com/mopius/ndef-nfc/


