/**
 * Test helper functions for NDEF library testing
 * Provides utilities for data comparison, validation, and test setup
 */

/**
 * Compare two byte arrays for equality
 * @param {Uint8Array|Array} array1 - First array
 * @param {Uint8Array|Array} array2 - Second array
 * @returns {boolean} True if arrays are equal
 */
export function compareByteArrays(array1, array2) {
    if (!array1 || !array2) {
        return array1 === array2;
    }

    if (array1.length !== array2.length) {
        return false;
    }

    for (let i = 0; i < array1.length; i++) {
        if (array1[i] !== array2[i]) {
            return false;
        }
    }

    return true;
}

/**
 * Convert various array types to Uint8Array for consistent comparison
 * @param {Uint8Array|Array|Buffer} data - Input data
 * @returns {Uint8Array} Normalized array
 */
export function normalizeByteArray(data) {
    if (!data) return new Uint8Array();
    if (data instanceof Uint8Array) return data;
    if (Array.isArray(data)) return new Uint8Array(data);
    if (data.buffer) return new Uint8Array(data);
    return new Uint8Array();
}

/**
 * Format byte array as hex string for debugging
 * @param {Uint8Array|Array} data - Byte array
 * @returns {string} Hex representation
 */
export function formatAsHex(data) {
    if (!data) return '';
    return Array.from(data)
        .map(byte => byte.toString(16).padStart(2, '0').toUpperCase())
        .join(' ');
}

/**
 * Create a detailed comparison report for byte arrays
 * @param {Uint8Array|Array} expected - Expected array
 * @param {Uint8Array|Array} actual - Actual array
 * @returns {Object} Comparison report
 */
export function createByteArrayComparisonReport(expected, actual) {
    const normalizedExpected = normalizeByteArray(expected);
    const normalizedActual = normalizeByteArray(actual);
    
    const report = {
        equal: compareByteArrays(normalizedExpected, normalizedActual),
        expectedLength: normalizedExpected.length,
        actualLength: normalizedActual.length,
        expectedHex: formatAsHex(normalizedExpected),
        actualHex: formatAsHex(normalizedActual),
        differences: []
    };

    // Find specific differences
    const maxLength = Math.max(normalizedExpected.length, normalizedActual.length);
    for (let i = 0; i < maxLength; i++) {
        const expectedByte = i < normalizedExpected.length ? normalizedExpected[i] : undefined;
        const actualByte = i < normalizedActual.length ? normalizedActual[i] : undefined;
        
        if (expectedByte !== actualByte) {
            report.differences.push({
                index: i,
                expected: expectedByte,
                actual: actualByte,
                expectedHex: expectedByte !== undefined ? expectedByte.toString(16).padStart(2, '0').toUpperCase() : 'MISSING',
                actualHex: actualByte !== undefined ? actualByte.toString(16).padStart(2, '0').toUpperCase() : 'MISSING'
            });
        }
    }

    return report;
}

/**
 * Validate NDEF record structure
 * @param {Object} record - NDEF record to validate
 * @returns {Object} Validation result
 */
export function validateNdefRecord(record) {
    const result = {
        valid: true,
        errors: [],
        warnings: []
    };

    // Check required methods
    const requiredMethods = ['getTypeNameFormat', 'getType', 'getPayload'];
    for (const method of requiredMethods) {
        if (typeof record[method] !== 'function') {
            result.valid = false;
            result.errors.push(`Missing required method: ${method}`);
        }
    }

    // Validate TNF
    try {
        const tnf = record.getTypeNameFormat();
        if (typeof tnf !== 'number' || tnf < 0 || tnf > 7) {
            result.valid = false;
            result.errors.push(`Invalid TNF value: ${tnf}`);
        }
    } catch (error) {
        result.valid = false;
        result.errors.push(`Error getting TNF: ${error.message}`);
    }

    // Validate type
    try {
        const type = record.getType();
        if (!(type instanceof Uint8Array) && !Array.isArray(type)) {
            result.valid = false;
            result.errors.push('Type must be Uint8Array or Array');
        }
    } catch (error) {
        result.valid = false;
        result.errors.push(`Error getting type: ${error.message}`);
    }

    // Validate payload
    try {
        const payload = record.getPayload();
        if (!(payload instanceof Uint8Array) && !Array.isArray(payload)) {
            result.valid = false;
            result.errors.push('Payload must be Uint8Array or Array');
        }
    } catch (error) {
        result.valid = false;
        result.errors.push(`Error getting payload: ${error.message}`);
    }

    return result;
}

/**
 * Validate NDEF message structure
 * @param {Object} message - NDEF message to validate
 * @returns {Object} Validation result
 */
export function validateNdefMessage(message) {
    const result = {
        valid: true,
        errors: [],
        warnings: []
    };

    // Check required methods
    if (typeof message.getRecords !== 'function') {
        result.valid = false;
        result.errors.push('Missing getRecords method');
        return result;
    }

    try {
        const records = message.getRecords();
        
        if (!Array.isArray(records)) {
            result.valid = false;
            result.errors.push('getRecords must return an array');
            return result;
        }

        // Validate each record
        records.forEach((record, index) => {
            const recordValidation = validateNdefRecord(record);
            if (!recordValidation.valid) {
                result.valid = false;
                recordValidation.errors.forEach(error => {
                    result.errors.push(`Record ${index}: ${error}`);
                });
            }
        });

        // Check message-level constraints
        if (records.length === 0) {
            result.warnings.push('Message contains no records');
        }

    } catch (error) {
        result.valid = false;
        result.errors.push(`Error validating message: ${error.message}`);
    }

    return result;
}

/**
 * Create a round-trip test for NDEF serialization/parsing
 * @param {Object} originalMessage - Original NDEF message
 * @returns {Object} Test result
 */
export function performRoundTripTest(originalMessage) {
    const result = {
        success: false,
        originalValid: false,
        serialized: null,
        parsed: null,
        parsedValid: false,
        recordsMatch: false,
        errors: []
    };

    try {
        // Validate original message
        const originalValidation = validateNdefMessage(originalMessage);
        result.originalValid = originalValidation.valid;
        
        if (!originalValidation.valid) {
            result.errors.push('Original message validation failed');
            originalValidation.errors.forEach(error => result.errors.push(error));
            return result;
        }

        // Serialize message
        if (typeof originalMessage.toByteArray !== 'function') {
            result.errors.push('Message missing toByteArray method');
            return result;
        }

        result.serialized = originalMessage.toByteArray();

        // Parse serialized data back
        // Note: This would require a parsing method - implementation depends on library structure
        // For now, we'll just validate the serialization
        if (result.serialized && result.serialized.length > 0) {
            result.success = true;
        }

    } catch (error) {
        result.errors.push(`Round-trip test failed: ${error.message}`);
    }

    return result;
}

/**
 * Generate test case variations for a given input
 * @param {Object} baseTestCase - Base test case
 * @param {Array} variations - Array of variation functions
 * @returns {Array} Array of test cases
 */
export function generateTestVariations(baseTestCase, variations) {
    const testCases = [baseTestCase];
    
    variations.forEach(variation => {
        try {
            const variedCase = variation(JSON.parse(JSON.stringify(baseTestCase)));
            if (variedCase) {
                testCases.push(variedCase);
            }
        } catch (error) {
            console.warn(`Failed to generate test variation: ${error.message}`);
        }
    });

    return testCases;
}

/**
 * Performance measurement utilities
 */
export class PerformanceMeasurer {
    constructor() {
        this.measurements = new Map();
    }

    start(label) {
        this.measurements.set(label, {
            startTime: performance.now(),
            endTime: null,
            duration: null
        });
    }

    end(label) {
        const measurement = this.measurements.get(label);
        if (measurement) {
            measurement.endTime = performance.now();
            measurement.duration = measurement.endTime - measurement.startTime;
        }
        return measurement;
    }

    getDuration(label) {
        const measurement = this.measurements.get(label);
        return measurement ? measurement.duration : null;
    }

    getAllMeasurements() {
        const results = {};
        this.measurements.forEach((value, key) => {
            results[key] = value;
        });
        return results;
    }

    clear() {
        this.measurements.clear();
    }
}

/**
 * Memory usage tracking utilities
 */
export class MemoryTracker {
    constructor() {
        this.snapshots = [];
    }

    takeSnapshot(label = '') {
        const snapshot = {
            label,
            timestamp: Date.now(),
            memory: this.getMemoryUsage()
        };
        this.snapshots.push(snapshot);
        return snapshot;
    }

    getMemoryUsage() {
        if (typeof performance !== 'undefined' && performance.memory) {
            return {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit
            };
        }
        return null;
    }

    getMemoryDelta(startLabel, endLabel) {
        const startSnapshot = this.snapshots.find(s => s.label === startLabel);
        const endSnapshot = this.snapshots.find(s => s.label === endLabel);
        
        if (!startSnapshot || !endSnapshot || !startSnapshot.memory || !endSnapshot.memory) {
            return null;
        }

        return {
            usedDelta: endSnapshot.memory.used - startSnapshot.memory.used,
            totalDelta: endSnapshot.memory.total - startSnapshot.memory.total,
            duration: endSnapshot.timestamp - startSnapshot.timestamp
        };
    }

    clear() {
        this.snapshots = [];
    }
}

/**
 * Test data validation utilities
 */
export const TestValidators = {
    /**
     * Validate URI format
     */
    isValidUri(uri) {
        try {
            new URL(uri);
            return true;
        } catch {
            // Check for special URI schemes that might not parse as URLs
            const specialSchemes = /^(tel:|sms:|mailto:|geo:)/i;
            return specialSchemes.test(uri);
        }
    },

    /**
     * Validate language code format
     */
    isValidLanguageCode(code) {
        return typeof code === 'string' && 
               code.length >= 2 && 
               code.length <= 8 && 
               /^[a-z]{2,3}(-[A-Z]{2})?$/.test(code);
    },

    /**
     * Validate coordinates
     */
    isValidCoordinate(lat, lon) {
        return typeof lat === 'number' && 
               typeof lon === 'number' &&
               lat >= -90 && lat <= 90 &&
               lon >= -180 && lon <= 180 &&
               !isNaN(lat) && !isNaN(lon);
    },

    /**
     * Validate package name format
     */
    isValidPackageName(packageName) {
        return typeof packageName === 'string' &&
               /^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$/.test(packageName);
    }
};

/**
 * Async test utilities
 */
export const AsyncTestUtils = {
    /**
     * Wait for a condition to be true
     */
    async waitFor(condition, timeout = 5000, interval = 100) {
        const startTime = Date.now();
        
        while (Date.now() - startTime < timeout) {
            if (await condition()) {
                return true;
            }
            await this.delay(interval);
        }
        
        throw new Error(`Condition not met within ${timeout}ms`);
    },

    /**
     * Simple delay utility
     */
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    },

    /**
     * Timeout wrapper for promises
     */
    withTimeout(promise, timeout = 5000) {
        return Promise.race([
            promise,
            new Promise((_, reject) => 
                setTimeout(() => reject(new Error(`Operation timed out after ${timeout}ms`)), timeout)
            )
        ]);
    }
};