import { NdefTextRecord, TextEncodingType } from '../../src/submodule/NdefTextRecord.js';
import { NdefRecord, TypeNameFormatType } from '../../src/submodule/NdefRecord.js';

describe('NdefTextRecord', () => {
    describe('Text record creation', () => {
        test('should create empty text record with no parameters', () => {
            const record = new NdefTextRecord();
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(record.getType())).toEqual([0x54]); // 'T'
            expect(record.getText()).toBe("");
            expect(record.getLanguageCode()).toBe("en");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });

        test('should create text record with text only', () => {
            const text = "Hello World";
            const record = new NdefTextRecord(text);
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(record.getType())).toEqual([0x54]);
            expect(record.getText()).toBe(text);
            expect(record.getLanguageCode()).toBe("en");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });

        test('should create text record with text and language code', () => {
            const text = "Bonjour le monde";
            const languageCode = "fr";
            const record = new NdefTextRecord(text, languageCode);
            
            expect(record.getText()).toBe(text);
            expect(record.getLanguageCode()).toBe(languageCode);
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });

        test('should create text record with text, language, and encoding', () => {
            const text = "Hello World";
            const languageCode = "en";
            const encoding = TextEncodingType.Utf16;
            const record = new NdefTextRecord(text, languageCode, encoding);
            
            expect(record.getLanguageCode()).toBe(languageCode);
            expect(record.getTextEncoding()).toBe(encoding);
            // KNOWN BUG: UTF-16 encoding is broken in the implementation
            // This test documents the expected behavior, not the current buggy behavior
            expect(record.getText()).toBe(text);
        });

        test('should handle null text parameter', () => {
            const record = new NdefTextRecord(null);
            
            expect(record.getText()).toBe("");
            expect(record.getLanguageCode()).toBe("en");
        });

        test('should handle undefined text parameter', () => {
            const record = new NdefTextRecord(undefined);
            
            expect(record.getText()).toBe("");
            expect(record.getLanguageCode()).toBe("en");
        });
    });

    describe('Language code handling', () => {
        test('should handle standard language codes', () => {
            const languageCodes = ["en", "fr", "de", "es", "it", "ja", "zh", "ko"];
            
            languageCodes.forEach(lang => {
                const record = new NdefTextRecord("Test", lang);
                expect(record.getLanguageCode()).toBe(lang);
            });
        });

        test('should handle extended language codes', () => {
            const extendedCodes = ["en-US", "fr-CA", "zh-CN", "pt-BR"];
            
            extendedCodes.forEach(lang => {
                const record = new NdefTextRecord("Test", lang);
                expect(record.getLanguageCode()).toBe(lang);
            });
        });

        test('should handle empty language code', () => {
            const record = new NdefTextRecord("Test", "");
            expect(record.getLanguageCode()).toBe("");
        });

        test('should handle long language codes', () => {
            const longLang = "x-very-long-custom-language-code";
            const record = new NdefTextRecord("Test", longLang);
            expect(record.getLanguageCode()).toBe(longLang);
        });

        test('should default to "en" when no language code provided', () => {
            const record = new NdefTextRecord("Test");
            expect(record.getLanguageCode()).toBe("en");
        });
    });

    describe('UTF-8 encoding and decoding', () => {
        test('should handle basic ASCII text', () => {
            const text = "Hello World 123!";
            const record = new NdefTextRecord(text, "en", TextEncodingType.Utf8);
            
            expect(record.getText()).toBe(text);
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });

        test('should handle UTF-8 encoded text with special characters', () => {
            const text = "Café, naïve, résumé";
            const record = new NdefTextRecord(text, "fr", TextEncodingType.Utf8);
            
            expect(record.getText()).toBe(text);
            expect(record.getLanguageCode()).toBe("fr");
        });

        test('should handle UTF-8 encoded text with emojis', () => {
            const text = "Hello 👋 World 🌍!";
            const record = new NdefTextRecord(text, "en", TextEncodingType.Utf8);
            
            expect(record.getText()).toBe(text);
        });

        test('should handle UTF-8 encoded text with various scripts', () => {
            const texts = [
                "こんにちは世界", // Japanese
                "你好世界", // Chinese
                "Привет мир", // Russian
                "مرحبا بالعالم", // Arabic
                "שלום עולם" // Hebrew
            ];
            
            texts.forEach(text => {
                const record = new NdefTextRecord(text, "multi", TextEncodingType.Utf8);
                expect(record.getText()).toBe(text);
            });
        });

        test('should handle empty text with UTF-8', () => {
            const record = new NdefTextRecord("", "en", TextEncodingType.Utf8);
            
            expect(record.getText()).toBe("");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });

        test('should handle very long UTF-8 text', () => {
            const longText = "A".repeat(1000) + " 测试 " + "B".repeat(1000);
            const record = new NdefTextRecord(longText, "en", TextEncodingType.Utf8);
            
            expect(record.getText()).toBe(longText);
        });
    });

    describe('UTF-16 encoding and decoding', () => {
        test('should handle basic text with UTF-16 encoding', () => {
            const text = "Hello World";
            const record = new NdefTextRecord(text, "en", TextEncodingType.Utf16);
            
            // KNOWN BUG: UTF-16 implementation is broken - these tests will fail
            expect(record.getText()).toBe(text);
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf16);
        });

        test('should handle UTF-16 encoded text with special characters', () => {
            const text = "Café, naïve, résumé";
            const record = new NdefTextRecord(text, "fr", TextEncodingType.Utf16);
            
            // KNOWN BUG: UTF-16 implementation is broken - these tests will fail
            expect(record.getText()).toBe(text);
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf16);
        });

        test('should handle UTF-16 encoded text with emojis', () => {
            const text = "Hello 👋 World 🌍!";
            const record = new NdefTextRecord(text, "en", TextEncodingType.Utf16);
            
            // KNOWN BUG: UTF-16 implementation is broken - these tests will fail
            expect(record.getText()).toBe(text);
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf16);
        });

        test('should handle empty text with UTF-16', () => {
            const record = new NdefTextRecord("", "en", TextEncodingType.Utf16);
            
            expect(record.getText()).toBe("");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf16);
        });
    });

    describe('Text record format compliance', () => {
        test('should create proper payload structure for UTF-8', () => {
            const text = "Hello";
            const languageCode = "en";
            const record = new NdefTextRecord(text, languageCode, TextEncodingType.Utf8);
            
            const payload = record.getPayload();
            
            // First byte: status (language code length + encoding flag)
            expect(payload[0]).toBe(languageCode.length); // UTF-8, so no 0x80 flag
            
            // Language code bytes
            const langBytes = new TextEncoder().encode(languageCode);
            for (let i = 0; i < langBytes.length; i++) {
                expect(payload[1 + i]).toBe(langBytes[i]);
            }
            
            // Text bytes
            const textBytes = new TextEncoder().encode(text);
            for (let i = 0; i < textBytes.length; i++) {
                expect(payload[1 + langBytes.length + i]).toBe(textBytes[i]);
            }
        });

        test('should create proper payload structure for UTF-16', () => {
            const text = "Hello";
            const languageCode = "en";
            const record = new NdefTextRecord(text, languageCode, TextEncodingType.Utf16);
            
            const payload = record.getPayload();
            
            // First byte: status (language code length + UTF-16 flag)
            expect(payload[0]).toBe(languageCode.length | 0x80); // UTF-16 flag set
            
            // Language code should still be UTF-8 encoded
            const langBytes = new TextEncoder().encode(languageCode);
            for (let i = 0; i < langBytes.length; i++) {
                expect(payload[1 + i]).toBe(langBytes[i]);
            }
        });

        test('should handle maximum language code length (63 characters)', () => {
            const maxLangCode = "x".repeat(63);
            const record = new NdefTextRecord("Test", maxLangCode);
            
            expect(record.getLanguageCode()).toBe(maxLangCode);
            
            const payload = record.getPayload();
            expect(payload[0] & 0x3F).toBe(63); // Language code length mask
        });
    });

    describe('Record type identification', () => {
        test('should identify text record type correctly', () => {
            const textRecord = new NdefTextRecord("Hello World");
            
            expect(NdefTextRecord.isRecordType(textRecord)).toBe(true);
        });

        test('should not identify non-text record as text type', () => {
            const uriRecord = new NdefRecord(TypeNameFormatType.NfcRtd, [0x55], [0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65]);
            
            expect(NdefTextRecord.isRecordType(uriRecord)).toBe(false);
        });

        test('should not identify empty record as text type', () => {
            const emptyRecord = new NdefRecord();
            
            expect(NdefTextRecord.isRecordType(emptyRecord)).toBe(false);
        });

        test('should not identify MIME record as text type', () => {
            const mimeRecord = new NdefRecord(TypeNameFormatType.Mime, [0x74, 0x65, 0x78, 0x74, 0x2F, 0x70, 0x6C, 0x61, 0x69, 0x6E]);
            
            expect(NdefTextRecord.isRecordType(mimeRecord)).toBe(false);
        });
    });

    describe('Text modification after creation', () => {
        test('should allow modifying text after creation', () => {
            const record = new NdefTextRecord("Original Text");
            
            expect(record.getText()).toBe("Original Text");
            
            record.setText("Modified Text");
            expect(record.getText()).toBe("Modified Text");
        });

        test('should allow modifying language code after creation', () => {
            const record = new NdefTextRecord("Hello", "en");
            
            expect(record.getLanguageCode()).toBe("en");
            
            record.setText("Bonjour", "fr");
            expect(record.getText()).toBe("Bonjour");
            expect(record.getLanguageCode()).toBe("fr");
        });

        test('should allow modifying encoding after creation', () => {
            const record = new NdefTextRecord("Hello", "en", TextEncodingType.Utf8);
            
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
            
            record.setText("Hello", "en", TextEncodingType.Utf16);
            // KNOWN BUG: UTF-16 implementation is broken - this test will fail
            expect(record.getText()).toBe("Hello");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf16);
        });

        test('should preserve defaults when not specified in setText', () => {
            const record = new NdefTextRecord();
            
            record.setText("New Text");
            expect(record.getText()).toBe("New Text");
            expect(record.getLanguageCode()).toBe("en");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });
    });

    describe('Edge cases and error conditions', () => {
        test('should handle empty payload gracefully', () => {
            const record = new NdefTextRecord();
            record.setPayload([]);
            
            expect(record.getText()).toBe("");
            expect(record.getLanguageCode()).toBe("en");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });

        test('should handle null payload gracefully', () => {
            const record = new NdefTextRecord();
            record.setPayload(null);
            
            expect(record.getText()).toBe("");
            expect(record.getLanguageCode()).toBe("en");
            expect(record.getTextEncoding()).toBe(TextEncodingType.Utf8);
        });

        test('should handle malformed payload with only status byte', () => {
            const record = new NdefTextRecord();
            record.setPayload(new Uint8Array([0x02])); // Language code length 2, but no actual data
            
            // Should handle gracefully without throwing
            expect(() => record.getText()).not.toThrow();
            expect(() => record.getLanguageCode()).not.toThrow();
        });

        test('should handle payload with language code but no text', () => {
            const record = new NdefTextRecord();
            // Status byte (2) + language code "en" but no text
            const payload = new Uint8Array([0x02, 0x65, 0x6E]);
            record.setPayload(payload);
            
            expect(record.getLanguageCode()).toBe("en");
            expect(record.getText()).toBe("");
        });

        test('should handle very large language code length in status byte', () => {
            const record = new NdefTextRecord();
            // Status byte with language code length 63 (maximum)
            const payload = new Uint8Array([0x3F, ...new Array(63).fill(0x61), ...new TextEncoder().encode("Test")]);
            record.setPayload(payload);
            
            expect(() => record.getLanguageCode()).not.toThrow();
            expect(() => record.getText()).not.toThrow();
        });

        test('should handle text with null characters', () => {
            const textWithNull = "Hello\x00World";
            const record = new NdefTextRecord(textWithNull);
            
            expect(record.getText()).toBe(textWithNull);
        });

        test('should handle text with control characters', () => {
            const textWithControls = "Hello\n\t\rWorld";
            const record = new NdefTextRecord(textWithControls);
            
            expect(record.getText()).toBe(textWithControls);
        });
    });

    describe('TextEncodingType constants', () => {
        test('should have correct encoding constant values', () => {
            expect(TextEncodingType.Utf8).toBe(0);
            expect(TextEncodingType.Utf16).toBe(1);
        });

        test('should handle both encoding types', () => {
            const text = "Test";
            
            const utf8Record = new NdefTextRecord(text, "en", TextEncodingType.Utf8);
            const utf16Record = new NdefTextRecord(text, "en", TextEncodingType.Utf16);
            
            expect(utf8Record.getTextEncoding()).toBe(TextEncodingType.Utf8);
            expect(utf16Record.getTextEncoding()).toBe(TextEncodingType.Utf16);
            
            expect(utf8Record.getText()).toBe(text);
            // KNOWN BUG: UTF-16 implementation is broken - this test will fail
            expect(utf16Record.getText()).toBe(text);
        });
    });

    describe('Static properties and constants', () => {
        test('should have correct TEXT_TYPE constant', () => {
            expect(Array.from(NdefTextRecord.TEXT_TYPE)).toEqual([0x54]); // 'T'
        });

        test('should use TEXT_TYPE in record creation', () => {
            const record = new NdefTextRecord("Hello World");
            
            expect(Array.from(record.getType())).toEqual(Array.from(NdefTextRecord.TEXT_TYPE));
        });
    });

    describe('Integration with NdefRecord base class', () => {
        test('should inherit NdefRecord functionality', () => {
            const record = new NdefTextRecord("Hello World");
            
            // Should have NdefRecord methods
            expect(typeof record.getTypeNameFormat).toBe('function');
            expect(typeof record.getType).toBe('function');
            expect(typeof record.getPayload).toBe('function');
            expect(typeof record.getId).toBe('function');
            expect(typeof record.setId).toBe('function');
        });

        test('should validate as proper NdefRecord', () => {
            const record = new NdefTextRecord("Hello World");
            
            expect(() => record.checkIfValid()).not.toThrow();
        });

        test('should allow setting ID field', () => {
            const record = new NdefTextRecord("Hello World");
            const id = [0x01, 0x02, 0x03];
            
            record.setId(id);
            expect(Array.from(record.getId())).toEqual(id);
        });

        test('should maintain proper TNF and type', () => {
            const record = new NdefTextRecord("Hello World");
            
            expect(record.getTypeNameFormat()).toBe(TypeNameFormatType.NfcRtd);
            expect(Array.from(record.getType())).toEqual([0x54]);
        });
    });
});