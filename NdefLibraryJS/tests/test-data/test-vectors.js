/**
 * Known good NDEF test vectors for validation
 * These are pre-validated NDEF byte arrays that should parse correctly
 */

/**
 * URI record test vectors with expected parsing results
 */
export const uriTestVectors = [
    {
        description: "HTTP URI with abbreviation",
        ndefBytes: [0xD1, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedRecords: [{
            type: "URI",
            uri: "http://example.com"
        }]
    },
    
    {
        description: "HTTPS URI with www abbreviation",
        ndefBytes: [0xD1, 0x01, 0x13, 0x55, 0x02, 0x77, 0x77, 0x77, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedRecords: [{
            type: "URI",
            uri: "https://www.example.com"
        }]
    },
    
    {
        description: "FTP URI with abbreviation",
        ndefBytes: [0xD1, 0x01, 0x15, 0x55, 0x03, 0x66, 0x69, 0x6C, 0x65, 0x73, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedRecords: [{
            type: "URI",
            uri: "ftp://files.example.com"
        }]
    },
    
    {
        description: "Email URI with mailto abbreviation",
        ndefBytes: [0xD1, 0x01, 0x15, 0x55, 0x06, 0x74, 0x65, 0x73, 0x74, 0x40, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedRecords: [{
            type: "URI",
            uri: "mailto:test@example.com"
        }]
    },
    
    {
        description: "URI without abbreviation",
        ndefBytes: [0xD1, 0x01, 0x15, 0x55, 0x00, 0x63, 0x75, 0x73, 0x74, 0x6F, 0x6D, 0x3A, 0x2F, 0x2F, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedRecords: [{
            type: "URI",
            uri: "custom://example.com"
        }]
    }
];

/**
 * Text record test vectors
 */
export const textTestVectors = [
    {
        description: "English text record",
        ndefBytes: [0xD1, 0x01, 0x0E, 0x54, 0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64],
        expectedRecords: [{
            type: "TEXT",
            text: "Hello World",
            language: "en"
        }]
    },
    
    {
        description: "German text record",
        ndefBytes: [0xD1, 0x01, 0x0D, 0x54, 0x02, 0x64, 0x65, 0x48, 0x61, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x65, 0x6C, 0x74],
        expectedRecords: [{
            type: "TEXT",
            text: "Hallo Welt",
            language: "de"
        }]
    },
    
    {
        description: "Empty text record",
        ndefBytes: [0xD1, 0x01, 0x02, 0x54, 0x02, 0x65, 0x6E],
        expectedRecords: [{
            type: "TEXT",
            text: "",
            language: "en"
        }]
    }
];

/**
 * Multi-record message test vectors
 */
export const multiRecordTestVectors = [
    {
        description: "Two URI records",
        ndefBytes: [
            // First record (MB=1, ME=0)
            0x91, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D,
            // Second record (MB=0, ME=1)
            0x51, 0x01, 0x13, 0x55, 0x02, 0x77, 0x77, 0x77, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D
        ],
        expectedRecords: [
            {
                type: "URI",
                uri: "http://example.com"
            },
            {
                type: "URI",
                uri: "https://www.example.com"
            }
        ]
    },
    
    {
        description: "URI and Text record combination",
        ndefBytes: [
            // URI record (MB=1, ME=0)
            0x91, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D,
            // Text record (MB=0, ME=1)
            0x51, 0x01, 0x0E, 0x54, 0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64
        ],
        expectedRecords: [
            {
                type: "URI",
                uri: "http://example.com"
            },
            {
                type: "TEXT",
                text: "Hello World",
                language: "en"
            }
        ]
    }
];/*
*
 * Long record format test vectors (payload > 255 bytes)
 */
export const longRecordTestVectors = [
    {
        description: "Long URI record",
        // This would be a URI record with a very long payload requiring long record format
        ndefBytes: [
            0xC1, 0x01, 0x00, 0x00, 0x01, 0x2C, 0x55, 0x01, // Header with long record format
            // Followed by 300 bytes of URI data
            ...Array.from("https://www.example.com/" + "a".repeat(270), c => c.charCodeAt(0))
        ],
        expectedRecords: [{
            type: "URI",
            uri: "https://www.example.com/" + "a".repeat(270)
        }]
    }
];

/**
 * Record with ID test vectors
 */
export const recordWithIdTestVectors = [
    {
        description: "URI record with ID",
        ndefBytes: [
            0xD9, 0x01, 0x0F, 0x55, 0x04, // Header with IL flag set, ID length = 4
            0x74, 0x65, 0x73, 0x74, // ID: "test"
            0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D // Payload
        ],
        expectedRecords: [{
            type: "URI",
            uri: "http://example.com",
            id: "test"
        }]
    }
];

/**
 * Malformed NDEF data for error testing
 */
export const malformedTestVectors = [
    {
        description: "Empty byte array",
        ndefBytes: [],
        expectedError: "Empty NDEF data"
    },
    
    {
        description: "Incomplete header",
        ndefBytes: [0xD1, 0x01],
        expectedError: "Incomplete NDEF header"
    },
    
    {
        description: "Payload length mismatch",
        ndefBytes: [0xD1, 0x01, 0x10, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D], // Claims 16 bytes but only has 5
        expectedError: "Payload length mismatch"
    },
    
    {
        description: "Invalid TNF value",
        ndefBytes: [0xD7, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedError: "Invalid TNF value"
    },
    
    {
        description: "Missing MB flag in first record",
        ndefBytes: [0x51, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedError: "First record must have MB flag"
    },
    
    {
        description: "Missing ME flag in last record",
        ndefBytes: [0x91, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D],
        expectedError: "Last record must have ME flag"
    }
];

/**
 * Real-world NDEF examples from common NFC tags
 */
export const realWorldExamples = [
    {
        description: "Business card with contact info",
        ndefBytes: [
            // Text record with name
            0x91, 0x01, 0x0C, 0x54, 0x02, 0x65, 0x6E, 0x4A, 0x6F, 0x68, 0x6E, 0x20, 0x44, 0x6F, 0x65,
            // URI record with website
            0x11, 0x01, 0x15, 0x55, 0x02, 0x77, 0x77, 0x77, 0x2E, 0x6A, 0x6F, 0x68, 0x6E, 0x64, 0x6F, 0x65, 0x2E, 0x63, 0x6F, 0x6D,
            // Tel record with phone
            0x51, 0x01, 0x0F, 0x55, 0x05, 0x2B, 0x31, 0x2D, 0x35, 0x35, 0x35, 0x2D, 0x31, 0x32, 0x33, 0x2D, 0x34, 0x35, 0x36, 0x37
        ],
        expectedRecords: [
            { type: "TEXT", text: "John Doe", language: "en" },
            { type: "URI", uri: "https://www.johndoe.com" },
            { type: "URI", uri: "tel:+1-555-123-4567" }
        ]
    },
    
    {
        description: "WiFi configuration (simplified)",
        ndefBytes: [
            // Text record with SSID
            0x91, 0x01, 0x0B, 0x54, 0x02, 0x65, 0x6E, 0x4D, 0x79, 0x57, 0x69, 0x46, 0x69,
            // Text record with password hint
            0x51, 0x01, 0x0F, 0x54, 0x02, 0x65, 0x6E, 0x50, 0x61, 0x73, 0x73, 0x77, 0x6F, 0x72, 0x64, 0x3A, 0x20, 0x2A, 0x2A, 0x2A
        ],
        expectedRecords: [
            { type: "TEXT", text: "MyWiFi", language: "en" },
            { type: "TEXT", text: "Password: ***", language: "en" }
        ]
    }
];

/**
 * Performance test data for stress testing
 */
export const performanceTestData = {
    largeMessage: {
        description: "Message with many records",
        recordCount: 100,
        generateMessage: () => {
            const records = [];
            for (let i = 0; i < 100; i++) {
                records.push([
                    0x11, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D
                ]);
            }
            // Set MB flag on first record and ME flag on last record
            records[0][0] = 0x91;
            records[records.length - 1][0] = 0x51;
            return records.flat();
        }
    },
    
    veryLongRecord: {
        description: "Single record with very large payload",
        payloadSize: 8192,
        generateRecord: () => {
            const payload = new Array(8192).fill(0x41); // 8KB of 'A' characters
            return [
                0xC1, 0x01, 0x00, 0x00, 0x20, 0x01, 0x55, 0x00, // Long record header
                ...payload
            ];
        }
    }
};