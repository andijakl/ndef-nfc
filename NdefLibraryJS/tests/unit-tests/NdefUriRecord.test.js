import { NdefUriRecord } from '../../src/submodule/NdefUriRecord.js';
import { NdefRecord, TypeNameFormatType } from '../../src/submodule/NdefRecord.js';

describe('NdefUriRecord', () => {
    describe('URI record creation', () => {
        test('should create empty URI record with no parameters', () => {
            const record = new NdefUriRecord();
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(record.getType())).toEqual([0x55]); // 'U'
            expect(record.getUri()).toBe("");
        });

        test('should create URI record with simple URI', () => {
            const uri = "example.com";
            const record = new NdefUriRecord(uri);
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(record.getType())).toEqual([0x55]);
            expect(record.getUri()).toBe(uri);
        });

        test('should create URI record with http:// URI', () => {
            const uri = "http://example.com";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });

        test('should create URI record with https:// URI', () => {
            const uri = "https://example.com";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });

        test('should create URI record with tel: URI', () => {
            const uri = "tel:+1234567890";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });

        test('should create URI record with mailto: URI', () => {
            const uri = "mailto:test@example.com";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });
    });

    describe('URI abbreviation handling', () => {
        test('should use http://www. abbreviation (code 1)', () => {
            const uri = "http://www.example.com";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(1); // http://www. abbreviation code
            
            // Verify the remaining URI is stored correctly
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe("example.com");
            
            expect(record.getUri()).toBe(uri);
        });

        test('should use https://www. abbreviation (code 2)', () => {
            const uri = "https://www.example.com";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(2); // https://www. abbreviation code
            
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe("example.com");
            
            expect(record.getUri()).toBe(uri);
        });

        test('should use http:// abbreviation (code 3)', () => {
            const uri = "http://example.com";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(3); // http:// abbreviation code
            
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe("example.com");
            
            expect(record.getUri()).toBe(uri);
        });

        test('should use https:// abbreviation (code 4)', () => {
            const uri = "https://example.com";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(4); // https:// abbreviation code
            
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe("example.com");
            
            expect(record.getUri()).toBe(uri);
        });

        test('should use tel: abbreviation (code 5)', () => {
            const uri = "tel:+1234567890";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(5); // tel: abbreviation code
            
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe("+1234567890");
            
            expect(record.getUri()).toBe(uri);
        });

        test('should use mailto: abbreviation (code 6)', () => {
            const uri = "mailto:test@example.com";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(6); // mailto: abbreviation code
            
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe("test@example.com");
            
            expect(record.getUri()).toBe(uri);
        });

        test('should use no abbreviation for unrecognized schemes', () => {
            const uri = "custom://example.com";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(0); // no abbreviation
            
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe(uri);
            
            expect(record.getUri()).toBe(uri);
        });

        test('should choose longest matching abbreviation', () => {
            // https://www. should be preferred over https://
            const uri = "https://www.example.com/path";
            const record = new NdefUriRecord(uri);
            
            const payload = record.getPayload();
            expect(payload[0]).toBe(2); // https://www. abbreviation code
            
            const remainingUri = new TextDecoder().decode(payload.slice(1));
            expect(remainingUri).toBe("example.com/path");
            
            expect(record.getUri()).toBe(uri);
        });
    });

    describe('URI encoding and decoding', () => {
        test('should handle UTF-8 encoded URIs', () => {
            const uri = "http://example.com/测试";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });

        test('should handle URIs with special characters', () => {
            const uri = "https://example.com/path?param=value&other=123#fragment";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });

        test('should handle empty URI string', () => {
            const uri = "";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });

        test('should handle very long URIs', () => {
            const longPath = "a".repeat(1000);
            const uri = `https://example.com/${longPath}`;
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });
    });

    describe('Raw URI methods', () => {
        test('should set and get raw URI without abbreviation', () => {
            const record = new NdefUriRecord();
            const rawUri = new TextEncoder().encode("example.com");
            
            record.setRawUri(rawUri);
            
            const retrievedRawUri = record.getRawUri();
            expect(Array.from(retrievedRawUri)).toEqual(Array.from(rawUri));
            
            // Should have no abbreviation code (0)
            const payload = record.getPayload();
            expect(payload[0]).toBe(0);
        });

        test('should return null for empty payload in getRawUri', () => {
            const record = new NdefUriRecord();
            record.setPayload([]);
            
            expect(record.getRawUri()).toBeNull();
        });

        test('should return null for null payload in getRawUri', () => {
            const record = new NdefUriRecord();
            record.setPayload(null);
            
            expect(record.getRawUri()).toBeNull();
        });

        test('should handle raw URI with binary data', () => {
            const record = new NdefUriRecord();
            const binaryData = new Uint8Array([0x01, 0x02, 0x03, 0xFF, 0xFE]);
            
            record.setRawUri(binaryData);
            
            const retrievedRawUri = record.getRawUri();
            expect(Array.from(retrievedRawUri)).toEqual(Array.from(binaryData));
        });
    });

    describe('Record type identification', () => {
        test('should identify URI record type correctly', () => {
            const uriRecord = new NdefUriRecord("http://example.com");
            
            expect(NdefUriRecord.isRecordType(uriRecord)).toBe(true);
        });

        test('should not identify non-URI record as URI type', () => {
            const textRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x54], [0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F]);
            
            expect(NdefUriRecord.isRecordType(textRecord)).toBe(false);
        });

        test('should not identify empty record as URI type', () => {
            const emptyRecord = new NdefRecord();
            
            expect(NdefUriRecord.isRecordType(emptyRecord)).toBe(false);
        });

        test('should not identify MIME record as URI type', () => {
            const mimeRecord = new NdefRecord(TypeNameFormatType.Mime, [0x74, 0x65, 0x78, 0x74, 0x2F, 0x70, 0x6C, 0x61, 0x69, 0x6E]);
            
            expect(NdefUriRecord.isRecordType(mimeRecord)).toBe(false);
        });
    });

    describe('URI format compliance', () => {
        test('should handle standard web URLs', () => {
            const urls = [
                "http://www.google.com",
                "https://www.github.com",
                "http://stackoverflow.com",
                "https://developer.mozilla.org"
            ];
            
            urls.forEach(url => {
                const record = new NdefUriRecord(url);
                expect(record.getUri()).toBe(url);
            });
        });

        test('should handle telephone URIs', () => {
            const phoneNumbers = [
                "tel:+1-555-123-4567",
                "tel:555-123-4567",
                "tel:+44-20-7946-0958"
            ];
            
            phoneNumbers.forEach(tel => {
                const record = new NdefUriRecord(tel);
                expect(record.getUri()).toBe(tel);
            });
        });

        test('should handle email URIs', () => {
            const emails = [
                "mailto:user@example.com",
                "mailto:test.email+tag@domain.co.uk",
                "mailto:support@company.org"
            ];
            
            emails.forEach(email => {
                const record = new NdefUriRecord(email);
                expect(record.getUri()).toBe(email);
            });
        });

        test('should handle FTP URIs', () => {
            const ftpUris = [
                "ftp://ftp.example.com/file.txt",
                "ftps://secure.ftp.com/data/",
                "sftp://server.com/uploads/"
            ];
            
            ftpUris.forEach(ftp => {
                const record = new NdefUriRecord(ftp);
                expect(record.getUri()).toBe(ftp);
            });
        });

        test('should handle URN schemes', () => {
            const urns = [
                "urn:isbn:0451450523",
                "urn:uuid:6e8bc430-9c3a-11d9-9669-0800200c9a66",
                "urn:nfc:wkt:T"
            ];
            
            urns.forEach(urn => {
                const record = new NdefUriRecord(urn);
                expect(record.getUri()).toBe(urn);
            });
        });
    });

    describe('Edge cases and error conditions', () => {
        test('should handle null URI parameter', () => {
            const record = new NdefUriRecord(null);
            
            expect(record.getUri()).toBe("");
        });

        test('should handle undefined URI parameter', () => {
            const record = new NdefUriRecord(undefined);
            
            expect(record.getUri()).toBe("");
        });

        test('should handle URI modification after creation', () => {
            const record = new NdefUriRecord("http://example.com");
            
            expect(record.getUri()).toBe("http://example.com");
            
            record.setUri("https://newsite.com");
            expect(record.getUri()).toBe("https://newsite.com");
        });

        test('should handle payload with invalid abbreviation code', () => {
            const record = new NdefUriRecord();
            // Set payload with invalid abbreviation code (> 35)
            const payload = new Uint8Array([99, ...new TextEncoder().encode("example.com")]);
            record.setPayload(payload);
            
            // Should fall back to no abbreviation (code 0)
            expect(record.getUri()).toBe("example.com");
        });

        test('should handle empty payload', () => {
            const record = new NdefUriRecord();
            record.setPayload([]);
            
            expect(record.getUri()).toBe("");
        });

        test('should handle single byte payload (only abbreviation code)', () => {
            const record = new NdefUriRecord();
            record.setPayload(new Uint8Array([3])); // http:// abbreviation with no URI part
            
            expect(record.getUri()).toBe("http://");
        });

        test('should preserve case sensitivity in URIs', () => {
            const uri = "HTTP://Example.COM/Path";
            const record = new NdefUriRecord(uri);
            
            // Should not match abbreviation due to case
            const payload = record.getPayload();
            expect(payload[0]).toBe(0); // no abbreviation
            expect(record.getUri()).toBe(uri);
        });

        test('should handle URI with only scheme', () => {
            const uri = "https://";
            const record = new NdefUriRecord(uri);
            
            expect(record.getUri()).toBe(uri);
        });
    });

    describe('Static properties and constants', () => {
        test('should have correct URI_TYPE constant', () => {
            expect(Array.from(NdefUriRecord.URI_TYPE)).toEqual([0x55]); // 'U'
        });

        test('should use URI_TYPE in record creation', () => {
            const record = new NdefUriRecord("http://example.com");
            
            expect(Array.from(record.getType())).toEqual(Array.from(NdefUriRecord.URI_TYPE));
        });
    });

    describe('Integration with NdefRecord base class', () => {
        test('should inherit NdefRecord functionality', () => {
            const record = new NdefUriRecord("http://example.com");
            
            // Should have NdefRecord methods
            expect(typeof record.getTypeNameFormat).toBe('function');
            expect(typeof record.getType).toBe('function');
            expect(typeof record.getPayload).toBe('function');
            expect(typeof record.getId).toBe('function');
            expect(typeof record.setId).toBe('function');
        });

        test('should validate as proper NdefRecord', () => {
            const record = new NdefUriRecord("http://example.com");
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should allow setting ID field', () => {
            const record = new NdefUriRecord("http://example.com");
            const id = [0x01, 0x02, 0x03];
            
            record.setId(id);
            expect(Array.from(record.getId())).toEqual(id);
        });
    });
});