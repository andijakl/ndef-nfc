import { NdefRecord, TypeNameFormatType } from './NdefRecord.js';

export const TextEncodingType = {
    Utf8: 0,
    Utf16: 1
};

export class NdefTextRecord extends NdefRecord {
    static TEXT_TYPE = new TextEncoder().encode("T");

    constructor(text, languageCode = "en", encoding = TextEncodingType.Utf8) {
        super(TypeNameFormatType.NfcRtd, NdefTextRecord.TEXT_TYPE);
        if (text != null) {
            this.setText(text, languageCode, encoding);
        }
    }

    static isRecordType(record) {
        return record.getTypeNameFormat() === TypeNameFormatType.NfcRtd &&
            record.getType().length === NdefTextRecord.TEXT_TYPE.length &&
            record.getType().every((b, i) => b === NdefTextRecord.TEXT_TYPE[i]);
    }

    getLanguageCode() {
        const payload = this.getPayload();
        if (!payload || payload.length === 0) {
            return "en";
        }
        const status = payload[0];
        const codeLength = (status & 0x3f);
        return new TextDecoder().decode(payload.slice(1, 1 + codeLength));
    }

    getText() {
        const payload = this.getPayload();
        if (!payload || payload.length === 0) {
            return "";
        }
        const status = payload[0];
        const codeLength = (status & 0x3f);
        const encoding = (status & 0x80) !== 0 ? 'utf-16be' : 'utf-8';
        return new TextDecoder(encoding).decode(payload.slice(1 + codeLength));
    }

    getTextEncoding() {
        const payload = this.getPayload();
        if (!payload || payload.length === 0) {
            return TextEncodingType.Utf8;
        }
        return (payload[0] & 0x80) !== 0 ? TextEncodingType.Utf16 : TextEncodingType.Utf8;
    }

    setText(text, languageCode = "en", encoding = TextEncodingType.Utf8) {
        const encodedLanguage = new TextEncoder().encode(languageCode);
        // TextEncoder only supports UTF-8
        const encodedText = encoding === TextEncodingType.Utf16 ? encodeUtf16BE(text) : new TextEncoder().encode(text);

        const payloadLength = 1 + encodedLanguage.length + encodedText.length;
        const payload = new Uint8Array(payloadLength);

        let status = encodedLanguage.length;
        if (encoding === TextEncodingType.Utf16) {
            status |= 0x80;
        }
        payload[0] = status;

        payload.set(encodedLanguage, 1);
        payload.set(encodedText, 1 + encodedLanguage.length);

        this.setPayload(payload);
    }
}

/**
 * Encodes a string as UTF-16 big endian (the byte order mandated by the NFC Forum Text RTD).
 * @param {string} text
 * @returns {Uint8Array}
 */
function encodeUtf16BE(text) {
    const bytes = new Uint8Array(text.length * 2);
    for (let i = 0; i < text.length; i++) {
        const codeUnit = text.charCodeAt(i);
        bytes[i * 2] = codeUnit >> 8;
        bytes[i * 2 + 1] = codeUnit & 0xff;
    }
    return bytes;
}
