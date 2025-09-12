import { NdefRecord, TypeNameFormatType } from '../../src/submodule/NdefRecord.js';

describe('NdefRecord', () => {
    describe('Constructor variations', () => {
        test('should create empty record with no parameters', () => {
            const record = new NdefRecord();
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.Empty);
            expect(record.getType()).toEqual([]);
            expect(record.getPayload()).toEqual([]);
            expect(record.getId()).toEqual([]);
        });

        test('should create record with TNF only', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd);
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(record.getType()).toEqual([]);
            expect(record.getPayload()).toEqual([]);
            expect(record.getId()).toEqual([]);
        });

        test('should create record with TNF and type', () => {
            const type = [0x54]; // 'T' for text record
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, type);
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(record.getType()).toEqual(type);
            expect(record.getPayload()).toEqual([]);
            expect(record.getId()).toEqual([]);
        });

        test('should create record with TNF, type, and payload', () => {
            const type = [0x54]; // 'T'
            const payload = [0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F]; // "Hello" in English
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, type, payload);
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(record.getType()).toEqual(type);
            expect(record.getPayload()).toEqual(payload);
            expect(record.getId()).toEqual([]);
        });

        test('should create record with all parameters', () => {
            const type = [0x54]; // 'T'
            const payload = [0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F];
            const id = [0x01, 0x02, 0x03];
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, type, payload, id);
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(record.getType()).toEqual(type);
            expect(record.getPayload()).toEqual(payload);
            expect(record.getId()).toEqual(id);
        });

        test('should create record from copy constructor', () => {
            const originalType = [0x55]; // 'U' for URI
            const originalPayload = [0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65]; // Google with abbreviation
            const originalId = [0x04, 0x05, 0x06];
            const original = new NdefRecord(TypeNameFormatType.NfcRtd, originalType, originalPayload, originalId);
            
            const copy = new NdefRecord(original);
            
            expect(copy.getTypeNameFormat()).toBe(original.getTypeNameFormat());
            expect(copy.getType()).toEqual(original.getType());
            expect(copy.getPayload()).toEqual(original.getPayload());
            expect(copy.getId()).toEqual(original.getId());
            
            // Verify deep copy - modifying copy shouldn't affect original
            copy.setType([0x54]);
            expect(original.getType()).toEqual(originalType);
        });
    });

    describe('Type name format handling', () => {
        test('should set and get type name format', () => {
            const record = new NdefRecord();
            
            record.setTypeNameFormat(TypeNameFormatType.Mime);
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.Mime);
            
            record.setTypeNameFormat(TypeNameFormatType.Uri);
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.Uri);
        });

        test('should handle all TNF types', () => {
            const record = new NdefRecord();
            
            Object.values(TypeNameFormatType).forEach(tnf => {
                record.setTypeNameFormat(tnf);
                expect(record.getTypeNameFormat()).toBe(tnf);
            });
        });
    });

    describe('Type manipulation methods', () => {
        test('should set and get type array', () => {
            const record = new NdefRecord();
            const type = [0x54, 0x65, 0x78, 0x74]; // "Text"
            
            record.setType(type);
            expect(record.getType()).toEqual(type);
        });

        test('should handle null type', () => {
            const record = new NdefRecord();
            
            record.setType(null);
            expect(record.getType()).toBeNull();
        });

        test('should create deep copy of type array', () => {
            const record = new NdefRecord();
            const originalType = [0x54, 0x65, 0x78, 0x74];
            
            record.setType(originalType);
            const retrievedType = record.getType();
            
            // Modify original array
            originalType[0] = 0x55;
            
            // Retrieved type should be unchanged
            expect(retrievedType[0]).toBe(0x54);
        });

        test('should handle empty type array', () => {
            const record = new NdefRecord();
            const emptyType = [];
            
            record.setType(emptyType);
            expect(record.getType()).toEqual([]);
        });
    });

    describe('Payload manipulation methods', () => {
        test('should set and get payload array', () => {
            const record = new NdefRecord();
            const payload = [0x48, 0x65, 0x6C, 0x6C, 0x6F]; // "Hello"
            
            record.setPayload(payload);
            expect(record.getPayload()).toEqual(payload);
        });

        test('should handle null payload', () => {
            const record = new NdefRecord();
            
            record.setPayload(null);
            expect(record.getPayload()).toBeNull();
        });

        test('should create deep copy of payload array', () => {
            const record = new NdefRecord();
            const originalPayload = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
            
            record.setPayload(originalPayload);
            const retrievedPayload = record.getPayload();
            
            // Modify original array
            originalPayload[0] = 0x57; // 'W'
            
            // Retrieved payload should be unchanged
            expect(retrievedPayload[0]).toBe(0x48); // 'H'
        });

        test('should handle empty payload array', () => {
            const record = new NdefRecord();
            const emptyPayload = [];
            
            record.setPayload(emptyPayload);
            expect(record.getPayload()).toEqual([]);
        });

        test('should handle large payload', () => {
            const record = new NdefRecord();
            const largePayload = new Array(1000).fill(0x41); // 1000 'A's
            
            record.setPayload(largePayload);
            expect(record.getPayload()).toEqual(largePayload);
            expect(record.getPayload().length).toBe(1000);
        });
    });

    describe('ID manipulation methods', () => {
        test('should set and get ID array', () => {
            const record = new NdefRecord();
            const id = [0x01, 0x02, 0x03, 0x04];
            
            record.setId(id);
            expect(record.getId()).toEqual(id);
        });

        test('should handle null ID', () => {
            const record = new NdefRecord();
            
            record.setId(null);
            expect(record.getId()).toBeNull();
        });

        test('should create deep copy of ID array', () => {
            const record = new NdefRecord();
            const originalId = [0x01, 0x02, 0x03];
            
            record.setId(originalId);
            const retrievedId = record.getId();
            
            // Modify original array
            originalId[0] = 0xFF;
            
            // Retrieved ID should be unchanged
            expect(retrievedId[0]).toBe(0x01);
        });

        test('should handle empty ID array', () => {
            const record = new NdefRecord();
            const emptyId = [];
            
            record.setId(emptyId);
            expect(record.getId()).toEqual([]);
        });
    });

    describe('Validation methods', () => {
        test('should validate empty record', () => {
            const record = new NdefRecord(TypeNameFormatType.Empty);
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should validate NFC RTD record with type', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54]);
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should validate MIME record with type', () => {
            const record = new NdefRecord(TypeNameFormatType.Mime, [0x74, 0x65, 0x78, 0x74, 0x2F, 0x70, 0x6C, 0x61, 0x69, 0x6E]); // "text/plain"
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should validate URI record with type', () => {
            const record = new NdefRecord(TypeNameFormatType.Uri, [0x68, 0x74, 0x74, 0x70, 0x3A, 0x2F, 0x2F]); // "http://"
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should validate External RTD record with type', () => {
            const record = new NdefRecord(TypeNameFormatType.ExternalRtd, [0x63, 0x6F, 0x6D, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65]); // "com.example"
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should validate Unknown record without type', () => {
            const record = new NdefRecord(TypeNameFormatType.Unknown);
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should validate Unchanged record without type', () => {
            const record = new NdefRecord(TypeNameFormatType.Unchanged);
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should throw error for Unknown TNF with type', () => {
            const record = new NdefRecord(TypeNameFormatType.Unknown, [0x54]);
            
            expect(() => record.checkIfValid()).toThrow('Unchanged and Unknown TNF must have a type length of 0');
        });

        test('should throw error for Unchanged TNF with type', () => {
            const record = new NdefRecord(TypeNameFormatType.Unchanged, [0x54]);
            
            expect(() => record.checkIfValid()).toThrow('Unchanged and Unknown TNF must have a type length of 0');
        });

        test('should throw error for non-Empty TNF without type', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd);
            
            expect(() => record.checkIfValid()).toThrow('All other TNF (except Empty) should have a type set');
        });

        test('should throw error for Unchanged TNF with ID', () => {
            const record = new NdefRecord(TypeNameFormatType.Unchanged, null, null, [0x01]);
            
            expect(() => record.checkIfValid()).toThrow('Unchanged TNF must not have an ID field');
        });

        test('should allow other TNF types with ID', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F], [0x01, 0x02]);
            
            expect(() => record.checkIfValid()).not.toThrow();
        });
    });

    describe('Edge cases and error conditions', () => {
        test('should handle undefined parameters in constructor', () => {
            const record = new NdefRecord(undefined, undefined, undefined, undefined);
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.Empty);
            expect(record.getType()).toEqual([]);
            expect(record.getPayload()).toEqual([]);
            expect(record.getId()).toEqual([]);
        });

        test('should handle setting undefined values', () => {
            const record = new NdefRecord();
            
            record.setType(undefined);
            record.setPayload(undefined);
            record.setId(undefined);
            
            expect(record.getType()).toBeNull();
            expect(record.getPayload()).toBeNull();
            expect(record.getId()).toBeNull();
        });

        test('should handle numeric TNF values', () => {
            const record = new NdefRecord();
            
            record.setTypeNameFormat(0x01);
            expect(record.getTypeNameFormat()).toBe(0x01);
            
            record.setTypeNameFormat(0x07);
            expect(record.getTypeNameFormat()).toBe(0x07);
        });

        test('should handle array-like objects for type, payload, and ID', () => {
            const record = new NdefRecord();
            
            // Test with Uint8Array
            const uint8Type = new Uint8Array([0x54]);
            const uint8Payload = new Uint8Array([0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            const uint8Id = new Uint8Array([0x01, 0x02]);
            
            record.setType(uint8Type);
            record.setPayload(uint8Payload);
            record.setId(uint8Id);
            
            // Uint8Array.slice() returns a Uint8Array, so we need to convert to regular array for comparison
            expect(Array.from(record.getType())).toEqual([0x54]);
            expect(Array.from(record.getPayload())).toEqual([0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            expect(Array.from(record.getId())).toEqual([0x01, 0x02]);
        });

        test('should return direct references to internal arrays (mutable)', () => {
            const record = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x48, 0x65, 0x6C, 0x6C, 0x6F], [0x01]);
            
            const type = record.getType();
            const payload = record.getPayload();
            const id = record.getId();
            
            // Modify returned arrays - this will modify the internal arrays
            type.push(0x55);
            payload.push(0x21);
            id.push(0x02);
            
            // Record should reflect the changes since getters return references
            expect(record.getType()).toEqual([0x54, 0x55]);
            expect(record.getPayload()).toEqual([0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x21]);
            expect(record.getId()).toEqual([0x01, 0x02]);
        });
    });

    describe('TypeNameFormatType constants', () => {
        test('should have correct TNF constant values', () => {
            expect(TypeNameFormatType.Empty).toBe(0x00);
            expect(TypeNameFormatType.NfcRtd).toBe(0x01);
            expect(TypeNameFormatType.Mime).toBe(0x02);
            expect(TypeNameFormatType.Uri).toBe(0x03);
            expect(TypeNameFormatType.ExternalRtd).toBe(0x04);
            expect(TypeNameFormatType.Unknown).toBe(0x05);
            expect(TypeNameFormatType.Unchanged).toBe(0x06);
            expect(TypeNameFormatType.Reserved).toBe(0x07);
        });

        test('should have all expected TNF types', () => {
            const expectedTypes = ['Empty', 'NfcRtd', 'Mime', 'Uri', 'ExternalRtd', 'Unknown', 'Unchanged', 'Reserved'];
            const actualTypes = Object.keys(TypeNameFormatType);
            
            expect(actualTypes).toEqual(expect.arrayContaining(expectedTypes));
            expect(actualTypes.length).toBe(expectedTypes.length);
        });
    });
});