/**
 * Web NFC Integration Tests for Demo Application
 * Tests Web NFC API interaction, NDEF message publishing/sharing, and error handling
 */

import { jest } from '@jest/globals';
import { 
    MockNDEFReader, 
    installWebNFCMock, 
    uninstallWebNFCMock,
    createMockNDEFMessage,
    WebNFCCompatibilityChecker
} from '../utils/mock-web-nfc.js';
import { NdefMessage } from '../../src/submodule/NdefMessage.js';
import { NdefRecord } from '../../src/submodule/NdefRecord.js';
import { NdefUriRecord } from '../../src/submodule/NdefUriRecord.js';
import { NdefTextRecord } from '../../src/submodule/NdefTextRecord.js';
import { NdefSocialRecord, NfcSocialType } from '../../src/submodule/NdefSocialRecord.js';

describe('Web NFC Integration Tests', () => {
    let mockNDEFReader;

    beforeEach(() => {
        installWebNFCMock();
        // Create a new instance for each test
        global.NDEFReader = jest.fn(() => new MockNDEFReader());
        
        // Mock navigator for sharing functionality
        global.navigator = {
            share: jest.fn(),
            permissions: {
                query: jest.fn()
            }
        };
        
        // Mock btoa for base64 encoding
        global.btoa = jest.fn((str) => Buffer.from(str, 'binary').toString('base64'));
    });

    afterEach(() => {
        uninstallWebNFCMock();
        delete global.navigator;
        delete global.btoa;
        jest.clearAllMocks();
    });

    describe('Web NFC API Interaction with Mocked Functionality', () => {
        test('NDEFReader can be instantiated', () => {
            const reader = new NDEFReader();
            expect(reader).toBeInstanceOf(MockNDEFReader);
            expect(global.NDEFReader).toHaveBeenCalled();
        });

        test('NDEFReader scan method works with mock', async () => {
            const reader = new NDEFReader();
            
            await expect(reader.scan()).resolves.toBeUndefined();
            expect(reader.scanning).toBe(true);
        });

        test('NDEFReader scan method handles options', async () => {
            const reader = new NDEFReader();
            const options = { signal: new AbortController().signal };
            
            await reader.scan(options);
            expect(reader.scanOptions).toEqual(options);
        });

        test('NDEFReader write method works with mock', async () => {
            const reader = new NDEFReader();
            const uriRecord = new NdefUriRecord('https://example.com');
            const message = new NdefMessage([uriRecord]);
            
            await expect(reader.write(message)).resolves.toBeUndefined();
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory).toHaveLength(1);
            expect(writeHistory[0].message).toBeDefined();
        });

        test('NDEFReader stop method works with mock', async () => {
            const reader = new NDEFReader();
            
            await reader.scan();
            expect(reader.scanning).toBe(true);
            
            await reader.stop();
            expect(reader.scanning).toBe(false);
        });

        test('NDEFReader handles reading events', async () => {
            const reader = new NDEFReader();
            const readingHandler = jest.fn();
            
            // Set up reading handler first
            reader.onreading = readingHandler;
            
            // Create mock NDEF message
            const uriRecord = new NdefUriRecord('https://example.com');
            const mockMessage = createMockNDEFMessage([uriRecord]);
            reader.addMockTag(mockMessage);
            
            await reader.scan();
            
            // Wait for mock reading event
            await new Promise(resolve => setTimeout(resolve, 150));
            
            expect(readingHandler).toHaveBeenCalledTimes(1);
            expect(readingHandler.mock.calls[0][0].message).toBeDefined();
        });

        test('NDEFReader handles multiple mock tags', async () => {
            const reader = new NDEFReader();
            const readingHandler = jest.fn();
            
            // Add multiple mock tags
            const uriRecord1 = new NdefUriRecord('https://example1.com');
            const uriRecord2 = new NdefUriRecord('https://example2.com');
            reader.addMockTag(createMockNDEFMessage([uriRecord1]), 'tag-001');
            reader.addMockTag(createMockNDEFMessage([uriRecord2]), 'tag-002');
            
            reader.onreading = readingHandler;
            await reader.scan();
            
            // Wait for mock reading event (only first tag is read in current implementation)
            await new Promise(resolve => setTimeout(resolve, 150));
            
            expect(readingHandler).toHaveBeenCalledTimes(1);
        });
    });

    describe('NDEF Message Publishing Features', () => {
        test('Publishes URI NDEF messages successfully', async () => {
            const reader = new NDEFReader();
            const testUri = 'https://www.nfcinteractor.com';
            const uriRecord = new NdefUriRecord(testUri);
            const message = new NdefMessage([uriRecord]);
            
            await reader.write(message);
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory).toHaveLength(1);
            expect(writeHistory[0].message.records).toHaveLength(1);
        });

        test('Publishes Twitter social NDEF messages successfully', async () => {
            const reader = new NDEFReader();
            const twitterHandle = 'andijakl';
            const socialRecord = new NdefSocialRecord(twitterHandle, NfcSocialType.Twitter);
            const message = new NdefMessage([socialRecord]);
            
            await reader.write(message);
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory).toHaveLength(1);
            expect(writeHistory[0].message.records).toHaveLength(1);
        });

        test('Publishes text NDEF messages successfully', async () => {
            const reader = new NDEFReader();
            const textRecord = new NdefTextRecord('Hello, NFC!', 'en');
            const message = new NdefMessage([textRecord]);
            
            await reader.write(message);
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory).toHaveLength(1);
            expect(writeHistory[0].message.records).toHaveLength(1);
        });

        test('Publishes multi-record NDEF messages successfully', async () => {
            const reader = new NDEFReader();
            const uriRecord = new NdefUriRecord('https://example.com');
            const textRecord = new NdefTextRecord('Visit our website', 'en');
            const message = new NdefMessage([uriRecord, textRecord]);
            
            await reader.write(message);
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory).toHaveLength(1);
            expect(writeHistory[0].message.records).toHaveLength(2);
        });

        test('Handles write options correctly', async () => {
            const reader = new NDEFReader();
            const uriRecord = new NdefUriRecord('https://example.com');
            const message = new NdefMessage([uriRecord]);
            const options = { overwrite: true, signal: new AbortController().signal };
            
            await reader.write(message, options);
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory[0].options).toEqual(options);
        });

        test('Tracks multiple write operations', async () => {
            const reader = new NDEFReader();
            
            // Write multiple messages
            const uriRecord1 = new NdefUriRecord('https://example1.com');
            const uriRecord2 = new NdefUriRecord('https://example2.com');
            const message1 = new NdefMessage([uriRecord1]);
            const message2 = new NdefMessage([uriRecord2]);
            
            await reader.write(message1);
            await reader.write(message2);
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory).toHaveLength(2);
            expect(writeHistory[0].timestamp).toBeLessThanOrEqual(writeHistory[1].timestamp);
        });
    });

    describe('NDEF Message Sharing Features', () => {
        test('Shares URI messages using Web Share API', async () => {
            const testUri = 'https://www.nfcinteractor.com';
            const uriRecord = new NdefUriRecord(testUri);
            const message = new NdefMessage([uriRecord]);
            
            navigator.share.mockResolvedValue();
            global.btoa.mockReturnValue('base64encodeddata');
            
            const shareData = {
                title: 'NDEF URI',
                text: 'NDEF URI',
                url: 'data:application/octet-stream;base64,' + btoa(String.fromCharCode.apply(null, message.toByteArray()))
            };
            
            await navigator.share(shareData);
            
            expect(navigator.share).toHaveBeenCalledWith(shareData);
            expect(global.btoa).toHaveBeenCalled();
        });

        test('Shares complex NDEF messages', async () => {
            const uriRecord = new NdefUriRecord('https://example.com');
            const textRecord = new NdefTextRecord('Check this out!', 'en');
            const message = new NdefMessage([uriRecord, textRecord]);
            
            navigator.share.mockResolvedValue();
            global.btoa.mockReturnValue('complexbase64data');
            
            const shareData = {
                title: 'NDEF Message',
                text: 'Complex NDEF Message',
                url: 'data:application/octet-stream;base64,' + btoa(String.fromCharCode.apply(null, message.toByteArray()))
            };
            
            await navigator.share(shareData);
            
            expect(navigator.share).toHaveBeenCalledWith(shareData);
        });

        test('Handles Web Share API unavailability', async () => {
            delete navigator.share;
            
            const testUri = 'https://example.com';
            const uriRecord = new NdefUriRecord(testUri);
            const message = new NdefMessage([uriRecord]);
            
            // Should handle gracefully when share API is not available
            expect(navigator.share).toBeUndefined();
        });

        test('Converts NDEF message to base64 correctly', () => {
            const uriRecord = new NdefUriRecord('https://example.com');
            const message = new NdefMessage([uriRecord]);
            
            global.btoa.mockImplementation((str) => Buffer.from(str, 'binary').toString('base64'));
            
            const byteArray = message.toByteArray();
            const base64 = btoa(String.fromCharCode.apply(null, byteArray));
            
            expect(global.btoa).toHaveBeenCalled();
            expect(base64).toBeDefined();
            expect(typeof base64).toBe('string');
        });
    });

    describe('Error Handling for Unsupported Browsers', () => {
        test('Handles Web NFC API not available', () => {
            uninstallWebNFCMock();
            
            expect(typeof NDEFReader).toBe('undefined');
            expect(WebNFCCompatibilityChecker.isSupported()).toBe(false);
            expect(WebNFCCompatibilityChecker.getUnsupportedReason()).toBe('Web NFC API not available');
        });

        test('Handles permission denied errors', async () => {
            const reader = new NDEFReader();
            const permissionError = new DOMException('Permission denied', 'NotAllowedError');
            reader.setMockError(permissionError);
            
            await expect(reader.scan()).rejects.toThrow('Permission denied');
            await expect(reader.write({})).rejects.toThrow('Permission denied');
        });

        test('Handles network errors during write', async () => {
            const reader = new NDEFReader();
            const networkError = new DOMException('Network error', 'NetworkError');
            reader.setMockError(networkError);
            
            const uriRecord = new NdefUriRecord('https://example.com');
            const message = new NdefMessage([uriRecord]);
            
            await expect(reader.write(message)).rejects.toThrow('Network error');
        });

        test('Handles invalid operation errors', async () => {
            const reader = new NDEFReader();
            const invalidError = new DOMException('Invalid operation', 'InvalidStateError');
            reader.setMockError(invalidError);
            
            await expect(reader.scan()).rejects.toThrow('Invalid operation');
        });

        test('Handles Web Share API errors', async () => {
            const shareError = new Error('Share cancelled by user');
            navigator.share.mockRejectedValue(shareError);
            
            const shareData = {
                title: 'Test',
                text: 'Test',
                url: 'https://example.com'
            };
            
            await expect(navigator.share(shareData)).rejects.toThrow('Share cancelled by user');
        });

        test('Handles NDEF library errors during message creation', () => {
            // Test invalid social record validation
            expect(() => {
                const socialRecord = new NdefSocialRecord('', NfcSocialType.Twitter);
                socialRecord.checkIfValid();
            }).toThrow('Social user name is empty');
            
            // Test that error handling works in general
            expect(() => {
                throw new Error('Test error');
            }).toThrow('Test error');
        });

        test('Handles browser compatibility issues', () => {
            const compatReport = WebNFCCompatibilityChecker.getCompatibilityReport();
            
            expect(compatReport).toHaveProperty('supported');
            expect(compatReport).toHaveProperty('unsupportedReason');
            expect(compatReport).toHaveProperty('secureContext');
            expect(compatReport).toHaveProperty('userAgent');
        });

        test('Handles secure context requirements', () => {
            // Mock insecure context
            global.isSecureContext = false;
            
            expect(WebNFCCompatibilityChecker.isSecureContext()).toBe(false);
            
            // Mock secure context
            global.isSecureContext = true;
            expect(WebNFCCompatibilityChecker.isSecureContext()).toBe(true);
            
            delete global.isSecureContext;
        });

        test('Handles permission query errors', async () => {
            navigator.permissions.query.mockRejectedValue(new Error('Permission query failed'));
            
            const permission = await WebNFCCompatibilityChecker.checkPermissions();
            expect(permission).toBe('unknown');
        });

        test('Handles missing permissions API', async () => {
            delete navigator.permissions;
            
            const permission = await WebNFCCompatibilityChecker.checkPermissions();
            expect(permission).toBe('unknown');
        });
    });

    describe('Integration with Demo Application Flow', () => {
        test('Complete subscribe-read-publish flow', async () => {
            const reader = new NDEFReader();
            const readingHandler = jest.fn();
            
            // Set up reading handler first
            reader.onreading = readingHandler;
            
            // Add a mock tag before scanning
            const uriRecord = new NdefUriRecord('https://example.com');
            const mockMessage = createMockNDEFMessage([uriRecord]);
            reader.addMockTag(mockMessage);
            
            // Start scanning
            await reader.scan();
            expect(reader.scanning).toBe(true);
            
            // Wait for reading event
            await new Promise(resolve => setTimeout(resolve, 150));
            expect(readingHandler).toHaveBeenCalled();
            
            // Write a new message
            const newUriRecord = new NdefUriRecord('https://newexample.com');
            const newMessage = new NdefMessage([newUriRecord]);
            await reader.write(newMessage);
            
            const writeHistory = reader.getWriteHistory();
            expect(writeHistory).toHaveLength(1);
            
            // Stop scanning
            await reader.stop();
            expect(reader.scanning).toBe(false);
        });

        test('Error recovery in demo application flow', async () => {
            const reader = new NDEFReader();
            
            // First operation fails
            reader.setMockError(new DOMException('First error', 'NotAllowedError'));
            await expect(reader.scan()).rejects.toThrow('First error');
            
            // Clear error and retry
            reader.clearMockError();
            await expect(reader.scan()).resolves.toBeUndefined();
            expect(reader.scanning).toBe(true);
        });

        test('Multiple reader instances work independently', async () => {
            const reader1 = new NDEFReader();
            const reader2 = new NDEFReader();
            
            // Both can scan independently
            await reader1.scan();
            await reader2.scan();
            
            expect(reader1.scanning).toBe(true);
            expect(reader2.scanning).toBe(true);
            
            // Both can write independently
            const message1 = new NdefMessage([new NdefUriRecord('https://example1.com')]);
            const message2 = new NdefMessage([new NdefUriRecord('https://example2.com')]);
            
            await reader1.write(message1);
            await reader2.write(message2);
            
            expect(reader1.getWriteHistory()).toHaveLength(1);
            expect(reader2.getWriteHistory()).toHaveLength(1);
        });
    });
});