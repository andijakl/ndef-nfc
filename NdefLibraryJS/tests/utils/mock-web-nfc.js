/**
 * Mock Web NFC API for testing demo application functionality
 * Provides a complete mock implementation of the Web NFC API
 */

/**
 * Mock NDEFReader class that simulates Web NFC functionality
 */
export class MockNDEFReader {
    constructor() {
        this.scanning = false;
        this.onreading = null;
        this.onreadingerror = null;
        this.mockTags = [];
        this.scanOptions = null;
        this.writeHistory = [];
        this.shouldThrowError = false;
        this.errorToThrow = null;
    }

    /**
     * Mock scan method
     * @param {Object} options - Scan options
     */
    async scan(options = {}) {
        if (this.shouldThrowError) {
            throw this.errorToThrow || new DOMException('Mock error', 'NotAllowedError');
        }

        this.scanning = true;
        this.scanOptions = options;

        // Simulate async scanning
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve();
                this._triggerMockReading();
            }, 100);
        });
    }

    /**
     * Mock write method
     * @param {Object} message - NDEF message to write
     * @param {Object} options - Write options
     */
    async write(message, options = {}) {
        if (this.shouldThrowError) {
            throw this.errorToThrow || new DOMException('Mock write error', 'NetworkError');
        }

        // Record the write operation
        this.writeHistory.push({
            message: this._cloneMessage(message),
            options: { ...options },
            timestamp: Date.now()
        });

        // Simulate async write operation
        return new Promise((resolve) => {
            setTimeout(resolve, 50);
        });
    }

    /**
     * Mock stop method
     */
    async stop() {
        this.scanning = false;
        this.scanOptions = null;
    }

    /**
     * Add a mock NFC tag that will be "read" during scanning
     * @param {Object} ndefMessage - Mock NDEF message
     * @param {string} serialNumber - Mock serial number
     */
    addMockTag(ndefMessage, serialNumber = 'mock-tag-001') {
        this.mockTags.push({
            message: this._cloneMessage(ndefMessage),
            serialNumber,
            id: new Uint8Array([0x01, 0x02, 0x03, 0x04])
        });
    }

    /**
     * Clear all mock tags
     */
    clearMockTags() {
        this.mockTags = [];
    }

    /**
     * Set error conditions for testing
     * @param {Error|DOMException} error - Error to throw
     */
    setMockError(error) {
        this.shouldThrowError = true;
        this.errorToThrow = error;
    }

    /**
     * Clear error conditions
     */
    clearMockError() {
        this.shouldThrowError = false;
        this.errorToThrow = null;
    }

    /**
     * Get write history for testing
     */
    getWriteHistory() {
        return [...this.writeHistory];
    }

    /**
     * Clear write history
     */
    clearWriteHistory() {
        this.writeHistory = [];
    }

    /**
     * Trigger a mock reading event
     * @private
     */
    _triggerMockReading() {
        if (!this.scanning || !this.onreading || this.mockTags.length === 0) {
            return;
        }

        // Simulate reading the first mock tag
        const mockTag = this.mockTags[0];
        const mockEvent = new MockNDEFReadingEvent('reading', {
            message: mockTag.message,
            serialNumber: mockTag.serialNumber
        });

        setTimeout(() => {
            if (this.onreading) {
                this.onreading(mockEvent);
            }
        }, 10);
    }

    /**
     * Clone an NDEF message for mock purposes
     * @private
     */
    _cloneMessage(message) {
        if (!message || !message.records) {
            return { records: [] };
        }

        return {
            records: message.records.map(record => ({
                recordType: record.recordType,
                mediaType: record.mediaType,
                id: record.id,
                data: record.data ? new Uint8Array(record.data) : undefined,
                encoding: record.encoding,
                lang: record.lang
            }))
        };
    }
}

/**
 * Mock NDEFReadingEvent class
 */
export class MockNDEFReadingEvent extends Event {
    constructor(type, eventInitDict = {}) {
        super(type);
        this.message = eventInitDict.message || { records: [] };
        this.serialNumber = eventInitDict.serialNumber || 'mock-serial';
    }
}

/**
 * Mock NDEF record for Web NFC API compatibility
 */
export class MockNDEFRecord {
    constructor(options = {}) {
        this.recordType = options.recordType || 'empty';
        this.mediaType = options.mediaType;
        this.id = options.id;
        this.data = options.data;
        this.encoding = options.encoding;
        this.lang = options.lang;
    }

    toJSON() {
        return {
            recordType: this.recordType,
            mediaType: this.mediaType,
            id: this.id,
            data: this.data,
            encoding: this.encoding,
            lang: this.lang
        };
    }
}

/**
 * Utility to install Web NFC API mock in global scope
 */
export function installWebNFCMock() {
    if (typeof global !== 'undefined') {
        // Node.js environment
        global.NDEFReader = MockNDEFReader;
        global.NDEFReadingEvent = MockNDEFReadingEvent;
        global.NDEFRecord = MockNDEFRecord;
    } else if (typeof window !== 'undefined') {
        // Browser environment
        window.NDEFReader = MockNDEFReader;
        window.NDEFReadingEvent = MockNDEFReadingEvent;
        window.NDEFRecord = MockNDEFRecord;
    }
}

/**
 * Utility to uninstall Web NFC API mock
 */
export function uninstallWebNFCMock() {
    if (typeof global !== 'undefined') {
        delete global.NDEFReader;
        delete global.NDEFReadingEvent;
        delete global.NDEFRecord;
    } else if (typeof window !== 'undefined') {
        delete window.NDEFReader;
        delete window.NDEFReadingEvent;
        delete window.NDEFRecord;
    }
}

/**
 * Create a mock NDEF message from library records
 */
export function createMockNDEFMessage(libraryRecords) {
    const webNfcRecords = libraryRecords.map(record => {
        const recordType = getWebNFCRecordType(record);
        const mockRecord = new MockNDEFRecord({
            recordType: recordType,
            data: record.getPayload ? record.getPayload() : new Uint8Array(),
            id: record.getId ? record.getId() : undefined
        });

        // Add specific properties based on record type
        if (recordType === 'url' && record.getUri) {
            mockRecord.data = new TextEncoder().encode(record.getUri());
        } else if (recordType === 'text' && record.getText) {
            mockRecord.data = new TextEncoder().encode(record.getText());
            mockRecord.encoding = 'utf-8';
            mockRecord.lang = record.getLanguageCode ? record.getLanguageCode() : 'en';
        }

        return mockRecord;
    });

    return { records: webNfcRecords };
}

/**
 * Convert library record type to Web NFC record type
 * @private
 */
function getWebNFCRecordType(record) {
    if (!record.getType) return 'unknown';
    
    const type = Array.from(record.getType());
    
    // Check for well-known types
    if (type.length === 1) {
        switch (type[0]) {
            case 0x55: return 'url';      // 'U'
            case 0x54: return 'text';     // 'T'
            default: return 'unknown';
        }
    }
    
    return 'unknown';
}

/**
 * Browser compatibility checker for Web NFC
 */
export class WebNFCCompatibilityChecker {
    static isSupported() {
        return typeof NDEFReader !== 'undefined';
    }

    static getUnsupportedReason() {
        if (typeof NDEFReader === 'undefined') {
            return 'Web NFC API not available';
        }
        return null;
    }

    static async checkPermissions() {
        if (!navigator.permissions) {
            return 'unknown';
        }

        try {
            const permission = await navigator.permissions.query({ name: 'nfc' });
            return permission.state;
        } catch (error) {
            return 'unknown';
        }
    }

    static isSecureContext() {
        return typeof isSecureContext !== 'undefined' ? isSecureContext : false;
    }

    static getCompatibilityReport() {
        return {
            supported: this.isSupported(),
            unsupportedReason: this.getUnsupportedReason(),
            secureContext: this.isSecureContext(),
            userAgent: typeof navigator !== 'undefined' ? navigator.userAgent : 'unknown'
        };
    }
}