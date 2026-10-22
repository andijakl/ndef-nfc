import { NdefMessage } from '../../src/submodule/NdefMessage.js';
import { NdefRecord, TypeNameFormatType } from '../../src/submodule/NdefRecord.js';
import { NdefUriRecord } from '../../src/submodule/NdefUriRecord.js';
import { NdefTextRecord } from '../../src/submodule/NdefTextRecord.js';

describe('Message Parsing Integration Tests', () => {
    describe('Round-trip tests (create → serialize → parse → verify)', () => {
        test('should handle single URI record round-trip', () => {
            // Create original URI record
            const originalUri = new NdefUriRecord('https://www.example.com');
            const originalMessage = new NdefMessage([originalUri]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            // Verify structure
            expect(parsedMessage.length).toBe(1);
            const parsedRecord = parsedMessage.records[0];
            expect(parsedRecord.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(parsedRecord.getType())).toEqual([0x55]); // 'U' type
            expect(Array.from(parsedRecord.getPayload())).toEqual(Array.from(originalUri.getPayload()));
        });

        test('should handle single text record round-trip', () => {
            // Create original text record
            const originalText = new NdefTextRecord('Hello World', 'en');
            const originalMessage = new NdefMessage([originalText]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            // Verify structure
            expect(parsedMessage.length).toBe(1);
            const parsedRecord = parsedMessage.records[0];
            expect(parsedRecord.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(parsedRecord.getType())).toEqual([0x54]); // 'T' type
            expect(parsedRecord.getPayload().length).toBeGreaterThan(0);
        });

        test('should handle empty record round-trip', () => {
            // Create empty record
            const emptyRecord = new NdefRecord();
            const originalMessage = new NdefMessage([emptyRecord]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            // Verify structure
            expect(parsedMessage.length).toBe(1);
            const parsedRecord = parsedMessage.records[0];
            expect(parsedRecord.getTypeNameFormat()).toBe(TypeNameFormatType.Empty);
            expect(Array.from(parsedRecord.getType())).toEqual([]);
            expect(Array.from(parsedRecord.getPayload())).toEqual([]);
        });

        test('should handle record with ID round-trip', () => {
            // Create record with ID
            const recordWithId = new NdefRecord(
                TypeNameFormatType.NfcRtd,
                [0x54], // 'T' type
                [0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F], // Text payload
                [0x01, 0x02, 0x03] // ID
            );
            const originalMessage = new NdefMessage([recordWithId]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Verify IL flag is set in serialization
            expect(bytes[0] & 0x08).toBe(0x08); // IL flag
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            // Verify structure
            expect(parsedMessage.length).toBe(1);
            const parsedRecord = parsedMessage.records[0];
            expect(parsedRecord.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(parsedRecord.getType())).toEqual([0x54]);
            expect(parsedRecord.getId().length).toBeGreaterThan(0);
        });

        test('should handle long record format round-trip', () => {
            // Create record with large payload (>255 bytes)
            const largePayload = new Array(300).fill(0x41); // 300 'A's
            const longRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], largePayload);
            const originalMessage = new NdefMessage([longRecord]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Verify long record format (no SR flag)
            expect(bytes[0] & 0x10).toBe(0x00); // SR flag should not be set
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            // Verify structure
            expect(parsedMessage.length).toBe(1);
            const parsedRecord = parsedMessage.records[0];
            expect(parsedRecord.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(parsedRecord.getPayload().length).toBeGreaterThan(250);
        });
    });

    describe('Complex multi-record message parsing', () => {
        test('should parse message with multiple different record types', () => {
            // Create message with URI and Text records
            const uriRecord = new NdefUriRecord('https://example.com');
            const textRecord = new NdefTextRecord('Hello', 'en');
            const originalMessage = new NdefMessage([uriRecord, textRecord]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Verify MB and ME flags in serialization
            expect(bytes[0] & 0x80).toBe(0x80); // First record should have MB flag
            
            // Second record starts after the first record's header (flags, type length, payload length, type) and payload
            const secondRecordStart = 4 + uriRecord.getPayload().length;
            expect(bytes[secondRecordStart] & 0x40).toBe(0x40); // Second record should have ME flag

            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);

            expect(parsedMessage.length).toBe(2);
            expect(NdefUriRecord.isRecordType(parsedMessage.records[0])).toBe(true);
            expect(NdefTextRecord.isRecordType(parsedMessage.records[1])).toBe(true);
            expect(Array.from(parsedMessage.records[0].getPayload())).toEqual(Array.from(uriRecord.getPayload()));
            expect(Array.from(parsedMessage.records[1].getPayload())).toEqual(Array.from(textRecord.getPayload()));
        });

        test('should parse message with three records', () => {
            // Create message with three simple records
            const record1 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x02, 0x65, 0x6E, 0x48, 0x69]); // Text "Hi"
            const record2 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x55], [0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65]); // URI google
            const record3 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x02, 0x65, 0x6E, 0x42, 0x79, 0x65]); // Text "Bye"
            const originalMessage = new NdefMessage([record1, record2, record3]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Verify flags in serialization
            expect(bytes[0] & 0x80).toBe(0x80); // First record MB flag
            expect(bytes[0] & 0x40).toBe(0x00); // First record no ME flag
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            expect(parsedMessage.length).toBe(3);
            [record1, record2, record3].forEach((original, index) => {
                expect(Array.from(parsedMessage.records[index].getType())).toEqual(original.getType());
                expect(Array.from(parsedMessage.records[index].getPayload())).toEqual(original.getPayload());
            });
        });

        test('should handle message with mixed record sizes', () => {
            // Create records with different payload sizes
            const shortRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x69]); // Short payload
            const longPayload = new Array(400).fill(0x42); // Long payload
            const longRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x55], longPayload);
            const originalMessage = new NdefMessage([shortRecord, longRecord]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Verify first record uses short format
            expect(bytes[0] & 0x10).toBe(0x10); // SR flag set for first record
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            // Verify basic structure
            expect(parsedMessage.length).toBeGreaterThanOrEqual(1);
            expect(parsedMessage.records[0].getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
        });

        test('should handle message with records containing IDs', () => {
            // Create records with and without IDs
            const recordWithId = new NdefRecord(
                TypeNameFormatType.NfcRtd,
                [0x54],
                [0x48, 0x65, 0x6C, 0x6C, 0x6F],
                [0x01, 0x02]
            );
            const recordWithoutId = new NdefRecord(TypeNameFormatType.NfcRtd, [0x55], [0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65]);
            const originalMessage = new NdefMessage([recordWithId, recordWithoutId]);
            
            // Serialize to bytes
            const bytes = originalMessage.toByteArray();
            
            // Verify IL flag for first record
            expect(bytes[0] & 0x08).toBe(0x08); // IL flag set
            
            // Parse back from bytes
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            // Verify basic structure
            expect(parsedMessage.length).toBeGreaterThanOrEqual(1);
            expect(parsedMessage.records[0].getId().length).toBeGreaterThan(0);
        });
    });

    describe('Malformed data handling and error conditions', () => {
        test('should handle empty byte array gracefully', () => {
            const emptyBytes = new Uint8Array([]);
            
            const message = NdefMessage.fromByteArray(emptyBytes);
            
            expect(message.length).toBe(0);
            expect(message.records).toEqual([]);
        });

        test('should handle single byte array', () => {
            const singleByte = new Uint8Array([0xD0]); // Just flags
            
            // Truncated data is rejected instead of returning garbage records
            expect(() => {
                NdefMessage.fromByteArray(singleByte);
            }).toThrow(/Invalid NDEF message/);
        });

        test('should handle truncated header', () => {
            // Incomplete header (missing payload length)
            const truncatedHeader = new Uint8Array([0xD1, 0x01]); // flags, type_len but no payload_len
            
            // Truncated data is rejected
            expect(() => {
                NdefMessage.fromByteArray(truncatedHeader);
            }).toThrow(/Invalid NDEF message/);
        });

        test('should handle header with missing payload data', () => {
            // Complete header but missing payload
            const headerOnly = new Uint8Array([0xD1, 0x01, 0x05, 0x54]); // Says payload is 5 bytes but none provided
            
            // Truncated data is rejected
            expect(() => {
                NdefMessage.fromByteArray(headerOnly);
            }).toThrow(/Invalid NDEF message/);
        });

        test('should handle invalid TNF values', () => {
            // Record with reserved TNF value
            const invalidTnf = new Uint8Array([
                0xD7, 0x01, 0x05, 0x54, // TNF = 7 (reserved)
                0x48, 0x65, 0x6C, 0x6C, 0x6F // payload
            ]);
            
            const message = NdefMessage.fromByteArray(invalidTnf);
            
            // Should parse but with reserved TNF
            expect(message.length).toBe(1);
            expect(message.records[0].getTypeNameFormat()).toBe(0x07); // Reserved TNF
        });

        test('should handle message without ME flag', () => {
            // Record without ME flag (malformed single record message)
            const noMeFlag = new Uint8Array([
                0x91, 0x01, 0x05, 0x54, // MB set but no ME flag
                0x48, 0x65, 0x6C, 0x6C, 0x6F
            ]);
            
            // Lenient parsing: the record is returned even though the ME flag is missing
            const message = NdefMessage.fromByteArray(noMeFlag);
            expect(message.length).toBe(1);
            expect(message.records[0].getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(message.records[0].getPayload())).toEqual([0x48, 0x65, 0x6C, 0x6C, 0x6F]);
        });

        test('should handle long record with incorrect length', () => {
            // Long record format with payload length that exceeds available data
            const incorrectLength = new Uint8Array([
                0xC1, 0x01, // flags (long format), type_len
                0x00, 0x00, 0x03, 0xE8, // payload_len = 1000 bytes
                0x54, // type
                0x48, 0x65, 0x6C, 0x6C, 0x6F // only 5 bytes of payload
            ]);
            
            // Truncated data is rejected
            expect(() => {
                NdefMessage.fromByteArray(incorrectLength);
            }).toThrow(/Invalid NDEF message/);
        });

        test('should handle record with ID length but no ID data', () => {
            // Record with IL flag and ID length but missing ID data
            const missingId = new Uint8Array([
                0xD9, 0x01, 0x05, 0x03, 0x54, // IL flag set, id_len=3, type='T'
                // Missing ID bytes
                0x48, 0x65, 0x6C, 0x6C, 0x6F // payload
            ]);
            
            // Truncated data is rejected
            expect(() => {
                NdefMessage.fromByteArray(missingId);
            }).toThrow(/Invalid NDEF message/);
        });

        test('should handle null or undefined input', () => {
            // Test null input
            expect(() => {
                NdefMessage.fromByteArray(null);
            }).toThrow();
            
            // Test undefined input
            expect(() => {
                NdefMessage.fromByteArray(undefined);
            }).toThrow();
        });
    });

    describe('Edge cases and boundary conditions', () => {
        test('should handle maximum short record payload (255 bytes)', () => {
            const maxShortPayload = new Array(255).fill(0x41);
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], maxShortPayload);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            expect(parsedMessage.length).toBe(1);
            expect(parsedMessage.records[0].getPayload().length).toBeGreaterThan(200);
        });

        test('should handle minimum long record payload (256 bytes)', () => {
            const minLongPayload = new Array(256).fill(0x42);
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], minLongPayload);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Verify long format is used
            expect(bytes[0] & 0x10).toBe(0x00); // No SR flag
            
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            expect(parsedMessage.length).toBe(1);
            expect(parsedMessage.records[0].getPayload().length).toBeGreaterThan(200);
        });

        test('should handle record with maximum type length (255 bytes)', () => {
            const maxType = new Array(255).fill(0x58); // 255 'X's
            const record = new NdefRecord(TypeNameFormatType.ExternalRtd, maxType, [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            expect(parsedMessage.length).toBe(1);
            expect(parsedMessage.records[0].getTypeNameFormat()).toBe(TypeNameFormatType.ExternalRtd);
        });

        test('should handle record with maximum ID length (255 bytes)', () => {
            const maxId = new Array(255).fill(0x49); // 255 'I's
            const record = new NdefRecord(
                TypeNameFormatType.NfcRtd,
                [0x54],
                [0x48, 0x65, 0x6C, 0x6C, 0x6F],
                maxId
            );
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Verify IL flag is set
            expect(bytes[0] & 0x08).toBe(0x08);
            
            const parsedMessage = NdefMessage.fromByteArray(bytes);
            
            expect(parsedMessage.length).toBe(1);
            expect(parsedMessage.records[0].getId().length).toBeGreaterThan(200);
        });

        test('should handle all TNF types', () => {
            const tnfTypes = [
                TypeNameFormatType.Empty,
                TypeNameFormatType.NfcRtd,
                TypeNameFormatType.Mime,
                TypeNameFormatType.Uri,
                TypeNameFormatType.ExternalRtd,
                TypeNameFormatType.Unknown,
                TypeNameFormatType.Unchanged
            ];
            
            tnfTypes.forEach(tnf => {
                let type = [];
                let payload = [];
                
                // Set appropriate type and payload based on TNF rules
                if (tnf !== TypeNameFormatType.Empty && tnf !== TypeNameFormatType.Unknown && tnf !== TypeNameFormatType.Unchanged) {
                    type = [0x54];
                    payload = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
                }
                
                const record = new NdefRecord(tnf, type, payload);
                const message = new NdefMessage([record]);
                
                const bytes = message.toByteArray();
                const parsedMessage = NdefMessage.fromByteArray(bytes);
                
                expect(parsedMessage.length).toBe(1);
                expect(parsedMessage.records[0].getTypeNameFormat()).toBe(tnf);
            });
        });
    });
});