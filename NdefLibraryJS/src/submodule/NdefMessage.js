import { NdefRecord, TypeNameFormatType } from './NdefRecord.js';

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

    static fromByteArray(bytes) {
        const message = new NdefMessage();
        let i = 0;
        while (i < bytes.length) {
            const flags = bytes[i];
            const tnf = flags & 0x07;
            const sr = (flags & 0x10) !== 0;
            const il = (flags & 0x08) !== 0;

            const typeLength = bytes[++i];
            const payloadLength = sr ? bytes[++i] : new DataView(bytes.buffer, ++i, 4).getUint32(0, false);
            i += sr ? 0 : 3;
            const idLength = il ? bytes[++i] : 0;

            const type = bytes.slice(++i, i + typeLength);
            i += typeLength -1;
            const id = il ? bytes.slice(++i, i + idLength) : new Uint8Array(0);
            i += il ? idLength - 1 : -1;
            const payload = bytes.slice(++i, i + payloadLength);
            i += payloadLength -1;

            const record = new NdefRecord(tnf, type, payload, id);
            message.push(record);

            if ((flags & 0x40) !== 0) { // ME flag
                break;
            }
            i++;
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
                offset += idLength;
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
