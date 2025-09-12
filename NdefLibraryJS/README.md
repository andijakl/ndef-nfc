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
*   `NdefSocialRecord`: A record that contains a social network link. Supports X (formerly Twitter), LinkedIn, Facebook, Instagram, Threads, and TikTok.
*   `NdefTelRecord`: A record that contains a telephone number.
*   `NdefAndroidAppRecord`: A record that contains an Android Application Record (AAR).

## Demo Application

The `NdefDemoJS` directory contains a demo application that shows how to use the library with the Web NFC API.

### Browser Compatibility

**Important**: The Web NFC API is currently only supported in **Chrome for Android** (version 89+). This means the demo application will only work on Android devices using the Chrome browser. Desktop browsers (including Chrome on Windows/Mac/Linux) and other mobile browsers (Safari, Firefox, Samsung Internet, etc.) do not support the Web NFC API.

### Running the Demo on Chrome for Android

To run the demo on your Android phone:

1. **Set up the development environment:**
   * Navigate to the `NdefDemoJS` directory on your development machine.
   * Install the dependencies: `npm install`
   * Start the development server: `npm run dev`
   * The server will start on port 5173 and be accessible from other devices on your network.

2. **Access the demo from your Android device:**
   * Make sure your Android device and development machine are on the same network.
   * Find your development machine's IP address:
     * On Windows: Open Command Prompt and run `ipconfig` (look for IPv4 Address)
     * On Mac/Linux: Run `ifconfig` or `ip addr show`
   * On your Android device, open **Chrome** browser.
   * Navigate to `http://[YOUR_IP_ADDRESS]:5173/src` (replace `[YOUR_IP_ADDRESS]` with your actual IP).
   * For example: `http://192.168.1.100:5173/src`

   **Troubleshooting network access:**
   * If you can't access the server from other devices, check your firewall settings.
   * On Windows, you may need to allow the Node.js application through Windows Defender Firewall.
   * Ensure your network allows communication between devices (some public networks block this).

3. **Enable NFC on your Android device:**
   * Go to **Settings > Connected devices > Connection preferences > NFC** (path may vary by device).
   * Turn on **NFC**.

4. **Test the demo:**
   * The demo will show whether Web NFC is supported.
   * Use the "Subscribe for NDEF" button to start listening for NFC tags.
   * Use the "Publish" buttons to write data to NFC tags.
   * You'll need physical NFC tags to test the read/write functionality.

### Alternative: Build and Deploy

For a production setup, you can build the demo and deploy it to a web server:

1. Build the demo: `npm run build`
2. Deploy the `dist` folder to any web server accessible from your Android device.
3. Access the deployed URL using Chrome on your Android device.

**Note**: HTTPS is required for the Web NFC API to work in production environments.
