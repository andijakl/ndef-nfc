/**
 * Test utilities index
 * Exports all testing utilities for easy importing
 */

// Mock Web NFC API
export {
    MockNDEFReader,
    MockNDEFReadingEvent,
    MockNDEFRecord,
    installWebNFCMock,
    uninstallWebNFCMock,
    createMockNDEFMessage,
    WebNFCCompatibilityChecker
} from './mock-web-nfc.js';

// Test helpers
export {
    compareByteArrays,
    normalizeByteArray,
    formatAsHex,
    createByteArrayComparisonReport,
    validateNdefRecord,
    validateNdefMessage,
    performRoundTripTest,
    generateTestVariations,
    PerformanceMeasurer,
    MemoryTracker,
    TestValidators,
    AsyncTestUtils
} from './test-helpers.js';

// Browser compatibility
export {
    BrowserFeatureDetector,
    CrossBrowserTester,
    PolyfillManager,
    EnvironmentDetector
} from './browser-compatibility.js';

// Test data
export * from '../test-data/sample-messages.js';
export * from '../test-data/test-vectors.js';
export * from '../test-data/generators.js';

/**
 * Convenience function to set up complete test environment
 */
export async function setupTestEnvironment(options = {}) {
    const {
        installWebNFCMock: shouldInstallMock = true,
        loadPolyfills = true,
        enablePerformanceTracking = false,
        enableMemoryTracking = false
    } = options;

    const environment = {
        webNFCMock: null,
        performanceMeasurer: null,
        memoryTracker: null,
        polyfillManager: null,
        compatibility: null
    };

    // Install Web NFC mock if requested
    if (shouldInstallMock) {
        const { installWebNFCMock, MockNDEFReader } = await import('./mock-web-nfc.js');
        installWebNFCMock();
        environment.webNFCMock = new MockNDEFReader();
    }

    // Load polyfills if requested
    if (loadPolyfills) {
        const { PolyfillManager } = await import('./browser-compatibility.js');
        environment.polyfillManager = new PolyfillManager();
        await environment.polyfillManager.loadBasicPolyfills();
    }

    // Set up performance tracking
    if (enablePerformanceTracking) {
        const { PerformanceMeasurer } = await import('./test-helpers.js');
        environment.performanceMeasurer = new PerformanceMeasurer();
    }

    // Set up memory tracking
    if (enableMemoryTracking) {
        const { MemoryTracker } = await import('./test-helpers.js');
        environment.memoryTracker = new MemoryTracker();
    }

    // Get compatibility information
    const { BrowserFeatureDetector } = await import('./browser-compatibility.js');
    environment.compatibility = BrowserFeatureDetector.checkNdefLibraryCompatibility();

    return environment;
}

/**
 * Convenience function to tear down test environment
 */
export async function teardownTestEnvironment(environment) {
    // Uninstall Web NFC mock
    if (environment.webNFCMock) {
        const { uninstallWebNFCMock } = await import('./mock-web-nfc.js');
        uninstallWebNFCMock();
    }

    // Clear performance measurements
    if (environment.performanceMeasurer) {
        environment.performanceMeasurer.clear();
    }

    // Clear memory tracking
    if (environment.memoryTracker) {
        environment.memoryTracker.clear();
    }
}

/**
 * Quick test runner for basic functionality
 */
export async function runQuickCompatibilityTest() {
    const { BrowserFeatureDetector, CrossBrowserTester } = await import('./browser-compatibility.js');
    const { UriRecordGenerator, TextRecordGenerator } = await import('../test-data/generators.js');
    
    const tester = new CrossBrowserTester();
    
    // Test basic record creation
    await tester.runTest('uri-record-creation', () => {
        const record = UriRecordGenerator.generateRandom();
        return record !== null && typeof record.getUri === 'function';
    });
    
    await tester.runTest('text-record-creation', () => {
        const record = TextRecordGenerator.generateRandom();
        return record !== null && typeof record.getText === 'function';
    });
    
    // Test feature detection
    await tester.runTest('feature-detection', () => {
        const features = BrowserFeatureDetector.detectJavaScriptFeatures();
        return features !== null && typeof features === 'object';
    });
    
    return tester.generateCompatibilityReport();
}

/**
 * Export commonly used test configurations
 */
export const TestConfigurations = {
    minimal: {
        installWebNFCMock: false,
        loadPolyfills: false,
        enablePerformanceTracking: false,
        enableMemoryTracking: false
    },
    
    standard: {
        installWebNFCMock: true,
        loadPolyfills: true,
        enablePerformanceTracking: false,
        enableMemoryTracking: false
    },
    
    comprehensive: {
        installWebNFCMock: true,
        loadPolyfills: true,
        enablePerformanceTracking: true,
        enableMemoryTracking: true
    }
};