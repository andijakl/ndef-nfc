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
        return NdefUriRecord.isRecordType(record) &&
            record.getUri().startsWith(TEL_SCHEME);
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
