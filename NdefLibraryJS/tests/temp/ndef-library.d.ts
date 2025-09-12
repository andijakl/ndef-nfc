
declare module 'ndef-library' {
  export class NdefRecord {
    constructor();
    getType(): string;
    setType(type: string): NdefRecord;
    getPayload(): Uint8Array;
    setPayload(payload: Uint8Array): NdefRecord;
  }
  
  export class NdefMessage {
    constructor();
    addRecord(record: NdefRecord): void;
    getRecords(): NdefRecord[];
    toByteArray(): Uint8Array;
  }
}
