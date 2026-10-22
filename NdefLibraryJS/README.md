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

### Creating Social Media Records

You can create social media records for various platforms:

```javascript
import { NdefSocialRecord, NfcSocialType } from 'ndef-library';

// Create records for different social platforms
const xRecord = new NdefSocialRecord("username", NfcSocialType.X);          // X (formerly Twitter)
const linkedinRecord = new NdefSocialRecord("username", NfcSocialType.LinkedIn);
const instagramRecord = new NdefSocialRecord("username", NfcSocialType.Instagram);
const threadsRecord = new NdefSocialRecord("username", NfcSocialType.Threads);
const tiktokRecord = new NdefSocialRecord("username", NfcSocialType.TikTok);

const socialMessage = new NdefMessage([xRecord, linkedinRecord]);
```

### Writing an NDEF Message with Web NFC

Web NFC doesn't accept raw NDEF bytes. `toWebNfcMessage()` converts a message of this library to the format expected by `NDEFReader.write()`:

```javascript
import { NdefMessage, NdefUriRecord, toWebNfcMessage } from 'ndef-library';

const message = new NdefMessage([new NdefUriRecord("https://www.nfcinteractor.com")]);
await new NDEFReader().write(toWebNfcMessage(message));
```

### Reading an NDEF Message with Web NFC

`fromWebNfcMessage()` converts the message of the `reading` event into an `NdefMessage`. URI and text records are returned as `NdefUriRecord` / `NdefTextRecord`; all other records as `NdefRecord` with the original type name format, type and payload:

```javascript
import { fromWebNfcMessage, NdefUriRecord, NdefTextRecord } from 'ndef-library';

const reader = new NDEFReader();
reader.onreading = event => {
    const message = fromWebNfcMessage(event.message);
    for (const record of message.records) {
        if (record instanceof NdefUriRecord) console.log("URI:", record.getUri());
        if (record instanceof NdefTextRecord) console.log("Text:", record.getText());
    }
};
await reader.scan();
```

### Raw NDEF bytes

Independent of Web NFC, messages can be serialized to and parsed from raw bytes (e.g., for other NFC APIs or to store tag contents in files):

```javascript
const bytes = message.toByteArray();              // Uint8Array
const parsed = NdefMessage.fromByteArray(bytes);  // also accepts ArrayBuffer, DataView and number[]
```

## API

The library provides the following classes and functions:

*   `NdefMessage`: Represents an NDEF message, which is a collection of NDEF records.
*   `NdefRecord`: The base class for all NDEF records.
*   `NdefUriRecord`: A record that contains a URI.
*   `NdefTextRecord`: A record that contains a text string (UTF-8 or UTF-16).
*   `NdefGeoRecord`: A record that contains a geographic coordinate.
*   `NdefSocialRecord`: A record that contains a social network link. Supports X (formerly Twitter), LinkedIn, Facebook, Instagram, Threads, and TikTok.
*   `NdefTelRecord`: A record that contains a telephone number.
*   `NdefAndroidAppRecord`: A record that contains an Android Application Record (AAR).
*   `toWebNfcMessage()` / `fromWebNfcMessage()`: conversion to and from the Web NFC API.

TypeScript declarations are included in the npm package.

## Development

```bash
npm install
npm test          # Vitest unit and integration tests
npm run lint      # ESLint
npm run build     # TypeScript declarations (./types) and ES module bundles (./dist)
```

`dist/ndef-library.js` and `dist/ndef-library.min.js` are single-file ES modules for use from a CDN or a plain `<script type="module">`.

## Demo Application

The `NdefDemoJS` directory contains a demo application that shows how to use the library with the Web NFC API: reading tags (including smart posters) and writing URI, text, social, telephone and geo records.

### Browser Compatibility

**Important**: The Web NFC API is currently only supported in **Chrome for Android**. Desktop browsers (including Chrome on Windows/Mac/Linux) and other mobile browsers do not support the Web NFC API.

Web NFC also requires a **secure context**: the page must be served via `https://` or from `http://localhost`. Plain `http://192.168.x.x` addresses do not work.

### Running the Demo on Chrome for Android

1. On your development machine, in the `NdefDemoJS` directory:
   ```bash
   npm install
   npm run dev:https
   ```
   This starts the Vite dev server with a self-signed certificate on port 5173 and prints the network addresses.
2. Make sure the phone is in the same network, then open `https://[YOUR_IP_ADDRESS]:5173/` in Chrome on Android. Accept the certificate warning (self-signed certificate).
3. Enable NFC on the phone (Settings > Connected devices > Connection preferences > NFC; the path varies by device).
4. Tap "Start scanning" and hold a tag to the back of the phone, or use one of the write buttons.

Alternatives if the phone can't reach your computer (e.g., firewall or client isolation in the network):

* **USB debugging**: connect the phone, run `npm run dev` and forward the port with `adb reverse tcp:5173 tcp:5173`. Then open `http://localhost:5173/` on the phone, which counts as a secure context.
* **Static hosting**: `npm run build` and deploy the `dist` folder to any HTTPS web server (e.g., GitHub Pages). All asset paths are relative, so it can live in a sub folder.
