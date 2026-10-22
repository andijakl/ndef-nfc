import {
    NdefMessage,
    NdefUriRecord,
    NdefTextRecord,
    NdefTelRecord,
    NdefGeoRecord,
    NdefSocialRecord,
    NfcSocialType,
    NdefAndroidAppRecord,
    TextEncodingType,
    TypeNameFormatType,
    toWebNfcMessage,
    fromWebNfcMessage,
} from 'ndef-library';

const $ = id => document.getElementById(id);
const statusList = $('statusList');
const scanBtn = $('scanBtn');
const cancelWriteBtn = $('cancelWriteBtn');
const writeButtons = ['writeUriBtn', 'writeTextBtn', 'writeLinkedInBtn', 'writeTelBtn', 'writeGeoBtn'].map($);

const decoder = new TextDecoder();
let scanController = null;
let writeController = null;

// ---------------------------------------------------------------------------
// Status output

function addStatus(title, details = '') {
    const li = document.createElement('li');
    const time = document.createElement('time');
    time.textContent = new Date().toLocaleTimeString();
    const heading = document.createElement('b');
    heading.textContent = title;
    li.append(time, heading);
    if (details) {
        // textContent: tag contents are untrusted and must never be interpreted as HTML
        li.append(document.createTextNode('\n' + details));
    }
    statusList.prepend(li);
}

function errorText(error) {
    return error?.name ? `${error.name}: ${error.message}` : String(error);
}

// ---------------------------------------------------------------------------
// Reading

async function toggleScan() {
    if (scanController) {
        scanController.abort();
        scanController = null;
        scanBtn.textContent = 'Start scanning';
        addStatus('Scanning stopped');
        return;
    }

    const reader = new NDEFReader();
    scanController = new AbortController();
    reader.onreading = event => handleReading(event);
    reader.onreadingerror = () => addStatus('Tag could not be read', 'The tag may be empty or not NDEF formatted.');
    try {
        await reader.scan({ signal: scanController.signal });
        scanBtn.textContent = 'Stop scanning';
        addStatus('Scanning started', 'Hold an NFC tag to the back of the phone.');
    } catch (error) {
        scanController = null;
        addStatus('Scanning failed', errorText(error));
    }
}

function handleReading(event) {
    try {
        // Convert the Web NFC message to an NDEF message of the library
        const message = fromWebNfcMessage(event.message);
        const lines = [`Serial number: ${event.serialNumber || '(not available)'}`];
        message.records.forEach((record, index) => {
            lines.push('', `Record ${index + 1}:`, ...describeRecord(record).map(line => '  ' + line));
        });
        lines.push('', `Raw NDEF message (${message.toByteArray().length} bytes):`, toHex(message.toByteArray()));
        addStatus(`Tag read: ${message.length} record(s)`, lines.join('\n'));
    } catch (error) {
        addStatus('Parsing the tag failed', errorText(error));
    }
}

function describeRecord(record) {
    if (NdefTelRecord.isRecordType(record)) {
        const uri = asUriRecord(record).getUri();
        return ['Telephone number', uri.substring('tel:'.length)];
    }
    if (NdefUriRecord.isRecordType(record)) {
        return ['URI', asUriRecord(record).getUri()];
    }
    if (NdefTextRecord.isRecordType(record)) {
        const text = new NdefTextRecord();
        text.setPayload(record.getPayload());
        const encoding = text.getTextEncoding() === TextEncodingType.Utf16 ? 'UTF-16' : 'UTF-8';
        return [`Text (language: ${text.getLanguageCode()}, ${encoding})`, text.getText()];
    }
    if (NdefAndroidAppRecord.isRecordType(record)) {
        return ['Android Application Record', decoder.decode(record.getPayload())];
    }

    const tnf = record.getTypeNameFormat();
    const type = decoder.decode(toBytes(record.getType()));
    if (tnf === TypeNameFormatType.NfcRtd && type === 'Sp') {
        const nested = NdefMessage.fromByteArray(record.getPayload());
        return ['Smart Poster', ...nested.records.flatMap(describeRecord).map(line => '  ' + line)];
    }
    if (tnf === TypeNameFormatType.Mime) {
        const payload = toBytes(record.getPayload());
        const content = type.startsWith('text/') || type.endsWith('json') ? decoder.decode(payload) : toHex(payload);
        return [`MIME: ${type}`, content];
    }
    const tnfName = Object.keys(TypeNameFormatType).find(key => TypeNameFormatType[key] === tnf);
    return [`${tnfName} record${type ? ` (type: ${type})` : ''}`, toHex(toBytes(record.getPayload()))];
}

function asUriRecord(record) {
    const uriRecord = new NdefUriRecord();
    uriRecord.setPayload(record.getPayload());
    return uriRecord;
}

// ---------------------------------------------------------------------------
// Writing

async function write(record, description) {
    writeController?.abort();
    writeController = new AbortController();
    const message = new NdefMessage([record]);
    cancelWriteBtn.disabled = false;
    addStatus(`Ready to write ${description}`, `Tap a tag to write ${message.toByteArray().length} bytes.`);
    try {
        // Convert the library message to the format expected by the Web NFC API
        await new NDEFReader().write(toWebNfcMessage(message), { signal: writeController.signal });
        addStatus(`Written: ${description}`);
    } catch (error) {
        addStatus(error.name === 'AbortError' ? 'Write cancelled' : 'Write failed', errorText(error));
    } finally {
        writeController = null;
        cancelWriteBtn.disabled = true;
    }
}

function writeUri() {
    write(new NdefUriRecord($('urlInput').value), 'URI record');
}

function writeText() {
    write(new NdefTextRecord($('textInput').value, navigator.language.split('-')[0] || 'en'), 'text record');
}

function writeLinkedIn() {
    write(new NdefSocialRecord($('linkedinInput').value, NfcSocialType.LinkedIn), 'LinkedIn record');
}

function writeTel() {
    write(new NdefTelRecord($('telInput').value), 'telephone record');
}

function writeGeo() {
    const latitude = parseFloat($('latInput').value);
    const longitude = parseFloat($('lonInput').value);
    if (Number.isNaN(latitude) || Number.isNaN(longitude)) {
        addStatus('Invalid coordinates');
        return;
    }
    write(new NdefGeoRecord(latitude, longitude), 'geo record');
}

// ---------------------------------------------------------------------------
// Sharing

async function shareUri() {
    try {
        await navigator.share({ title: 'NFC / NDEF Demo', url: $('urlInput').value });
        addStatus('Shared URL');
    } catch (error) {
        addStatus('Sharing failed', errorText(error));
    }
}

// ---------------------------------------------------------------------------
// Utilities

function toBytes(value) {
    return value instanceof Uint8Array ? value : Uint8Array.from(value ?? []);
}

function toHex(bytes) {
    return Array.from(bytes, b => b.toString(16).padStart(2, '0')).join(' ');
}

// ---------------------------------------------------------------------------
// Initialization

function init() {
    const supportInfo = $('supportInfo');
    let nfcAvailable = false;
    if (!window.isSecureContext) {
        supportInfo.textContent = 'Web NFC requires a secure context. Open this page via https:// (e.g. "npm run dev:https") or http://localhost.';
    } else if (!('NDEFReader' in window)) {
        supportInfo.textContent = 'Web NFC is not supported by this browser. Use Chrome on Android with NFC enabled.';
    } else {
        nfcAvailable = true;
        supportInfo.textContent = 'Web NFC is supported. Tap "Start scanning" or one of the write buttons.';
    }
    supportInfo.classList.toggle('error', !nfcAvailable);
    scanBtn.disabled = !nfcAvailable;
    writeButtons.forEach(button => (button.disabled = !nfcAvailable));
    $('shareUriBtn').disabled = !('share' in navigator);

    scanBtn.addEventListener('click', toggleScan);
    $('writeUriBtn').addEventListener('click', writeUri);
    $('writeTextBtn').addEventListener('click', writeText);
    $('writeLinkedInBtn').addEventListener('click', writeLinkedIn);
    $('writeTelBtn').addEventListener('click', writeTel);
    $('writeGeoBtn').addEventListener('click', writeGeo);
    cancelWriteBtn.addEventListener('click', () => writeController?.abort());
    $('shareUriBtn').addEventListener('click', shareUri);
}

init();
