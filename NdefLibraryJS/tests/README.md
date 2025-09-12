# NDEF Library Testing

This directory contains comprehensive tests for the JavaScript NDEF library and demo application.

## Test Structure

```
tests/
├── unit-tests/           # Jest-based unit tests for library components
├── integration-tests/    # End-to-end library functionality tests
├── demo-tests/          # Demo application testing
├── test-data/           # Sample NDEF data for testing
├── utils/               # Testing utilities and helpers
├── setup.js             # Jest setup and configuration
└── setup.test.js        # Basic setup verification test
```

## Running Tests

### Basic test execution
```bash
npm test
```

### Watch mode (runs tests on file changes)
```bash
npm run test:watch
```

### Coverage reporting
```bash
npm run test:coverage
```

### CI mode (for continuous integration)
```bash
npm run test:ci
```

## Test Configuration

- **Jest Configuration**: `jest.config.js` - Main Jest configuration with ES6 module support
- **Babel Configuration**: `babel.config.js` - Babel setup for ES6 transpilation
- **Setup File**: `tests/setup.js` - Global test setup, mocks, and utilities

## Features

### ES6 Module Support
The testing environment is configured to work with ES6 modules using Babel transpilation.

### Web API Mocking
- **Web NFC API**: Mocked `NDEFReader` class for testing NFC functionality
- **Web Share API**: Mocked `navigator.share` for testing sharing features

### Coverage Reporting
- HTML reports in `coverage/` directory
- Console output with detailed coverage metrics
- LCOV format for CI integration
- JSON format for programmatic access

### Test Utilities
- Console error/warning suppression for expected test errors
- Helper functions for test setup and teardown
- Mock restoration between tests

## Writing Tests

### Unit Tests
Place unit tests in `unit-tests/` directory with `.test.js` extension:

```javascript
import { NdefRecord } from '../../src/submodule/NdefRecord.js';

describe('NdefRecord', () => {
  test('should create record with valid parameters', () => {
    // Test implementation
  });
});
```

### Integration Tests
Place integration tests in `integration-tests/` directory for end-to-end scenarios.

### Demo Tests
Place demo application tests in `demo-tests/` directory for UI and Web NFC integration testing.

## Test Data
Use the `test-data/` directory for:
- Sample NDEF messages
- Test vectors
- Known good data from other implementations

## Utilities
The `utils/` directory contains:
- Test helper functions
- Mock implementations
- Data generators
- Validation utilities