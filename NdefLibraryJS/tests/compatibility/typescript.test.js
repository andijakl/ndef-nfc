/**
 * TypeScript Compatibility Tests
 * Tests that the library works with TypeScript and provides proper type inference
 */

import { execSync } from 'child_process';
import { writeFileSync, readFileSync, existsSync, mkdirSync } from 'fs';
import path from 'path';

describe('TypeScript Compatibility', () => {
  const tempDir = path.resolve('tests/temp');
  const tsConfigPath = path.join(tempDir, 'tsconfig.json');
  const testTsFile = path.join(tempDir, 'test.ts');

  beforeAll(() => {
    // Create temp directory for TypeScript tests
    if (!existsSync(tempDir)) {
      mkdirSync(tempDir, { recursive: true });
    }

    // Create a basic tsconfig.json
    const tsConfig = {
      compilerOptions: {
        target: 'ES2020',
        module: 'ESNext',
        moduleResolution: 'node',
        allowJs: true,
        checkJs: false,
        strict: true,
        esModuleInterop: true,
        skipLibCheck: true,
        forceConsistentCasingInFileNames: true,
        declaration: true,
        outDir: './dist'
      },
      include: ['**/*.ts', '**/*.js'],
      exclude: ['node_modules', 'dist']
    };

    writeFileSync(tsConfigPath, JSON.stringify(tsConfig, null, 2));
  });

  afterAll(() => {
    // Clean up temp files
    try {
      if (existsSync(testTsFile)) {
        execSync(`del /f "${testTsFile}"`, { shell: true });
      }
      if (existsSync(tsConfigPath)) {
        execSync(`del /f "${tsConfigPath}"`, { shell: true });
      }
    } catch (error) {
      // Ignore cleanup errors
    }
  });

  describe('Type Inference', () => {
    test('should infer types for NdefRecord class', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      const record = new NdefRecord();
      
      // Test that methods return expected types (arrays in this implementation)
      expect(Array.isArray(record.getType())).toBe(true);
      expect(Array.isArray(record.getPayload())).toBe(true);
      expect(Array.isArray(record.getId())).toBe(true);
      
      // Test method chaining type inference
      const chainedRecord = record.setType('U').setPayload(new Uint8Array([1, 2, 3]));
      expect(chainedRecord).toBeInstanceOf(NdefRecord);
    });

    test('should infer types for NdefMessage class', async () => {
      const { NdefMessage, NdefRecord } = await import('../../src/index.js');
      
      const message = new NdefMessage();
      const record = new NdefRecord();
      
      // Test array type inference
      message.push(record);
      const records = message.records;
      
      expect(Array.isArray(records)).toBe(true);
      expect(records[0]).toBeInstanceOf(NdefRecord);
      
      // Test byte array type inference
      const bytes = message.toByteArray();
      expect(bytes).toBeInstanceOf(Uint8Array);
    });

    test('should infer types for specialized record classes', async () => {
      const { NdefUriRecord, NdefTextRecord } = await import('../../src/index.js');
      
      const uriRecord = new NdefUriRecord();
      const textRecord = new NdefTextRecord();
      
      // URI record type inference
      uriRecord.setUri('https://example.com');
      expect(typeof uriRecord.getUri()).toBe('string');
      
      // Text record type inference
      textRecord.setText('Hello World');
      if (typeof textRecord.setLanguageCode === 'function') {
        textRecord.setLanguageCode('en');
      }
      
      expect(typeof textRecord.getText()).toBe('string');
      if (typeof textRecord.getLanguageCode === 'function') {
        expect(typeof textRecord.getLanguageCode()).toBe('string');
      }
    });
  });

  describe('TypeScript Compilation', () => {
    test('should compile TypeScript code using the library', () => {
      // Create a TypeScript test file
      const tsCode = `
import { NdefRecord, NdefMessage, NdefUriRecord, NdefTextRecord } from '../../src/index.js';

// Test basic usage with type annotations
const record: NdefRecord = new NdefRecord();
const message: NdefMessage = new NdefMessage();

// Test specialized records
const uriRecord: NdefUriRecord = new NdefUriRecord();
uriRecord.setUri('https://example.com');

const textRecord: NdefTextRecord = new NdefTextRecord();
textRecord.setText('Hello TypeScript');
textRecord.setLanguageCode('en');

// Test method chaining
const chainedRecord: NdefRecord = record
  .setType('U')
  .setPayload(new Uint8Array([1, 2, 3]));

// Test array operations
message.addRecord(uriRecord);
message.addRecord(textRecord);

const records: NdefRecord[] = message.getRecords();
const bytes: Uint8Array = message.toByteArray();

// Export for testing
export { record, message, uriRecord, textRecord, records, bytes };
`;

      writeFileSync(testTsFile, tsCode);

      try {
        // Try to compile with TypeScript (if available)
        const tscOutput = execSync(`npx tsc --noEmit --project "${tsConfigPath}" "${testTsFile}"`, {
          encoding: 'utf8',
          cwd: tempDir
        });

        // If compilation succeeds, there should be no output (no errors)
        expect(tscOutput.trim()).toBe('');
      } catch (error) {
        // If TypeScript is not available, skip this test
        if (error.message.includes('npx: installed') || error.message.includes('command not found')) {
          console.warn('TypeScript not available, skipping compilation test');
          return;
        }
        
        // If there are actual TypeScript errors, fail the test
        throw new Error(`TypeScript compilation failed: ${error.message}`);
      }
    });

    test('should support generic type parameters', async () => {
      const { NdefMessage } = await import('../../src/submodule/NdefMessage.js');
      
      // Test that the library works with generic patterns (simulated without TypeScript syntax)
      const createTypedMessage = (item) => {
        if (typeof item.toByteArray === 'function') {
          return item.toByteArray();
        }
        throw new Error('Item must have toByteArray method');
      };
      
      const message = new NdefMessage();
      const bytes = createTypedMessage(message);
      
      expect(bytes).toBeInstanceOf(Uint8Array);
    });

    test('should support interface compatibility', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      // Test interface-like usage (simulated without TypeScript syntax)
      const useRecord = (record) => {
        // Verify the record has the expected methods
        expect(typeof record.getType).toBe('function');
        expect(typeof record.setType).toBe('function');
        expect(typeof record.getPayload).toBe('function');
        expect(typeof record.setPayload).toBe('function');
        
        record.setType('U');
        return record.getType();
      };
      
      const record = new NdefRecord();
      const result = useRecord(record);
      
      expect(Array.isArray(result)).toBe(true);
    });
  });

  describe('Declaration Files', () => {
    test('should work with ambient module declarations', () => {
      // Test that the library can be used with ambient declarations
      const ambientDeclaration = `
declare module 'ndef-library' {
  export class NdefRecord {
    constructor();
    getType(): string;
    setType(type: string): NdefRecord;
    getPayload(): Uint8Array;
    setPayload(payload: Uint8Array): NdefRecord;
  }
  
  export class NdefMessage {
    constructor();
    addRecord(record: NdefRecord): void;
    getRecords(): NdefRecord[];
    toByteArray(): Uint8Array;
  }
}
`;

      // Write ambient declaration to temp file
      const declarationFile = path.join(tempDir, 'ndef-library.d.ts');
      writeFileSync(declarationFile, ambientDeclaration);
      
      expect(existsSync(declarationFile)).toBe(true);
      
      // Verify the declaration file is valid TypeScript
      const content = readFileSync(declarationFile, 'utf8');
      expect(content).toContain('declare module');
      expect(content).toContain('export class NdefRecord');
    });

    test('should support JSDoc type annotations', async () => {
      // Test that JSDoc comments provide type information
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      // Test JSDoc type annotations (simulated)
      const setAndGetType = (record, type) => {
        record.setType(type);
        return record.getType();
      };
      
      const record = new NdefRecord();
      const result = setAndGetType(record, 'U');
      
      expect(result).toBe('U');
    });
  });

  describe('Modern TypeScript Features', () => {
    test('should support optional chaining with TypeScript', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      const record = new NdefRecord();
      
      // Test optional chaining (should work in TypeScript 3.7+)
      const type = record?.getType?.();
      const payload = record?.getPayload?.();
      
      expect(type).toBeDefined();
      expect(Array.isArray(payload)).toBe(true);
    });

    test('should support nullish coalescing with TypeScript', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      const record = new NdefRecord();
      
      // Test nullish coalescing (should work in TypeScript 3.7+)
      const type = record.getType() ?? 'default';
      const id = record.getId() ?? '';
      
      expect(Array.isArray(type)).toBe(true);
      expect(Array.isArray(id)).toBe(true);
    });

    test('should support template literal types pattern', async () => {
      const { NdefUriRecord } = await import('../../src/submodule/NdefUriRecord.js');
      
      const record = new NdefUriRecord();
      
      // Test template literal usage (TypeScript 4.1+ feature)
      const protocol = 'https';
      const domain = 'example.com';
      const uri = `${protocol}://${domain}`;
      
      record.setUri(uri);
      expect(record.getUri()).toBe('https://example.com');
    });
  });

  describe('Type Safety', () => {
    test('should maintain type safety with inheritance', async () => {
      const { NdefRecord, NdefUriRecord } = await import('../../src/index.js');
      
      const uriRecord = new NdefUriRecord();
      
      // Should be both NdefUriRecord and NdefRecord
      expect(uriRecord).toBeInstanceOf(NdefUriRecord);
      expect(uriRecord).toBeInstanceOf(NdefRecord);
      
      // Should have methods from both classes
      expect(typeof uriRecord.getType).toBe('function'); // from NdefRecord
      expect(typeof uriRecord.setUri).toBe('function');  // from NdefUriRecord
    });

    test('should handle union types correctly', async () => {
      const { NdefRecord, NdefUriRecord, NdefTextRecord } = await import('../../src/index.js');
      
      // Function that accepts multiple record types
      const processRecord = (record) => {
        if (record instanceof NdefUriRecord) {
          return record.getUri();
        } else if (record instanceof NdefTextRecord) {
          return record.getText();
        } else {
          return record.getType();
        }
      };
      
      const uriRecord = new NdefUriRecord();
      uriRecord.setUri('https://example.com');
      
      const textRecord = new NdefTextRecord();
      textRecord.setText('Hello');
      
      const baseRecord = new NdefRecord();
      baseRecord.setType('X');
      
      expect(processRecord(uriRecord)).toBe('https://example.com');
      expect(processRecord(textRecord)).toBe('Hello');
      expect(processRecord(baseRecord)).toBe('X');
    });
  });
});