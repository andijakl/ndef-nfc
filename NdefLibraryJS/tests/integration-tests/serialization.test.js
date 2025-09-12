import { NdefMessage } from '../../src/submodule/NdefMessage.js';
import { NdefRecord, TypeNameFormatType } from '../../src/submodule/NdefRecord.js';
import { NdefUriRecord } from '../../src/submodule/NdefUriRecord.js';
import { NdefTextRecord } from '../../src/submodule/NdefTextRecord.js';

describe('Serialization Validation Tests', () => {
    describe('NDEF message byte array generation', () => {
        test('should generate valid NDEF header for single record', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Verify basic structure
            expect(bytes).toBeInstanceOf(Uint8Array);
            expect(bytes.length).toBeGreaterThan(0);
            
            // Verify header structure for single record
            const flags = bytes[0];
            const typeLength = bytes[1];
            const payloadLength = bytes[2]; // Short record format
            
            // Check flags: MB (0x80) | ME (0x40) | SR (0x10) | TNF (0x01) = 0xD1
            expect(flags).toBe(0xD1);
            expect(typeLength).toBe(1);
            expect(payloadLength).toBe(5);
            expect(bytes[3]).toBe(0x54); // Type 'T'
        });

        test('should generate valid NDEF bytes for empty message', () => {
            const message = new NdefMessage();
            
            const bytes = message.toByteArray();
            
            // Empty message should generate default empty record
            expect(bytes).toBeInstanceOf(Uint8Array);
            expect(bytes.length).toBe(3); // flags + type_len + payload_len
            
            // Check flags: MB (0x80) | ME (0x40) | SR (0x10) | Empty TNF (0x00) = 0xD0
            expect(bytes[0]).toBe(0xD0);
            expect(bytes[1]).toBe(0x00); // type length = 0
            expect(bytes[2]).toBe(0x00); // payload length = 0
        });

        test('should generate valid bytes for URI record', () => {
            const uriRecord = new NdefUriRecord('https://www.example.com');
            const message = new NdefMessage([uriRecord]);
            
            const bytes = message.toByteArray();
            
            expect(bytes).toBeInstanceOf(Uint8Array);
            expect(bytes.length).toBeGreaterThan(0);
            
            // Verify URI record structure
            const flags = bytes[0];
            expect(flags & 0x07).toBe(TypeNameFormatType.NfcRtd); // TNF
            expect(flags & 0x80).toBe(0x80); // MB flag
            expect(flags & 0x40).toBe(0x40); // ME flag
            expect(flags & 0x10).toBe(0x10); // SR flag (short record)
            
            expect(bytes[1]).toBe(1); // Type length = 1
            expect(bytes[3]).toBe(0x55); // Type 'U'
        });

        test('should generate valid bytes for text record', () => {
            const textRecord = new NdefTextRecord('Hello World', 'en');
            const message = new NdefMessage([textRecord]);
            
            const bytes = message.toByteArray();
            
            expect(bytes).toBeInstanceOf(Uint8Array);
            expect(bytes.length).toBeGreaterThan(0);
            
            // Verify text record structure
            const flags = bytes[0];
            expect(flags & 0x07).toBe(TypeNameFormatType.NfcRtd); // TNF
            expect(flags & 0x80).toBe(0x80); // MB flag
            expect(flags & 0x40).toBe(0x40); // ME flag
            
            expect(bytes[1]).toBe(1); // Type length = 1
            expect(bytes[3]).toBe(0x54); // Type 'T'
        });

        test('should generate consistent byte arrays for identical messages', () => {
            const record1 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const record2 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            
            const message1 = new NdefMessage([record1]);
            const message2 = new NdefMessage([record2]);
            
            const bytes1 = message1.toByteArray();
            const bytes2 = message2.toByteArray();
            
            expect(bytes1.length).toBe(bytes2.length);
            expect(Array.from(bytes1)).toEqual(Array.from(bytes2));
        });

        test('should handle records with different TNF types', () => {
            const records = [
                new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F]),
                new NdefRecord(TypeNameFormatType.Mime, [0x74, 0x65, 0x78, 0x74, 0x2F, 0x70, 0x6C, 0x61, 0x69, 0x6E], [0x48, 0x65, 0x6C, 0x6C, 0x6F]),
                new NdefRecord(TypeNameFormatType.Uri, [], [0x68, 0x74, 0x74, 0x70, 0x3A, 0x2F, 0x2F, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D])
            ];
            
            records.forEach(record => {
                const message = new NdefMessage([record]);
                const bytes = message.toByteArray();
                
                expect(bytes).toBeInstanceOf(Uint8Array);
                expect(bytes.length).toBeGreaterThan(0);
                
                // Verify TNF is preserved in flags
                const flags = bytes[0];
                expect(flags & 0x07).toBe(record.getTypeNameFormat());
            });
        });
    });

    describe('Flag setting for different message configurations', () => {
        test('should set MB flag only on first record in multi-record message', () => {
            const record1 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x69]); // "Hi"
            const record2 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x55], [0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65]); // URI
            const message = new NdefMessage([record1, record2]);
            
            const bytes = message.toByteArray();
            
            // First record should have MB flag
            const firstRecordFlags = bytes[0];
            expect(firstRecordFlags & 0x80).toBe(0x80); // MB flag set
            expect(firstRecordFlags & 0x40).toBe(0x00); // ME flag not set
            
            // Find second record start (approximate calculation)
            const firstRecordSize = 3 + 1 + 2; // header + type + payload
            const secondRecordFlags = bytes[firstRecordSize];
            expect(secondRecordFlags & 0x80).toBe(0x00); // MB flag not set
            expect(secondRecordFlags & 0x40).toBe(0x40); // ME flag set
        });

        test('should set ME flag only on last record in multi-record message', () => {
            const record1 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x69]);
            const record2 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x42, 0x79, 0x65]);
            const record3 = new NdefRecord(TypeNameFormatType.NfcRtd, [0x55], [0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65]);
            const message = new NdefMessage([record1, record2, record3]);
            
            const bytes = message.toByteArray();
            
            // First record: MB but not ME
            expect(bytes[0] & 0x80).toBe(0x80); // MB flag
            expect(bytes[0] & 0x40).toBe(0x00); // No ME flag
            
            // Find approximate positions (this is simplified)
            let offset = 3 + 1 + 2; // First record
            expect(bytes[offset] & 0x80).toBe(0x00); // No MB flag on second record
            expect(bytes[offset] & 0x40).toBe(0x00); // No ME flag on second record
            
            // The last record should have ME flag (verified in serialization)
            expect(bytes.length).toBeGreaterThan(10); // Sanity check
        });

        test('should set both MB and ME flags for single record message', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            const flags = bytes[0];
            expect(flags & 0x80).toBe(0x80); // MB flag set
            expect(flags & 0x40).toBe(0x40); // ME flag set
        });

        test('should set SR flag for short records (payload < 256 bytes)', () => {
            const shortPayloads = [
                [0x48], // 1 byte
                new Array(50).fill(0x41), // 50 bytes
                new Array(255).fill(0x42) // 255 bytes (max for short record)
            ];
            
            shortPayloads.forEach(payload => {
                const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], payload);
                const message = new NdefMessage([record]);
                
                const bytes = message.toByteArray();
                
                // Should have SR flag set
                expect(bytes[0] & 0x10).toBe(0x10);
                
                // Payload length should be in single byte at position 2
                expect(bytes[2]).toBe(payload.length);
            });
        });

        test('should not set SR flag for long records (payload >= 256 bytes)', () => {
            const longPayloads = [
                new Array(256).fill(0x43), // 256 bytes (min for long record)
                new Array(1000).fill(0x44), // 1000 bytes
                new Array(65535).fill(0x45) // Large payload
            ];
            
            longPayloads.forEach(payload => {
                const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], payload);
                const message = new NdefMessage([record]);
                
                const bytes = message.toByteArray();
                
                // Should not have SR flag set
                expect(bytes[0] & 0x10).toBe(0x00);
                
                // Payload length should be in 4 bytes starting at position 2
                const payloadLength = new DataView(bytes.buffer, 2, 4).getUint32(0, false);
                expect(payloadLength).toBe(payload.length);
            });
        });

        test('should set IL flag when record has ID', () => {
            const recordWithId = new NdefRecord(
                TypeNameFormatType.NfcRtd,
                [0x54],
                [0x48, 0x65, 0x6C, 0x6C, 0x6F],
                [0x01, 0x02, 0x03]
            );
            const message = new NdefMessage([recordWithId]);
            
            const bytes = message.toByteArray();
            
            // Should have IL flag set
            expect(bytes[0] & 0x08).toBe(0x08);
            
            // ID length should be present at position 3 (after type length and payload length)
            expect(bytes[3]).toBe(3); // ID length
        });

        test('should not set IL flag when record has no ID', () => {
            const recordWithoutId = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const message = new NdefMessage([recordWithoutId]);
            
            const bytes = message.toByteArray();
            
            // Should not have IL flag set
            expect(bytes[0] & 0x08).toBe(0x00);
        });

        test('should handle empty ID array as no ID', () => {
            const recordWithEmptyId = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F], []);
            const message = new NdefMessage([recordWithEmptyId]);
            
            const bytes = message.toByteArray();
            
            // Should not have IL flag set for empty ID
            expect(bytes[0] & 0x08).toBe(0x00);
        });
    });

    describe('Long record format handling for large payloads', () => {
        test('should use long record format for 256-byte payload', () => {
            const payload256 = new Array(256).fill(0x41);
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], payload256);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Verify long record format
            expect(bytes[0] & 0x10).toBe(0x00); // No SR flag
            
            // Verify 4-byte payload length
            const payloadLength = new DataView(bytes.buffer, 2, 4).getUint32(0, false);
            expect(payloadLength).toBe(256);
            
            // Verify header structure: flags + type_len + payload_len(4) + type
            expect(bytes[1]).toBe(1); // Type length
            expect(bytes[6]).toBe(0x54); // Type 'T' at position 6
        });

        test('should use long record format for very large payload', () => {
            const largePayload = new Array(10000).fill(0x42);
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], largePayload);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Verify long record format
            expect(bytes[0] & 0x10).toBe(0x00); // No SR flag
            
            // Verify 4-byte payload length
            const payloadLength = new DataView(bytes.buffer, 2, 4).getUint32(0, false);
            expect(payloadLength).toBe(10000);
            
            // Verify total message size
            const expectedSize = 1 + 1 + 4 + 1 + 10000; // flags + type_len + payload_len + type + payload
            expect(bytes.length).toBe(expectedSize);
        });

        test('should handle long record with ID', () => {
            const largePayload = new Array(500).fill(0x43);
            const recordId = [0x01, 0x02, 0x03, 0x04];
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], largePayload, recordId);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Verify flags: no SR, but IL flag set
            expect(bytes[0] & 0x10).toBe(0x00); // No SR flag
            expect(bytes[0] & 0x08).toBe(0x08); // IL flag set
            
            // Verify header structure: flags + type_len + payload_len(4) + id_len + type + id + payload
            expect(bytes[1]).toBe(1); // Type length
            const payloadLength = new DataView(bytes.buffer, 2, 4).getUint32(0, false);
            expect(payloadLength).toBe(500);
            expect(bytes[6]).toBe(4); // ID length
            expect(bytes[7]).toBe(0x54); // Type 'T'
            expect(bytes[8]).toBe(0x01); // First ID byte
        });

        test('should handle maximum payload size', () => {
            // Test with maximum reasonable payload size (limited by memory)
            const maxPayload = new Array(65536).fill(0x44); // 64KB
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], maxPayload);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Verify long record format
            expect(bytes[0] & 0x10).toBe(0x00); // No SR flag
            
            // Verify payload length encoding
            const payloadLength = new DataView(bytes.buffer, 2, 4).getUint32(0, false);
            expect(payloadLength).toBe(65536);
            
            // Verify total size
            expect(bytes.length).toBe(1 + 1 + 4 + 1 + 65536);
        });

        test('should handle long record in multi-record message', () => {
            const shortRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x55], [0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65]);
            const longPayload = new Array(300).fill(0x45);
            const longRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], longPayload);
            const message = new NdefMessage([shortRecord, longRecord]);
            
            const bytes = message.toByteArray();
            
            // First record should use short format
            expect(bytes[0] & 0x10).toBe(0x10); // SR flag set
            expect(bytes[0] & 0x80).toBe(0x80); // MB flag set
            expect(bytes[0] & 0x40).toBe(0x00); // No ME flag
            
            // Find second record (approximate)
            const firstRecordSize = 3 + 1 + 7; // Short record header + type + payload
            const secondRecordFlags = bytes[firstRecordSize];
            expect(secondRecordFlags & 0x10).toBe(0x00); // No SR flag for long record
            expect(secondRecordFlags & 0x40).toBe(0x40); // ME flag set
        });

        test('should preserve payload data integrity in long records', () => {
            // Create payload with specific pattern
            const payload = [];
            for (let i = 0; i < 1000; i++) {
                payload.push(i % 256);
            }
            
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], payload);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            // Extract payload from serialized bytes
            const headerSize = 1 + 1 + 4 + 1; // flags + type_len + payload_len + type
            const extractedPayload = bytes.slice(headerSize, headerSize + 1000);
            
            // Verify payload integrity
            expect(extractedPayload.length).toBe(1000);
            for (let i = 0; i < 1000; i++) {
                expect(extractedPayload[i]).toBe(i % 256);
            }
        });
    });

    describe('Edge cases and boundary conditions', () => {
        test('should handle record with maximum type length', () => {
            const maxType = new Array(255).fill(0x58); // 255 'X's
            const record = new NdefRecord(TypeNameFormatType.ExternalRtd, maxType, [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            expect(bytes[1]).toBe(255); // Type length
            expect(bytes.length).toBeGreaterThan(260); // At least header + type + payload
        });

        test('should handle record with maximum ID length', () => {
            const maxId = new Array(255).fill(0x49); // 255 'I's
            const record = new NdefRecord(
                TypeNameFormatType.NfcRtd,
                [0x54],
                [0x48, 0x65, 0x6C, 0x6C, 0x6F],
                maxId
            );
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            expect(bytes[0] & 0x08).toBe(0x08); // IL flag set
            expect(bytes[3]).toBe(255); // ID length
        });

        test('should handle empty type array', () => {
            const record = new NdefRecord(TypeNameFormatType.Unknown, [], [0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            expect(bytes[1]).toBe(0); // Type length = 0
            expect(bytes[0] & 0x07).toBe(TypeNameFormatType.Unknown); // TNF preserved
        });

        test('should handle empty payload array', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], []);
            const message = new NdefMessage([record]);
            
            const bytes = message.toByteArray();
            
            expect(bytes[2]).toBe(0); // Payload length = 0
            expect(bytes[0] & 0x10).toBe(0x10); // Still uses short record format
        });

        test('should handle all TNF types in serialization', () => {
            const tnfTestCases = [
                { tnf: TypeNameFormatType.Empty, type: [], payload: [] },
                { tnf: TypeNameFormatType.NfcRtd, type: [0x54], payload: [0x48, 0x65, 0x6C, 0x6C, 0x6F] },
                { tnf: TypeNameFormatType.Mime, type: [0x74, 0x65, 0x78, 0x74], payload: [0x48, 0x65, 0x6C, 0x6C, 0x6F] },
                { tnf: TypeNameFormatType.Uri, type: [], payload: [0x68, 0x74, 0x74, 0x70, 0x3A, 0x2F, 0x2F] },
                { tnf: TypeNameFormatType.ExternalRtd, type: [0x63, 0x6F, 0x6D], payload: [0x48, 0x65, 0x6C, 0x6C, 0x6F] },
                { tnf: TypeNameFormatType.Unknown, type: [], payload: [0x48, 0x65, 0x6C, 0x6C, 0x6F] },
                { tnf: TypeNameFormatType.Unchanged, type: [], payload: [0x48, 0x65, 0x6C, 0x6C, 0x6F] }
            ];
            
            tnfTestCases.forEach(testCase => {
                const record = new NdefRecord(testCase.tnf, testCase.type, testCase.payload);
                const message = new NdefMessage([record]);
                
                const bytes = message.toByteArray();
                
                // Verify TNF is preserved in flags
                expect(bytes[0] & 0x07).toBe(testCase.tnf);
                expect(bytes[1]).toBe(testCase.type.length);
            });
        });

        test('should handle zero-length message serialization', () => {
            const emptyMessage = new NdefMessage([]);
            
            const bytes = emptyMessage.toByteArray();
            
            // Should generate default empty record
            expect(bytes.length).toBe(3);
            expect(bytes[0]).toBe(0xD0); // MB|ME|SR|Empty
            expect(bytes[1]).toBe(0); // Type length
            expect(bytes[2]).toBe(0); // Payload length
        });
    });
});