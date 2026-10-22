import { NdefUriRecord } from './NdefUriRecord.js';

const TEL_SCHEME = "tel:";

export class NdefTelRecord extends NdefUriRecord {
    constructor(telNumber) {
        super();
        if (telNumber) {
            this.setTelNumber(telNumber);
        }
    }

    static isRecordType(record) {
        if (!NdefUriRecord.isRecordType(record)) {
            return false;
        }
        // The record may be a generic NdefRecord (e.g., parsed from raw bytes)
        const uriRecord = new NdefUriRecord();
        uriRecord.setPayload(record.getPayload());
        return uriRecord.getUri().startsWith(TEL_SCHEME);
    }

    checkIfValid() {
        super.checkIfValid();
        if (!this.telNumber) {
            throw new Error("Telephone number is empty");
        }
        return true;
    }

    getTelNumber() {
        return this.telNumber;
    }

    setTelNumber(value) {
        this.telNumber = value;
        this.updatePayload();
    }

    updatePayload() {
        if (this.telNumber) {
            this.setUri(TEL_SCHEME + this.telNumber);
        }
    }
}
