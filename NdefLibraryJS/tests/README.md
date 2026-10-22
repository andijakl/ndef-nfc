# NDEF Library Tests

The tests use [Vitest](https://vitest.dev/) and run directly on the ES module sources (no build step needed).

```bash
npm test               # run all tests once
npm run test:watch     # re-run on file changes
```

## Structure

- `unit-tests/`: tests for the individual record classes, `NdefMessage` parsing / serialization and the Web NFC conversion (`NdefWebNfc.test.js`)
- `integration-tests/`: round trips of complete messages, multi-record messages and malformed data handling

The Web NFC tests simulate the `NDEFRecord` objects that Chrome on Android passes to the `reading` event (including `DataView` payloads), so they run without NFC hardware.
