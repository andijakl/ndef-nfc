import { NdefRecord, TypeNameFormatType } from './NdefRecord.js';

const URI_ABBREVIATIONS = [
    "", "http://www.", "https://www.", "http://", "https://", "tel:", "mailto:",
    "ftp://anonymous:anonymous@", "ftp://ftp.", "ftps://", "sftp://", "smb://",
    "nfs://", "ftp://", "dav://", "news:", "telnet://", "imap:", "rtsp://",
    "urn:", "pop:", "sip:", "sips:", "tftp:", "btspp://", "btl2cap://",
    "btgoep://", "tcpobex://", "irdaobex://", "file://", "urn:epc:id:",
    "urn:epc:tag:", "urn:epc:pat:", "urn:epc:raw:", "urn:epc:", "urn:nfc:"
];

export class NdefUriRecord extends NdefRecord {
    static URI_TYPE = new TextEncoder().encode("U");

    constructor(uri) {
        super(TypeNameFormatType.NfcRtd, NdefUriRecord.URI_TYPE);
        if (uri) {
            this.setUri(uri);
        }
    }

    static isRecordType(record) {
        return record.getTypeNameFormat() === TypeNameFormatType.NfcRtd &&
            record.getType().length === NdefUriRecord.URI_TYPE.length &&
            record.getType().every((b, i) => b === NdefUriRecord.URI_TYPE[i]);
    }

    getRawUri() {
        const payload = this.getPayload();
        if (!payload || payload.length === 0) {
            return null;
        }
        const code = payload[0];
        if (code !== 0) {
            console.log("Raw URI can only be extracted from non-abbreviated URIs");
        }
        return payload.slice(1);
    }

    setRawUri(value) {
        const payload = new Uint8Array(value.length + 1);
        payload[0] = 0;
        payload.set(value, 1);
        this.setPayload(payload);
    }

    getUri() {
        const payload = this.getPayload();
        if (!payload || payload.length === 0) {
            return "";
        }
        let code = payload[0];
        if (code >= URI_ABBREVIATIONS.length) {
            code = 0;
        }
        const uri = new TextDecoder().decode(payload.slice(1));
        return URI_ABBREVIATIONS[code] + uri;
    }

    setUri(uri) {
        let useAbbreviation = 0;
        for (let i = 1; i < URI_ABBREVIATIONS.length; i++) {
            if (uri.startsWith(URI_ABBREVIATIONS[i])) {
                useAbbreviation = i;
                break;
            }
        }

        const plainUri = uri.substring(URI_ABBREVIATIONS[useAbbreviation].length);
        const encodedUri = new TextEncoder().encode(plainUri);
        const payload = new Uint8Array(encodedUri.length + 1);
        payload[0] = useAbbreviation;
        payload.set(encodedUri, 1);
        this.setPayload(payload);
    }
}
