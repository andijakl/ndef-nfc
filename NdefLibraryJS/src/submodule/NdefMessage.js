import { NdefRecord } from './NdefRecord.js';

export class NdefMessage {
    constructor(records) {
        this._records = records || [];
    }

    get records() {
        return this._records;
    }

    push(record) {
        if (record) {
            this._records.push(record);
        }
    }

    clear() {
        this._records = [];
    }

    get length() {
        return this._records.length;
    }

    /**
     * Parses a raw NDEF message.
     * @param {Uint8Array | ArrayBuffer | DataView | number[]} input Raw bytes of the NDEF message.
     * @returns {NdefMessage}
     */
    static fromByteArray(input) {
        const bytes = toUint8Array(input);
        const message = new NdefMessage();
        const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
        let offset = 0;
        while (offset < bytes.length) {
            // Record layout: flags | type length | payload length (1 or 4 bytes) | [id length] | type | [id] | payload
            const flags = bytes[offset++];
            const tnf = flags & 0x07;
            const sr = (flags & 0x10) !== 0;
            const il = (flags & 0x08) !== 0;

            const headerLength = 1 + (sr ? 1 : 4) + (il ? 1 : 0);
            if (offset + headerLength > bytes.length) {
                throw new Error("Invalid NDEF message: record header exceeds message length");
            }
            const typeLength = bytes[offset++];
            let payloadLength;
            if (sr) {
                payloadLength = bytes[offset++];
            } else {
                payloadLength = view.getUint32(offset, false);
                offset += 4;
            }
            const idLength = il ? bytes[offset++] : 0;

            if (offset + typeLength + idLength + payloadLength > bytes.length) {
                throw new Error("Invalid NDEF message: record exceeds message length");
            }
            const type = bytes.slice(offset, offset + typeLength);
            offset += typeLength;
            const id = bytes.slice(offset, offset + idLength);
            offset += idLength;
            const payload = bytes.slice(offset, offset + payloadLength);
            offset += payloadLength;

            message.push(new NdefRecord(tnf, type, payload, id));

            if ((flags & 0x40) !== 0) { // ME flag
                break;
            }
        }
        return message;
    }

    toByteArray() {
        const count = this._records.length;
        if (count === 0) {
            return new NdefMessage([new NdefRecord()]).toByteArray();
        }

        let buffer = new Uint8Array();
        for (let i = 0; i < count; i++) {
            const record = this._records[i];
            let flags = record.getTypeNameFormat();

            if (i === 0) flags |= 0x80; // MB
            if (i === count - 1) flags |= 0x40; // ME

            const sr = record.getPayload().length < 256;
            if (sr) flags |= 0x10;

            const il = record.getId().length > 0;
            if (il) flags |= 0x08;

            const typeLength = record.getType().length;
            const payloadLength = record.getPayload().length;
            const idLength = record.getId().length;

            const header = new Uint8Array(2 + (sr ? 1 : 4) + (il ? 1 : 0) + typeLength + idLength);
            const dataView = new DataView(header.buffer);
            let offset = 0;

            dataView.setUint8(offset++, flags);
            dataView.setUint8(offset++, typeLength);

            if (sr) {
                dataView.setUint8(offset++, payloadLength);
            } else {
                dataView.setUint32(offset, payloadLength, false);
                offset += 4;
            }

            if (il) {
                dataView.setUint8(offset++, idLength);
            }

            header.set(record.getType(), offset);
            offset += typeLength;

            if (il) {
                header.set(record.getId(), offset);
            }

            const newBuffer = new Uint8Array(buffer.length + header.length + payloadLength);
            newBuffer.set(buffer);
            newBuffer.set(header, buffer.length);
            newBuffer.set(record.getPayload(), buffer.length + header.length);
            buffer = newBuffer;
        }

        return buffer;
    }
}

/**
 * @param {Uint8Array | ArrayBuffer | DataView | number[]} input
 * @returns {Uint8Array}
 */
function toUint8Array(input) {
    if (input instanceof Uint8Array) {
        return input;
    }
    if (ArrayBuffer.isView(input)) {
        return new Uint8Array(input.buffer, input.byteOffset, input.byteLength);
    }
    if (input instanceof ArrayBuffer) {
        return new Uint8Array(input);
    }
    return Uint8Array.from(input);
}
