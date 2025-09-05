# NDEF Library for JavaScript

This library provides a simple way to create and parse NDEF (NFC Data Exchange Format) messages in JavaScript. It is designed to be used in modern web applications that use the [Web NFC API](https://w3c.github.io/web-nfc/).

## Installation

You can install the library using npm:

```bash
npm install ndef-library
```

Or by linking to it locally if you have the repository cloned:

```json
"dependencies": {
  "ndef-library": "file:../NdefLibraryJS"
}
```

## Usage

The library is distributed as an ES module. You can import the classes you need in your JavaScript files:

```javascript
import { NdefMessage, NdefRecord, NdefUriRecord, NdefTextRecord } from 'ndef-library';
```

### Creating an NDEF Message

To create an NDEF message, you can create an instance of the `NdefMessage` class and pass an array of `NdefRecord` instances to its constructor.

```javascript
const textRecord = new NdefTextRecord("Hello, World!");
const uriRecord = new NdefUriRecord("https://www.example.com");
const message = new NdefMessage([textRecord, uriRecord]);
```

### Writing an NDEF Message

You can use the Web NFC API to write an NDEF message to an NFC tag:

```javascript
const ndef = new NDEFReader();
await ndef.write(message);
```

### Reading an NDEF Message

You can also use the Web NFC API to read an NDEF message from an NFC tag:

```javascript
const ndef = new NDEFReader();
await ndef.scan();
ndef.onreading = event => {
    const message = new NdefMessage(event.message.records.map(r => new NdefRecord(r.recordType, r.mediaType, r.data, r.id)));
    console.log(message.records);
};
```

## API

The library provides the following classes:

*   `NdefMessage`: Represents an NDEF message, which is a collection of NDEF records.
*   `NdefRecord`: The base class for all NDEF records.
*   `NdefUriRecord`: A record that contains a URI.
*   `NdefTextRecord`: A record that contains a text string.
*   `NdefGeoRecord`: A record that contains a geographic coordinate.
*   `NdefSocialRecord`: A record that contains a social network link.
*   `NdefTelRecord`: A record that contains a telephone number.
*   `NdefAndroidAppRecord`: A record that contains an Android Application Record (AAR).

## Demo Application

The `NdefDemoJS` directory contains a demo application that shows how to use the library. To run the demo:

1.  Navigate to the `NdefDemoJS` directory.
2.  Install the dependencies: `npm install`
3.  Start the development server: `npm run dev`
4.  Open your browser and navigate to `http://localhost:5173/src`.

You will need a browser that supports the Web NFC API and an NFC-enabled device to test the demo.
