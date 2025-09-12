/**
 * Comprehensive Compatibility Test Suite
 * Main entry point for all compatibility tests
 */

describe('Library Compatibility Suite', () => {
  // Note: Individual compatibility test files should be run separately
  // This file contains additional cross-cutting compatibility tests

  describe('Browser Environment Compatibility', () => {
    test('should work in modern browser environments', async () => {
      // Test that the library works with modern browser APIs
      expect(typeof Uint8Array).toBe('function');
      expect(typeof Promise).toBe('function');
      expect(typeof Map).toBe('function');
      expect(typeof Set).toBe('function');
      
      // Test async/await support
      const testAsync = async () => {
        const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
        return new NdefRecord();
      };
      
      const record = await testAsync();
      expect(record).toBeDefined();
    });

    test('should handle Web APIs gracefully when available', async () => {
      const { NdefMessage } = await import('../../src/submodule/NdefMessage.js');
      
      // Test that the library doesn't break when Web APIs are not available
      const message = new NdefMessage();
      
      // Should work regardless of Web NFC API availability
      expect(message).toBeInstanceOf(NdefMessage);
      expect(message.getRecords()).toEqual([]);
    });

    test('should support ArrayBuffer and TypedArray operations', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      const record = new NdefRecord();
      
      // Test with different typed arrays
      const uint8Array = new Uint8Array([1, 2, 3, 4]);
      const uint16Array = new Uint16Array([256, 512]);
      const buffer = new ArrayBuffer(8);
      
      record.setPayload(uint8Array);
      expect(record.getPayload()).toEqual(uint8Array);
      
      // Test ArrayBuffer conversion
      const bufferView = new Uint8Array(buffer);
      record.setPayload(bufferView);
      expect(record.getPayload()).toEqual(bufferView);
    });
  });

  describe('Performance and Memory', () => {
    test('should not leak memory with repeated operations', async () => {
      const { NdefMessage, NdefRecord } = await import('../../src/index.js');
      
      // Create and destroy many objects to test for memory leaks
      const iterations = 1000;
      const startMemory = process.memoryUsage().heapUsed;
      
      for (let i = 0; i < iterations; i++) {
        const message = new NdefMessage();
        const record = new NdefRecord();
        
        record.setType('U');
        record.setPayload(new Uint8Array([i % 256]));
        message.addRecord(record);
        
        // Force some operations
        message.toByteArray();
        message.getRecords();
      }
      
      // Force garbage collection if available
      if (global.gc) {
        global.gc();
      }
      
      const endMemory = process.memoryUsage().heapUsed;
      const memoryIncrease = endMemory - startMemory;
      
      // Memory increase should be reasonable (less than 10MB for 1000 operations)
      expect(memoryIncrease).toBeLessThan(10 * 1024 * 1024);
    });

    test('should handle large payloads efficiently', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      const record = new NdefRecord();
      
      // Test with large payload (1MB)
      const largePayload = new Uint8Array(1024 * 1024);
      largePayload.fill(42);
      
      const startTime = Date.now();
      record.setPayload(largePayload);
      const payload = record.getPayload();
      const endTime = Date.now();
      
      expect(payload).toEqual(largePayload);
      expect(endTime - startTime).toBeLessThan(1000); // Should complete in less than 1 second
    });

    test('should perform well with many records', async () => {
      const { NdefMessage, NdefUriRecord } = await import('../../src/index.js');
      
      const message = new NdefMessage();
      const recordCount = 100;
      
      const startTime = Date.now();
      
      // Add many records
      for (let i = 0; i < recordCount; i++) {
        const record = new NdefUriRecord();
        record.setUri(`https://example${i}.com`);
        message.addRecord(record);
      }
      
      // Serialize message
      const bytes = message.toByteArray();
      
      const endTime = Date.now();
      
      expect(message.getRecords()).toHaveLength(recordCount);
      expect(bytes).toBeInstanceOf(Uint8Array);
      expect(endTime - startTime).toBeLessThan(5000); // Should complete in less than 5 seconds
    });
  });

  describe('Error Handling and Edge Cases', () => {
    test('should handle invalid module imports gracefully', async () => {
      try {
        await import('../../src/nonexistent.js');
        fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeDefined();
        expect(error.message).toMatch(/Cannot resolve module|Module not found/i);
      }
    });

    test('should handle circular dependencies if any exist', async () => {
      // Test that the module system handles any potential circular dependencies
      const modules = [
        '../../src/submodule/NdefRecord.js',
        '../../src/submodule/NdefMessage.js',
        '../../src/submodule/NdefUriRecord.js',
        '../../src/submodule/NdefTextRecord.js'
      ];
      
      // Import all modules simultaneously
      const importPromises = modules.map(module => import(module));
      const results = await Promise.all(importPromises);
      
      // All imports should succeed
      results.forEach((module, index) => {
        expect(module).toBeDefined();
        expect(Object.keys(module).length).toBeGreaterThan(0);
      });
    });

    test('should maintain consistent behavior across import methods', async () => {
      // Test that different import methods yield the same results
      const { NdefRecord: NamedImport } = await import('../../src/submodule/NdefRecord.js');
      const WildcardImport = await import('../../src/submodule/NdefRecord.js');
      const IndexImport = await import('../../src/index.js');
      
      // All should provide the same constructor
      expect(NamedImport).toBe(WildcardImport.NdefRecord);
      expect(NamedImport).toBe(IndexImport.NdefRecord);
      
      // Instances should behave identically
      const record1 = new NamedImport();
      const record2 = new WildcardImport.NdefRecord();
      const record3 = new IndexImport.NdefRecord();
      
      record1.setType('U');
      record2.setType('U');
      record3.setType('U');
      
      expect(record1.getType()).toBe(record2.getType());
      expect(record2.getType()).toBe(record3.getType());
    });
  });

  describe('Cross-Platform Compatibility', () => {
    test('should work consistently across different JavaScript engines', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      // Test features that might behave differently across engines
      const record = new NdefRecord();
      
      // Test property descriptors
      const descriptor = Object.getOwnPropertyDescriptor(record, 'constructor');
      expect(descriptor).toBeDefined();
      
      // Test prototype chain
      expect(Object.getPrototypeOf(record)).toBe(NdefRecord.prototype);
      
      // Test instanceof behavior
      expect(record instanceof NdefRecord).toBe(true);
      expect(record instanceof Object).toBe(true);
    });

    test('should handle different string encodings properly', async () => {
      const { NdefTextRecord } = await import('../../src/submodule/NdefTextRecord.js');
      
      const record = new NdefTextRecord();
      
      // Test various Unicode strings
      const testStrings = [
        'Hello World',           // ASCII
        'Héllo Wörld',          // Latin-1 extended
        '你好世界',              // Chinese
        '🌍🚀💻',               // Emojis
        'مرحبا بالعالم',         // Arabic
        'Здравствуй мир'        // Cyrillic
      ];
      
      testStrings.forEach(testString => {
        record.setText(testString);
        expect(record.getText()).toBe(testString);
      });
    });

    test('should handle different number formats and endianness', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      const record = new NdefRecord();
      
      // Test with various byte patterns
      const testArrays = [
        new Uint8Array([0x00, 0x01, 0x02, 0x03]),           // Little endian pattern
        new Uint8Array([0x03, 0x02, 0x01, 0x00]),           // Big endian pattern
        new Uint8Array([0xFF, 0xFE, 0xFD, 0xFC]),           // High values
        new Uint8Array([0x80, 0x40, 0x20, 0x10])            // Powers of 2
      ];
      
      testArrays.forEach(testArray => {
        record.setPayload(testArray);
        const retrieved = record.getPayload();
        
        expect(retrieved).toEqual(testArray);
        expect(retrieved.length).toBe(testArray.length);
        
        // Verify byte-by-byte equality
        for (let i = 0; i < testArray.length; i++) {
          expect(retrieved[i]).toBe(testArray[i]);
        }
      });
    });
  });
});