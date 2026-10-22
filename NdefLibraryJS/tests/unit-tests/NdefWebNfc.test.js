import {
    NdefMessage,
    NdefRecord,
    TypeNameFormatType,
    NdefUriRecord,
    NdefTextRecord,
    TextEncodingType,
    NdefSocialRecord,
    NfcSocialType,
    NdefAndroidAppRecord,
    NdefTelRecord,
    toWebNfcMessage,
    fromWebNfcMessage,
} from '../../src/index.js';

const encoder = new TextEncoder();
const decoder = new TextDecoder();

/** Creates a DataView like the Web NFC API delivers it (a view into a larger buffer). */
function dataView(bytes) {
    const buffer = new Uint8Array(bytes.length + 8);
    buffer.set(bytes, 4);
    return new DataView(buffer.buffer, 4, bytes.length);
}

/** Simulates an NDEFRecord as created by the browser from an NDEFRecordInit. */
function asReadRecord(init) {
    let data = null;
    if (typeof init.data === 'string') {
        data = dataView(encoder.encode(init.data));
    } else if (init.data instanceof Uint8Array) {
        data = dataView(init.data);
    } else if (init.data && init.data.records) {
        // Nested message (smart poster): the browser serializes it to raw NDEF bytes.
        // Re-create it via the library to get the raw payload.
        data = dataView(fromWebNfcMessage({ records: init.data.records.map(asReadRecord) }).toByteArray());
    }
    return {
        recordType: init.recordType,
        mediaType: init.mediaType ?? null,
        id: init.id ?? null,
        encoding: init.encoding ?? null,
        lang: init.lang ?? null,
        data,
    };
}

function roundTrip(message) {
    const init = toWebNfcMessage(message);
    return fromWebNfcMessage({ records: init.records.map(asReadRecord) });
}

describe('Web NFC conversion', () => {
    describe('toWebNfcMessage', () => {
        test('converts a URI record to a Web NFC url record', () => {
            const message = new NdefMessage([new NdefUriRecord('https://www.nfcinteractor.com')]);

            expect(toWebNfcMessage(message)).toEqual({
                records: [{ recordType: 'url', data: 'https://www.nfcinteractor.com' }],
            });
        });

        test('converts specialized URI records (social) to url records', () => {
            const init = toWebNfcMessage([new NdefSocialRecord('andreasjakl', NfcSocialType.LinkedIn)]);

            expect(init.records[0]).toEqual({ recordType: 'url', data: 'https://linkedin.com/in/andreasjakl' });
        });

        test('converts a UTF-8 text record with language', () => {
            const init = toWebNfcMessage([new NdefTextRecord('Hallo', 'de')]);
            const record = init.records[0];

            expect(record.recordType).toBe('text');
            expect(record.lang).toBe('de');
            expect(record.encoding).toBe('utf-8');
            expect(decoder.decode(record.data)).toBe('Hallo');
        });

        test('converts a UTF-16 text record with the raw UTF-16BE bytes', () => {
            const init = toWebNfcMessage([new NdefTextRecord('Hi', 'en', TextEncodingType.Utf16)]);
            const record = init.records[0];

            expect(record.encoding).toBe('utf-16be');
            expect(Array.from(record.data)).toEqual([0x00, 0x48, 0x00, 0x69]);
        });

        test('converts an Android Application Record to an external type', () => {
            const init = toWebNfcMessage([new NdefAndroidAppRecord('com.example.nfc')]);

            expect(init.records[0].recordType).toBe('android.com:pkg');
            expect(decoder.decode(init.records[0].data)).toBe('com.example.nfc');
        });

        test('converts MIME, absolute URL, unknown and empty records', () => {
            const init = toWebNfcMessage([
                new NdefRecord(TypeNameFormatType.Mime, encoder.encode('text/plain'), encoder.encode('abc')),
                new NdefRecord(TypeNameFormatType.Uri, encoder.encode('https://example.com/abs')),
                new NdefRecord(TypeNameFormatType.Unknown, null, new Uint8Array([1, 2, 3])),
                new NdefRecord(TypeNameFormatType.Empty),
            ]);

            expect(init.records[0]).toMatchObject({ recordType: 'mime', mediaType: 'text/plain' });
            expect(init.records[1]).toEqual({ recordType: 'absolute-url', data: 'https://example.com/abs' });
            expect(init.records[2].recordType).toBe('unknown');
            expect(Array.from(init.records[2].data)).toEqual([1, 2, 3]);
            expect(init.records[3]).toEqual({ recordType: 'empty' });
        });

        test('converts a smart poster to nested records with local types', () => {
            const nested = new NdefMessage([
                new NdefUriRecord('https://andijakl.github.io/ndef-nfc/'),
                new NdefTextRecord('NDEF Library', 'en'),
                // Action record ("act"): 0 = do the action
                new NdefRecord(TypeNameFormatType.NfcRtd, encoder.encode('act'), new Uint8Array([0])),
            ]);
            const smartPoster = new NdefRecord(TypeNameFormatType.NfcRtd, encoder.encode('Sp'), nested.toByteArray());

            const init = toWebNfcMessage([smartPoster]);
            const sp = init.records[0];

            expect(sp.recordType).toBe('smart-poster');
            expect(sp.data.records.map(r => r.recordType)).toEqual(['url', 'text', ':act']);
            expect(sp.data.records[0].data).toBe('https://andijakl.github.io/ndef-nfc/');
        });

        test('passes on the record id', () => {
            const record = new NdefUriRecord('https://example.com');
            record.setId(encoder.encode('record-1'));

            expect(toWebNfcMessage([record]).records[0].id).toBe('record-1');
        });

        test('throws for well-known types that Web NFC cannot write', () => {
            const handoverSelect = new NdefRecord(TypeNameFormatType.NfcRtd, encoder.encode('Hs'), new Uint8Array([0x12]));

            expect(() => toWebNfcMessage([handoverSelect])).toThrow(/can't be written/);
        });
    });

    describe('fromWebNfcMessage', () => {
        test('parses url and text records from DataViews into specialized records', () => {
            const message = fromWebNfcMessage({
                records: [
                    { recordType: 'url', data: dataView(encoder.encode('https://www.nfcinteractor.com')) },
                    { recordType: 'text', lang: 'de-AT', encoding: 'utf-8', data: dataView(encoder.encode('Grüß Gott')) },
                ],
            });

            expect(message.length).toBe(2);
            expect(message.records[0]).toBeInstanceOf(NdefUriRecord);
            expect(message.records[0].getUri()).toBe('https://www.nfcinteractor.com');
            expect(message.records[1]).toBeInstanceOf(NdefTextRecord);
            expect(message.records[1].getLanguageCode()).toBe('de-AT');
            expect(message.records[1].getText()).toBe('Grüß Gott');
        });

        test('parses external types and keeps the raw type', () => {
            const message = fromWebNfcMessage({
                records: [{ recordType: 'android.com:pkg', data: dataView(encoder.encode('com.example.nfc')) }],
            });
            const record = message.records[0];

            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.ExternalRtd);
            expect(NdefAndroidAppRecord.isRecordType(record)).toBe(true);
        });

        test('allows detecting specialized types on generic parsed records', () => {
            const raw = new NdefMessage([new NdefTelRecord('+431234'), new NdefUriRecord('https://example.com')]).toByteArray();
            const [tel, uri] = NdefMessage.fromByteArray(raw).records;

            expect(NdefTelRecord.isRecordType(tel)).toBe(true);
            expect(NdefTelRecord.isRecordType(uri)).toBe(false);
        });

        test('parses records without data (empty records)', () => {
            const message = fromWebNfcMessage({ records: [{ recordType: 'empty', data: null }] });

            expect(message.records[0].getTypeNameFormat()).toBe(TypeNameFormatType.Empty);
        });
    });

    describe('round trip', () => {
        test('produces identical NDEF bytes for typical tag contents', () => {
            const original = new NdefMessage([
                new NdefUriRecord('https://www.nfcinteractor.com'),
                new NdefTextRecord('Hello 👋', 'en'),
                new NdefTextRecord('UTF-16 text', 'en', TextEncodingType.Utf16),
                new NdefAndroidAppRecord('com.example.nfc'),
                new NdefRecord(TypeNameFormatType.Mime, encoder.encode('application/json'), encoder.encode('{"a":1}')),
            ]);

            expect(Array.from(roundTrip(original).toByteArray())).toEqual(Array.from(original.toByteArray()));
        });

        test('keeps smart poster contents', () => {
            const nested = new NdefMessage([
                new NdefUriRecord('https://andijakl.github.io/ndef-nfc/'),
                new NdefTextRecord('NDEF Library', 'en'),
            ]);
            const original = new NdefMessage([
                new NdefRecord(TypeNameFormatType.NfcRtd, encoder.encode('Sp'), nested.toByteArray()),
            ]);

            const parsed = roundTrip(original);
            const parsedNested = NdefMessage.fromByteArray(parsed.records[0].getPayload());

            expect(Array.from(parsed.toByteArray())).toEqual(Array.from(original.toByteArray()));
            expect(NdefTextRecord.isRecordType(parsedNested.records[1])).toBe(true);
            expect(decoder.decode(parsedNested.records[1].getPayload().slice(3))).toBe('NDEF Library');
        });
    });
});

describe('NdefMessage.fromByteArray input types', () => {
    const bytes = new NdefMessage([new NdefUriRecord('https://example.com')]).toByteArray();

    test.each([
        ['Uint8Array', bytes],
        ['ArrayBuffer', bytes.slice().buffer],
        ['DataView', new DataView(bytes.slice().buffer)],
        ['number[]', Array.from(bytes)],
    ])('accepts %s', (_, input) => {
        const message = NdefMessage.fromByteArray(input);
        const uri = new NdefUriRecord();
        uri.setPayload(message.records[0].getPayload());

        expect(NdefUriRecord.isRecordType(message.records[0])).toBe(true);
        expect(uri.getUri()).toBe('https://example.com');
    });

    test('parses long records from a view into a larger buffer', () => {
        const longText = 'x'.repeat(300);
        const raw = new NdefMessage([new NdefTextRecord(longText, 'en')]).toByteArray();
        const buffer = new Uint8Array(raw.length + 10);
        buffer.set(raw, 10);

        const message = NdefMessage.fromByteArray(buffer.subarray(10));
        const text = new NdefTextRecord();
        text.setPayload(message.records[0].getPayload());

        expect(text.getText()).toBe(longText);
    });
});
