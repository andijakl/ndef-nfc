import { NdefRecord, TypeNameFormatType } from './NdefRecord.js';

export class NdefAndroidAppRecord extends NdefRecord {
    static AAR_TYPE = new TextEncoder().encode("android.com:pkg");

    constructor(packageName) {
        super(TypeNameFormatType.ExternalRtd, NdefAndroidAppRecord.AAR_TYPE);
        if (packageName) {
            this.setPackageName(packageName);
        }
    }

    static isRecordType(record) {
        return record.getTypeNameFormat() === TypeNameFormatType.ExternalRtd &&
            record.getType().length === NdefAndroidAppRecord.AAR_TYPE.length &&
            record.getType().every((b, i) => b === NdefAndroidAppRecord.AAR_TYPE[i]);
    }

    getPackageName() {
        const payload = this.getPayload();
        if (!payload || payload.length === 0) {
            return "";
        }
        return new TextDecoder().decode(payload);
    }

    setPackageName(packageName) {
        if (!packageName) {
            this.setPayload(null);
            return;
        }
        this.setPayload(new TextEncoder().encode(packageName));
    }
}
