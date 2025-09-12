/**
 * Test data generators for creating dynamic test cases
 */

import { 
    NdefRecord, 
    NdefMessage, 
    NdefUriRecord, 
    NdefTextRecord, 
    NdefGeoRecord, 
    NdefSocialRecord, 
    NdefTelRecord, 
    NdefAndroidAppRecord 
} from '../../src/index.js';

/**
 * Generate random URI records for testing
 */
export class UriRecordGenerator {
    static schemes = ['http', 'https', 'ftp', 'mailto', 'tel', 'sms'];
    static domains = ['example.com', 'test.org', 'sample.net', 'demo.co.uk'];
    static paths = ['', '/path', '/path/to/resource', '/api/v1/data'];
    
    static generateRandom() {
        const scheme = this.schemes[Math.floor(Math.random() * this.schemes.length)];
        const domain = this.domains[Math.floor(Math.random() * this.domains.length)];
        const path = this.paths[Math.floor(Math.random() * this.paths.length)];
        
        let uri;
        if (scheme === 'mailto') {
            uri = `${scheme}:test@${domain}`;
        } else if (scheme === 'tel' || scheme === 'sms') {
            uri = `${scheme}:+1-555-${Math.floor(Math.random() * 900 + 100)}-${Math.floor(Math.random() * 9000 + 1000)}`;
        } else {
            uri = `${scheme}://${domain}${path}`;
        }
        
        return new NdefUriRecord(uri);
    }
    
    static generateWithLength(minLength, maxLength) {
        const baseUri = 'https://example.com/';
        const targetLength = Math.floor(Math.random() * (maxLength - minLength)) + minLength;
        const padding = 'a'.repeat(Math.max(0, targetLength - baseUri.length));
        return new NdefUriRecord(baseUri + padding);
    }
    
    static generateBatch(count) {
        return Array.from({ length: count }, () => this.generateRandom());
    }
}

/**
 * Generate random text records for testing
 */
export class TextRecordGenerator {
    static languages = ['en', 'de', 'fr', 'es', 'ja', 'zh', 'ko', 'ru'];
    static sampleTexts = [
        'Hello World',
        'Test Message',
        'Sample Text',
        'Lorem ipsum dolor sit amet',
        'The quick brown fox jumps over the lazy dog',
        'Testing 123',
        'Special chars: !@#$%^&*()',
        'Unicode: 你好世界 🌍'
    ];
    
    static generateRandom() {
        const language = this.languages[Math.floor(Math.random() * this.languages.length)];
        const text = this.sampleTexts[Math.floor(Math.random() * this.sampleTexts.length)];
        return new NdefTextRecord(text, language);
    }
    
    static generateWithLength(minLength, maxLength) {
        const targetLength = Math.floor(Math.random() * (maxLength - minLength)) + minLength;
        const baseText = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. ';
        const text = baseText.repeat(Math.ceil(targetLength / baseText.length)).substring(0, targetLength);
        return new NdefTextRecord(text, 'en');
    }
    
    static generateMultiLanguage() {
        const text = 'Hello World';
        return this.languages.map(lang => new NdefTextRecord(text, lang));
    }
    
    static generateBatch(count) {
        return Array.from({ length: count }, () => this.generateRandom());
    }
}

/**
 * Generate random geo records for testing
 */
export class GeoRecordGenerator {
    static generateRandom() {
        const lat = (Math.random() - 0.5) * 180; // -90 to 90
        const lon = (Math.random() - 0.5) * 360; // -180 to 180
        const includeAltitude = Math.random() > 0.5;
        
        if (includeAltitude) {
            const alt = Math.random() * 10000 - 1000; // -1000 to 9000 meters
            return new NdefGeoRecord(lat, lon, alt);
        } else {
            return new NdefGeoRecord(lat, lon);
        }
    }
    
    static generateBoundaryValues() {
        return [
            new NdefGeoRecord(90, 180),    // Max values
            new NdefGeoRecord(-90, -180),  // Min values
            new NdefGeoRecord(0, 0),       // Zero values
            new NdefGeoRecord(45.5, -122.3), // Typical values
        ];
    }
    
    static generateBatch(count) {
        return Array.from({ length: count }, () => this.generateRandom());
    }
}

/**
 * Generate random social records for testing
 */
export class SocialRecordGenerator {
    static platforms = ['twitter', 'facebook', 'linkedin', 'instagram', 'youtube'];
    static usernames = ['testuser', 'sampleaccount', 'demouser', 'nfctest', 'example123'];
    
    static generateRandom() {
        const platform = this.platforms[Math.floor(Math.random() * this.platforms.length)];
        const username = this.usernames[Math.floor(Math.random() * this.usernames.length)];
        return new NdefSocialRecord(platform, username);
    }
    
    static generateForAllPlatforms(username = 'testuser') {
        return this.platforms.map(platform => new NdefSocialRecord(platform, username));
    }
    
    static generateBatch(count) {
        return Array.from({ length: count }, () => this.generateRandom());
    }
}

/**
 * Generate random telephone records for testing
 */
export class TelRecordGenerator {
    static countryCodes = ['+1', '+44', '+49', '+33', '+81', '+86'];
    
    static generateRandom() {
        const countryCode = this.countryCodes[Math.floor(Math.random() * this.countryCodes.length)];
        const areaCode = Math.floor(Math.random() * 900 + 100);
        const number1 = Math.floor(Math.random() * 900 + 100);
        const number2 = Math.floor(Math.random() * 9000 + 1000);
        
        return new NdefTelRecord(`${countryCode}-${areaCode}-${number1}-${number2}`);
    }
    
    static generateFormats() {
        return [
            new NdefTelRecord('+1-555-123-4567'),
            new NdefTelRecord('555-123-4567'),
            new NdefTelRecord('(555) 123-4567'),
            new NdefTelRecord('555.123.4567'),
            new NdefTelRecord('15551234567'),
            new NdefTelRecord('911'), // Emergency number
        ];
    }
    
    static generateBatch(count) {
        return Array.from({ length: count }, () => this.generateRandom());
    }
}

/**
 * Generate random Android App records for testing
 */
export class AndroidAppRecordGenerator {
    static packagePrefixes = ['com.example', 'com.test', 'org.sample', 'net.demo'];
    static appNames = ['myapp', 'testapp', 'sampleapp', 'demoapp', 'nfcapp'];
    
    static generateRandom() {
        const prefix = this.packagePrefixes[Math.floor(Math.random() * this.packagePrefixes.length)];
        const appName = this.appNames[Math.floor(Math.random() * this.appNames.length)];
        return new NdefAndroidAppRecord(`${prefix}.${appName}`);
    }
    
    static generateCommonApps() {
        return [
            new NdefAndroidAppRecord('com.google.android.apps.maps'),
            new NdefAndroidAppRecord('com.android.chrome'),
            new NdefAndroidAppRecord('com.whatsapp'),
            new NdefAndroidAppRecord('com.facebook.katana'),
            new NdefAndroidAppRecord('com.twitter.android'),
        ];
    }
    
    static generateBatch(count) {
        return Array.from({ length: count }, () => this.generateRandom());
    }
}

/**
 * Generate complex NDEF messages for testing
 */
export class MessageGenerator {
    static generateRandomMessage(minRecords = 1, maxRecords = 5) {
        const recordCount = Math.floor(Math.random() * (maxRecords - minRecords + 1)) + minRecords;
        const records = [];
        
        const generators = [
            () => UriRecordGenerator.generateRandom(),
            () => TextRecordGenerator.generateRandom(),
            () => GeoRecordGenerator.generateRandom(),
            () => SocialRecordGenerator.generateRandom(),
            () => TelRecordGenerator.generateRandom(),
            () => AndroidAppRecordGenerator.generateRandom()
        ];
        
        for (let i = 0; i < recordCount; i++) {
            const generator = generators[Math.floor(Math.random() * generators.length)];
            records.push(generator());
        }
        
        return new NdefMessage(records);
    }
    
    static generateBusinessCard() {
        return new NdefMessage([
            new NdefTextRecord('John Doe', 'en'),
            new NdefTextRecord('Software Engineer', 'en'),
            new NdefUriRecord('mailto:john.doe@example.com'),
            new NdefTelRecord('+1-555-123-4567'),
            new NdefUriRecord('https://www.johndoe.com'),
            new NdefSocialRecord('linkedin', 'johndoe')
        ]);
    }
    
    static generateEventInfo() {
        return new NdefMessage([
            new NdefTextRecord('Tech Conference 2024', 'en'),
            new NdefGeoRecord(37.7749, -122.4194), // San Francisco
            new NdefUriRecord('https://techconf2024.com'),
            new NdefTextRecord('March 15-17, 2024', 'en')
        ]);
    }
    
    static generateProductInfo() {
        return new NdefMessage([
            new NdefTextRecord('Smart Widget Pro', 'en'),
            new NdefUriRecord('https://example.com/products/smart-widget-pro'),
            new NdefAndroidAppRecord('com.example.smartwidget'),
            new NdefTextRecord('Model: SWP-2024', 'en')
        ]);
    }
    
    static generateBatch(count, messageType = 'random') {
        const generators = {
            random: () => this.generateRandomMessage(),
            businessCard: () => this.generateBusinessCard(),
            eventInfo: () => this.generateEventInfo(),
            productInfo: () => this.generateProductInfo()
        };
        
        const generator = generators[messageType] || generators.random;
        return Array.from({ length: count }, () => generator());
    }
}

/**
 * Generate edge case and stress test data
 */
export class EdgeCaseGenerator {
    static generateLargePayloads() {
        return {
            shortRecord: UriRecordGenerator.generateWithLength(1, 255),
            longRecord: UriRecordGenerator.generateWithLength(256, 1000),
            veryLongRecord: UriRecordGenerator.generateWithLength(1000, 8192)
        };
    }
    
    static generateBoundaryConditions() {
        return {
            emptyMessage: new NdefMessage([]),
            singleRecord: new NdefMessage([new NdefUriRecord('http://example.com')]),
            maxRecords: new NdefMessage(Array.from({ length: 100 }, () => UriRecordGenerator.generateRandom())),
            mixedRecordTypes: new NdefMessage([
                new NdefUriRecord('http://example.com'),
                new NdefTextRecord('Test', 'en'),
                new NdefGeoRecord(0, 0),
                new NdefSocialRecord('twitter', 'test'),
                new NdefTelRecord('+1-555-123-4567'),
                new NdefAndroidAppRecord('com.example.test')
            ])
        };
    }
    
    static generatePerformanceTestData() {
        return {
            manySmallRecords: Array.from({ length: 1000 }, () => new NdefUriRecord('http://example.com')),
            fewLargeRecords: Array.from({ length: 10 }, () => TextRecordGenerator.generateWithLength(5000, 10000)),
            mixedSizes: [
                ...Array.from({ length: 100 }, () => UriRecordGenerator.generateWithLength(10, 50)),
                ...Array.from({ length: 10 }, () => TextRecordGenerator.generateWithLength(1000, 5000)),
                ...Array.from({ length: 5 }, () => TextRecordGenerator.generateWithLength(10000, 20000))
            ]
        };
    }
}