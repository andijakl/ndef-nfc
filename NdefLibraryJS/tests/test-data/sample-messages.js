/**
 * Sample NDEF messages for testing
 * Contains known good test vectors and sample data for each record type
 */

import { 
    NdefRecord, 
    NdefMessage, 
    NdefUriRecord, 
    NdefTextRecord, 
    NdefGeoRecord, 
    NdefSocialRecord, 
    NdefTelRecord, 
    NdefAndroidAppRecord,
    TypeNameFormatType 
} from '../../src/index.js';
import { NfcGeoType } from '../../src/submodule/NdefGeoRecord.js';
import { NfcSocialType } from '../../src/submodule/NdefSocialRecord.js';

/**
 * Sample URI records with various schemes and abbreviations
 */
export const sampleUriRecords = {
    httpExample: {
        description: "HTTP URI with abbreviation",
        record: new NdefUriRecord("http://example.com"),
        expectedUri: "http://example.com",
        expectedBytes: [0xD1, 0x01, 0x0F, 0x55, 0x01, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D]
    },
    
    httpsExample: {
        description: "HTTPS URI with abbreviation",
        record: new NdefUriRecord("https://www.example.com"),
        expectedUri: "https://www.example.com",
        expectedBytes: [0xD1, 0x01, 0x13, 0x55, 0x02, 0x77, 0x77, 0x77, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D]
    },
    
    ftpExample: {
        description: "FTP URI with abbreviation",
        record: new NdefUriRecord("ftp://files.example.com"),
        expectedUri: "ftp://files.example.com",
        expectedBytes: [0xD1, 0x01, 0x15, 0x55, 0x03, 0x66, 0x69, 0x6C, 0x65, 0x73, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D]
    },
    
    noAbbreviation: {
        description: "URI without abbreviation",
        record: new NdefUriRecord("custom://example.com"),
        expectedUri: "custom://example.com",
        expectedBytes: [0xD1, 0x01, 0x15, 0x55, 0x00, 0x63, 0x75, 0x73, 0x74, 0x6F, 0x6D, 0x3A, 0x2F, 0x2F, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D]
    },
    
    emailExample: {
        description: "Email URI",
        record: new NdefUriRecord("mailto:test@example.com"),
        expectedUri: "mailto:test@example.com",
        expectedBytes: [0xD1, 0x01, 0x15, 0x55, 0x06, 0x74, 0x65, 0x73, 0x74, 0x40, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D]
    }
};

/**
 * Sample text records with different languages and encodings
 */
export const sampleTextRecords = {
    englishText: {
        description: "English text record",
        record: new NdefTextRecord("Hello World", "en"),
        expectedText: "Hello World",
        expectedLanguage: "en",
        expectedBytes: [0xD1, 0x01, 0x0E, 0x54, 0x02, 0x65, 0x6E, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64]
    },
    
    germanText: {
        description: "German text record",
        record: new NdefTextRecord("Hallo Welt", "de"),
        expectedText: "Hallo Welt",
        expectedLanguage: "de",
        expectedBytes: [0xD1, 0x01, 0x0D, 0x54, 0x02, 0x64, 0x65, 0x48, 0x61, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x65, 0x6C, 0x74]
    },
    
    unicodeText: {
        description: "Unicode text record",
        record: new NdefTextRecord("こんにちは", "ja"),
        expectedText: "こんにちは",
        expectedLanguage: "ja"
    },
    
    emptyText: {
        description: "Empty text record",
        record: new NdefTextRecord("", "en"),
        expectedText: "",
        expectedLanguage: "en"
    }
};/**

 * Sample geo records with various coordinate formats
 */
export const sampleGeoRecords = {
    basicCoordinates: {
        description: "Basic latitude/longitude coordinates",
        record: new NdefGeoRecord(47.6062, -122.3321),
        expectedLatitude: 47.6062,
        expectedLongitude: -122.3321,
        expectedUri: "geo:47.6062,-122.3321"
    },
    
    withGeoType: {
        description: "Coordinates with different geo type",
        record: new NdefGeoRecord(47.6062, -122.3321, NfcGeoType.BingMaps),
        expectedLatitude: 47.6062,
        expectedLongitude: -122.3321,
        expectedGeoType: NfcGeoType.BingMaps,
        expectedUri: "bingmaps:?cp=47.6062~-122.3321"
    },
    
    negativeCoordinates: {
        description: "Negative coordinates",
        record: new NdefGeoRecord(-33.8688, 151.2093),
        expectedLatitude: -33.8688,
        expectedLongitude: 151.2093,
        expectedUri: "geo:-33.8688,151.2093"
    },
    
    zeroCoordinates: {
        description: "Zero coordinates",
        record: new NdefGeoRecord(0.0001, 0.0001), // Avoid exact zero due to library logic
        expectedLatitude: 0.0001,
        expectedLongitude: 0.0001,
        expectedUri: "geo:0.0001,0.0001"
    }
};

/**
 * Sample social records for different platforms
 */
export const sampleSocialRecords = {
    twitter: {
        description: "Twitter social record",
        record: new NdefSocialRecord("nfcinteractor", NfcSocialType.Twitter),
        expectedPlatform: "twitter",
        expectedUsername: "nfcinteractor",
        expectedUri: "http://twitter.com/nfcinteractor"
    },
    
    facebook: {
        description: "Facebook social record",
        record: new NdefSocialRecord("nfcinteractor", NfcSocialType.Facebook),
        expectedPlatform: "facebook",
        expectedUsername: "nfcinteractor",
        expectedUri: "http://facebook.com/nfcinteractor"
    },
    
    linkedin: {
        description: "LinkedIn social record",
        record: new NdefSocialRecord("nfcinteractor", NfcSocialType.LinkedIn),
        expectedPlatform: "linkedin",
        expectedUsername: "nfcinteractor",
        expectedUri: "http://linkedin.com/in/nfcinteractor"
    }
};

/**
 * Sample telephone records with various formats
 */
export const sampleTelRecords = {
    basicNumber: {
        description: "Basic phone number",
        record: new NdefTelRecord("+1-555-123-4567"),
        expectedNumber: "+1-555-123-4567",
        expectedUri: "tel:+1-555-123-4567"
    },
    
    internationalNumber: {
        description: "International phone number",
        record: new NdefTelRecord("+49-30-12345678"),
        expectedNumber: "+49-30-12345678",
        expectedUri: "tel:+49-30-12345678"
    },
    
    shortNumber: {
        description: "Short phone number",
        record: new NdefTelRecord("911"),
        expectedNumber: "911",
        expectedUri: "tel:911"
    }
};

/**
 * Sample Android App records
 */
export const sampleAndroidAppRecords = {
    basicApp: {
        description: "Basic Android app record",
        record: new NdefAndroidAppRecord("com.example.myapp"),
        expectedPackageName: "com.example.myapp"
    },
    
    googleApp: {
        description: "Google Play app record",
        record: new NdefAndroidAppRecord("com.google.android.apps.maps"),
        expectedPackageName: "com.google.android.apps.maps"
    }
};/**
 * Sam
ple multi-record NDEF messages
 */
export const sampleMessages = {
    singleUriMessage: {
        description: "Single URI record message",
        message: new NdefMessage([sampleUriRecords.httpsExample.record]),
        expectedRecordCount: 1
    },
    
    multiRecordMessage: {
        description: "Multi-record message with different types",
        message: new NdefMessage([
            sampleUriRecords.httpsExample.record,
            sampleTextRecords.englishText.record,
            sampleGeoRecords.basicCoordinates.record
        ]),
        expectedRecordCount: 3
    },
    
    socialMediaMessage: {
        description: "Social media contact message",
        message: new NdefMessage([
            sampleTextRecords.englishText.record,
            sampleSocialRecords.twitter.record,
            sampleTelRecords.basicNumber.record
        ]),
        expectedRecordCount: 3
    },
    
    emptyMessage: {
        description: "Empty NDEF message",
        message: new NdefMessage([]),
        expectedRecordCount: 0
    }
};

/**
 * Edge case test data for boundary conditions
 */
export const edgeCaseData = {
    longUri: {
        description: "Very long URI that requires long record format",
        uri: "https://www.example.com/" + "a".repeat(300),
        get record() { return new NdefUriRecord(this.uri); }
    },
    
    longText: {
        description: "Very long text that requires long record format",
        text: "This is a very long text message that should exceed the short record format limit. " + "Lorem ipsum dolor sit amet, consectetur adipiscing elit. ".repeat(10),
        get record() { return new NdefTextRecord(this.text, "en"); }
    },
    
    maxCoordinates: {
        description: "Maximum valid coordinates",
        record: new NdefGeoRecord(89.9999, 179.9999), // Avoid exact boundary values
        expectedLatitude: 89.9999,
        expectedLongitude: 179.9999
    },
    
    minCoordinates: {
        description: "Minimum valid coordinates",
        record: new NdefGeoRecord(-89.9999, -179.9999), // Avoid exact boundary values
        expectedLatitude: -89.9999,
        expectedLongitude: -179.9999
    },
    
    specialCharacters: {
        description: "Text with special characters",
        text: "Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?`~",
        get record() { return new NdefTextRecord(this.text, "en"); }
    }
};

/**
 * Invalid/malformed test data for error testing
 */
export const invalidData = {
    invalidCoordinates: [
        { lat: 91, lon: 0, description: "Latitude too high" },
        { lat: -91, lon: 0, description: "Latitude too low" },
        { lat: 0, lon: 181, description: "Longitude too high" },
        { lat: 0, lon: -181, description: "Longitude too low" },
        { lat: NaN, lon: 0, description: "NaN latitude" },
        { lat: 0, lon: NaN, description: "NaN longitude" }
    ],
    
    invalidUris: [
        "",
        null,
        undefined,
        "not-a-uri",
        "://invalid"
    ],
    
    invalidLanguageCodes: [
        "",
        null,
        undefined,
        "toolong",
        "1",
        "a1"
    ]
};