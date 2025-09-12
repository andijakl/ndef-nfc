/**
 * Browser compatibility testing utilities
 * Provides tools for testing NDEF library across different browser environments
 */

/**
 * Browser feature detection utilities
 */
export class BrowserFeatureDetector {
    /**
     * Detect available JavaScript features
     */
    static detectJavaScriptFeatures() {
        const features = {
            es6Modules: false,
            es6Classes: false,
            es6ArrowFunctions: false,
            es6TemplateLiterals: false,
            es6Destructuring: false,
            es6Promises: false,
            es6AsyncAwait: false,
            webNFC: false,
            webShare: false,
            serviceWorker: false,
            webAssembly: false,
            bigInt: false,
            dynamicImport: false
        };

        try {
            // ES6 Modules - check for module support
            features.es6Modules = typeof module !== 'undefined' && module.exports !== undefined;
        } catch (e) {}

        try {
            // ES6 Classes
            eval('class TestClass {}');
            features.es6Classes = true;
        } catch (e) {}

        try {
            // ES6 Arrow Functions
            eval('(() => {})');
            features.es6ArrowFunctions = true;
        } catch (e) {}

        try {
            // ES6 Template Literals
            eval('`template`');
            features.es6TemplateLiterals = true;
        } catch (e) {}

        try {
            // ES6 Destructuring
            eval('const {a} = {a: 1}');
            features.es6Destructuring = true;
        } catch (e) {}

        // ES6 Promises
        features.es6Promises = typeof Promise !== 'undefined';

        try {
            // ES6 Async/Await
            eval('async function test() { await Promise.resolve(); }');
            features.es6AsyncAwait = true;
        } catch (e) {}

        // Web APIs
        features.webNFC = typeof NDEFReader !== 'undefined';
        features.webShare = typeof navigator !== 'undefined' && typeof navigator.share !== 'undefined';
        features.serviceWorker = typeof navigator !== 'undefined' && 'serviceWorker' in navigator;
        features.webAssembly = typeof WebAssembly !== 'undefined';
        features.bigInt = typeof BigInt !== 'undefined';

        try {
            // Dynamic Import - check if Function constructor supports import
            features.dynamicImport = typeof Function('return import') === 'function';
        } catch (e) {}

        return features;
    }

    /**
     * Get browser information
     */
    static getBrowserInfo() {
        if (typeof navigator === 'undefined') {
            return {
                name: 'Unknown',
                version: 'Unknown',
                userAgent: 'Node.js or non-browser environment',
                platform: typeof process !== 'undefined' ? process.platform : 'unknown'
            };
        }

        const userAgent = navigator.userAgent;
        let browserName = 'Unknown';
        let browserVersion = 'Unknown';

        // Chrome
        if (userAgent.includes('Chrome') && !userAgent.includes('Edg')) {
            browserName = 'Chrome';
            const match = userAgent.match(/Chrome\/(\d+)/);
            if (match) browserVersion = match[1];
        }
        // Firefox
        else if (userAgent.includes('Firefox')) {
            browserName = 'Firefox';
            const match = userAgent.match(/Firefox\/(\d+)/);
            if (match) browserVersion = match[1];
        }
        // Safari
        else if (userAgent.includes('Safari') && !userAgent.includes('Chrome')) {
            browserName = 'Safari';
            const match = userAgent.match(/Version\/(\d+)/);
            if (match) browserVersion = match[1];
        }
        // Edge
        else if (userAgent.includes('Edg')) {
            browserName = 'Edge';
            const match = userAgent.match(/Edg\/(\d+)/);
            if (match) browserVersion = match[1];
        }

        return {
            name: browserName,
            version: browserVersion,
            userAgent: userAgent,
            platform: navigator.platform || 'Unknown'
        };
    }

    /**
     * Check if browser supports required features for NDEF library
     */
    static checkNdefLibraryCompatibility() {
        const features = this.detectJavaScriptFeatures();
        const browserInfo = this.getBrowserInfo();
        
        const requiredFeatures = [
            'es6Classes',
            'es6ArrowFunctions',
            'es6Promises'
        ];

        const recommendedFeatures = [
            'es6Modules',
            'es6AsyncAwait',
            'es6Destructuring'
        ];

        const compatibility = {
            supported: true,
            browserInfo,
            features,
            missingRequired: [],
            missingRecommended: [],
            webNFCSupported: features.webNFC,
            overallScore: 0
        };

        // Check required features
        requiredFeatures.forEach(feature => {
            if (!features[feature]) {
                compatibility.supported = false;
                compatibility.missingRequired.push(feature);
            }
        });

        // Check recommended features
        recommendedFeatures.forEach(feature => {
            if (!features[feature]) {
                compatibility.missingRecommended.push(feature);
            }
        });

        // Calculate compatibility score
        const totalFeatures = Object.keys(features).length;
        const supportedFeatures = Object.values(features).filter(Boolean).length;
        compatibility.overallScore = Math.round((supportedFeatures / totalFeatures) * 100);

        return compatibility;
    }
}

/**
 * Cross-browser testing utilities
 */
export class CrossBrowserTester {
    constructor() {
        this.testResults = new Map();
        this.browserInfo = BrowserFeatureDetector.getBrowserInfo();
    }

    /**
     * Run a test function and record results with browser context
     */
    async runTest(testName, testFunction) {
        const startTime = performance.now();
        let result;

        try {
            result = await testFunction();
            const endTime = performance.now();
            
            this.testResults.set(testName, {
                success: true,
                result,
                duration: endTime - startTime,
                browser: this.browserInfo,
                timestamp: new Date().toISOString(),
                error: null
            });
        } catch (error) {
            const endTime = performance.now();
            
            this.testResults.set(testName, {
                success: false,
                result: null,
                duration: endTime - startTime,
                browser: this.browserInfo,
                timestamp: new Date().toISOString(),
                error: {
                    message: error.message,
                    stack: error.stack,
                    name: error.name
                }
            });
        }

        return this.testResults.get(testName);
    }

    /**
     * Get all test results
     */
    getResults() {
        const results = {};
        this.testResults.forEach((value, key) => {
            results[key] = value;
        });
        return results;
    }

    /**
     * Generate compatibility report
     */
    generateCompatibilityReport() {
        const results = this.getResults();
        const totalTests = Object.keys(results).length;
        const passedTests = Object.values(results).filter(r => r.success).length;
        
        return {
            browser: this.browserInfo,
            compatibility: BrowserFeatureDetector.checkNdefLibraryCompatibility(),
            testResults: results,
            summary: {
                totalTests,
                passedTests,
                failedTests: totalTests - passedTests,
                successRate: totalTests > 0 ? Math.round((passedTests / totalTests) * 100) : 0
            },
            generatedAt: new Date().toISOString()
        };
    }

    /**
     * Clear all test results
     */
    clear() {
        this.testResults.clear();
    }
}

/**
 * Polyfill detector and loader
 */
export class PolyfillManager {
    constructor() {
        this.loadedPolyfills = new Set();
    }

    /**
     * Check what polyfills might be needed
     */
    checkPolyfillNeeds() {
        const needs = {
            promise: typeof Promise === 'undefined',
            fetch: typeof fetch === 'undefined',
            url: typeof URL === 'undefined',
            textEncoder: typeof TextEncoder === 'undefined',
            textDecoder: typeof TextDecoder === 'undefined',
            uint8Array: typeof Uint8Array === 'undefined'
        };

        return needs;
    }

    /**
     * Load basic polyfills for testing
     */
    async loadBasicPolyfills() {
        const needs = this.checkPolyfillNeeds();
        
        // Promise polyfill
        if (needs.promise && !this.loadedPolyfills.has('promise')) {
            await this.loadPromisePolyfill();
        }

        // TextEncoder/TextDecoder polyfill
        if ((needs.textEncoder || needs.textDecoder) && !this.loadedPolyfills.has('textencoder')) {
            await this.loadTextEncoderPolyfill();
        }

        // URL polyfill
        if (needs.url && !this.loadedPolyfills.has('url')) {
            await this.loadUrlPolyfill();
        }
    }

    /**
     * Simple Promise polyfill for very old browsers
     */
    async loadPromisePolyfill() {
        if (typeof Promise !== 'undefined') return;

        // Very basic Promise implementation
        window.Promise = class Promise {
            constructor(executor) {
                this.state = 'pending';
                this.value = undefined;
                this.handlers = [];

                const resolve = (value) => {
                    if (this.state === 'pending') {
                        this.state = 'fulfilled';
                        this.value = value;
                        this.handlers.forEach(handler => handler.onFulfilled(value));
                    }
                };

                const reject = (reason) => {
                    if (this.state === 'pending') {
                        this.state = 'rejected';
                        this.value = reason;
                        this.handlers.forEach(handler => handler.onRejected(reason));
                    }
                };

                try {
                    executor(resolve, reject);
                } catch (error) {
                    reject(error);
                }
            }

            then(onFulfilled, onRejected) {
                return new Promise((resolve, reject) => {
                    const handler = {
                        onFulfilled: (value) => {
                            try {
                                const result = onFulfilled ? onFulfilled(value) : value;
                                resolve(result);
                            } catch (error) {
                                reject(error);
                            }
                        },
                        onRejected: (reason) => {
                            try {
                                const result = onRejected ? onRejected(reason) : reason;
                                reject(result);
                            } catch (error) {
                                reject(error);
                            }
                        }
                    };

                    if (this.state === 'fulfilled') {
                        setTimeout(() => handler.onFulfilled(this.value), 0);
                    } else if (this.state === 'rejected') {
                        setTimeout(() => handler.onRejected(this.value), 0);
                    } else {
                        this.handlers.push(handler);
                    }
                });
            }

            catch(onRejected) {
                return this.then(null, onRejected);
            }

            static resolve(value) {
                return new Promise(resolve => resolve(value));
            }

            static reject(reason) {
                return new Promise((_, reject) => reject(reason));
            }
        };

        this.loadedPolyfills.add('promise');
    }

    /**
     * TextEncoder/TextDecoder polyfill
     */
    async loadTextEncoderPolyfill() {
        if (typeof TextEncoder !== 'undefined' && typeof TextDecoder !== 'undefined') return;

        // Basic TextEncoder implementation
        if (typeof TextEncoder === 'undefined') {
            window.TextEncoder = class TextEncoder {
                encode(string) {
                    const utf8 = [];
                    for (let i = 0; i < string.length; i++) {
                        let charCode = string.charCodeAt(i);
                        if (charCode < 0x80) {
                            utf8.push(charCode);
                        } else if (charCode < 0x800) {
                            utf8.push(0xc0 | (charCode >> 6), 0x80 | (charCode & 0x3f));
                        } else if (charCode < 0xd800 || charCode >= 0xe000) {
                            utf8.push(0xe0 | (charCode >> 12), 0x80 | ((charCode >> 6) & 0x3f), 0x80 | (charCode & 0x3f));
                        } else {
                            // Surrogate pair
                            i++;
                            charCode = 0x10000 + (((charCode & 0x3ff) << 10) | (string.charCodeAt(i) & 0x3ff));
                            utf8.push(0xf0 | (charCode >> 18), 0x80 | ((charCode >> 12) & 0x3f), 0x80 | ((charCode >> 6) & 0x3f), 0x80 | (charCode & 0x3f));
                        }
                    }
                    return new Uint8Array(utf8);
                }
            };
        }

        // Basic TextDecoder implementation
        if (typeof TextDecoder === 'undefined') {
            window.TextDecoder = class TextDecoder {
                decode(bytes) {
                    let result = '';
                    let i = 0;
                    while (i < bytes.length) {
                        let byte1 = bytes[i++];
                        if (byte1 < 0x80) {
                            result += String.fromCharCode(byte1);
                        } else if ((byte1 >> 5) === 0x06) {
                            let byte2 = bytes[i++];
                            result += String.fromCharCode(((byte1 & 0x1f) << 6) | (byte2 & 0x3f));
                        } else if ((byte1 >> 4) === 0x0e) {
                            let byte2 = bytes[i++];
                            let byte3 = bytes[i++];
                            result += String.fromCharCode(((byte1 & 0x0f) << 12) | ((byte2 & 0x3f) << 6) | (byte3 & 0x3f));
                        } else if ((byte1 >> 3) === 0x1e) {
                            let byte2 = bytes[i++];
                            let byte3 = bytes[i++];
                            let byte4 = bytes[i++];
                            let codePoint = ((byte1 & 0x07) << 18) | ((byte2 & 0x3f) << 12) | ((byte3 & 0x3f) << 6) | (byte4 & 0x3f);
                            codePoint -= 0x10000;
                            result += String.fromCharCode((codePoint >> 10) + 0xd800, (codePoint & 0x3ff) + 0xdc00);
                        }
                    }
                    return result;
                }
            };
        }

        this.loadedPolyfills.add('textencoder');
    }

    /**
     * Basic URL polyfill
     */
    async loadUrlPolyfill() {
        if (typeof URL !== 'undefined') return;

        // Very basic URL implementation
        window.URL = class URL {
            constructor(url, base) {
                // This is a very simplified implementation
                // In a real polyfill, you'd want a more complete implementation
                this.href = url;
                
                const match = url.match(/^(https?:)\/\/([^\/]+)(\/.*)?$/);
                if (match) {
                    this.protocol = match[1];
                    this.host = match[2];
                    this.pathname = match[3] || '/';
                } else {
                    this.protocol = '';
                    this.host = '';
                    this.pathname = url;
                }
            }

            toString() {
                return this.href;
            }
        };

        this.loadedPolyfills.add('url');
    }

    /**
     * Get list of loaded polyfills
     */
    getLoadedPolyfills() {
        return Array.from(this.loadedPolyfills);
    }
}

/**
 * Environment detection utilities
 */
export const EnvironmentDetector = {
    /**
     * Detect the current JavaScript environment
     */
    detectEnvironment() {
        // Node.js
        if (typeof process !== 'undefined' && process.versions && process.versions.node) {
            return {
                type: 'node',
                version: process.versions.node,
                details: {
                    platform: process.platform,
                    arch: process.arch
                }
            };
        }

        // Browser
        if (typeof window !== 'undefined' && typeof document !== 'undefined') {
            return {
                type: 'browser',
                version: BrowserFeatureDetector.getBrowserInfo().version,
                details: BrowserFeatureDetector.getBrowserInfo()
            };
        }

        // Web Worker
        if (typeof importScripts !== 'undefined') {
            return {
                type: 'webworker',
                version: 'unknown',
                details: {}
            };
        }

        // Service Worker
        if (typeof ServiceWorkerGlobalScope !== 'undefined') {
            return {
                type: 'serviceworker',
                version: 'unknown',
                details: {}
            };
        }

        return {
            type: 'unknown',
            version: 'unknown',
            details: {}
        };
    },

    /**
     * Check if running in a secure context
     */
    isSecureContext() {
        if (typeof isSecureContext !== 'undefined') {
            return isSecureContext;
        }
        
        if (typeof location !== 'undefined') {
            return location.protocol === 'https:' || location.hostname === 'localhost';
        }
        
        return false;
    },

    /**
     * Get comprehensive environment report
     */
    getEnvironmentReport() {
        return {
            environment: this.detectEnvironment(),
            secureContext: this.isSecureContext(),
            features: BrowserFeatureDetector.detectJavaScriptFeatures(),
            compatibility: BrowserFeatureDetector.checkNdefLibraryCompatibility(),
            timestamp: new Date().toISOString()
        };
    }
};