// Jest setup file for NDEF library testing

// Polyfill TextEncoder and TextDecoder for Node.js environment
import { TextEncoder, TextDecoder } from 'util';
global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder;

// Mock Web NFC API for testing
global.NDEFReader = class MockNDEFReader {
  constructor() {
    this.scanning = false;
    this.onreading = null;
    this.onerror = null;
  }
  
  async scan() {
    this.scanning = true;
    return Promise.resolve();
  }
  
  async write(message) {
    return Promise.resolve();
  }
  
  async stop() {
    this.scanning = false;
    return Promise.resolve();
  }
};

// Mock navigator.share for Web Share API testing
global.navigator = global.navigator || {};
global.navigator.share = jest.fn(() => Promise.resolve());

// Set up console error/warn suppression for expected test errors
const originalError = console.error;
const originalWarn = console.warn;

beforeEach(() => {
  // Reset mocks before each test
  jest.clearAllMocks();
});

afterEach(() => {
  // Clean up after each test
  console.error = originalError;
  console.warn = originalWarn;
});

// Helper function to suppress console errors during tests
global.suppressConsoleErrors = () => {
  console.error = jest.fn();
  console.warn = jest.fn();
};

// Helper function to restore console
global.restoreConsole = () => {
  console.error = originalError;
  console.warn = originalWarn;
};