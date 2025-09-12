/**
 * ES6 Module Import/Export Compatibility Tests
 * Tests that all library components can be imported and used as ES6 modules
 */

describe('ES6 Module Compatibility', () => {
  describe('Named Imports', () => {
    test('should import NdefRecord class', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      expect(NdefRecord).toBeDefined();
      expect(typeof NdefRecord).toBe('function');
      
      // Test instantiation
      const record = new NdefRecord();
      expect(record).toBeInstanceOf(NdefRecord);
    });

    test('should import NdefMessage class', async () => {
      const { NdefMessage } = await import('../../src/submodule/NdefMessage.js');
      expect(NdefMessage).toBeDefined();
      expect(typeof NdefMessage).toBe('function');
      
      // Test instantiation
      const message = new NdefMessage();
      expect(message).toBeInstanceOf(NdefMessage);
    });

    test('should import NdefUriRecord class', async () => {
      const { NdefUriRecord } = await import('../../src/submodule/NdefUriRecord.js');
      expect(NdefUriRecord).toBeDefined();
      expect(typeof NdefUriRecord).toBe('function');
      
      // Test instantiation
      const record = new NdefUriRecord();
      expect(record).toBeInstanceOf(NdefUriRecord);
    });

    test('should import NdefTextRecord class', async () => {
      const { NdefTextRecord } = await import('../../src/submodule/NdefTextRecord.js');
      expect(NdefTextRecord).toBeDefined();
      expect(typeof NdefTextRecord).toBe('function');
      
      // Test instantiation
      const record = new NdefTextRecord();
      expect(record).toBeInstanceOf(NdefTextRecord);
    });

    test('should import specialized record classes', async () => {
      const modules = [
        '../../src/submodule/NdefGeoRecord.js',
        '../../src/submodule/NdefSocialRecord.js',
        '../../src/submodule/NdefTelRecord.js',
        '../../src/submodule/NdefAndroidAppRecord.js'
      ];

      for (const modulePath of modules) {
        const module = await import(modulePath);
        const className = modulePath.split('/').pop().replace('.js', '');
        const ClassConstructor = module[className];
        
        expect(ClassConstructor).toBeDefined();
        expect(typeof ClassConstructor).toBe('function');
        
        // Test instantiation
        const instance = new ClassConstructor();
        expect(instance).toBeInstanceOf(ClassConstructor);
      }
    });
  });

  describe('Wildcard Imports', () => {
    test('should import all exports from main index', async () => {
      const allExports = await import('../../src/index.js');
      
      // Check that main classes are exported
      expect(allExports.NdefRecord).toBeDefined();
      expect(allExports.NdefMessage).toBeDefined();
      expect(allExports.NdefUriRecord).toBeDefined();
      expect(allExports.NdefTextRecord).toBeDefined();
      expect(allExports.NdefGeoRecord).toBeDefined();
      expect(allExports.NdefSocialRecord).toBeDefined();
      expect(allExports.NdefTelRecord).toBeDefined();
      expect(allExports.NdefAndroidAppRecord).toBeDefined();
    });

    test('should allow destructuring imports', async () => {
      const {
        NdefRecord,
        NdefMessage,
        NdefUriRecord,
        NdefTextRecord
      } = await import('../../src/index.js');
      
      expect(NdefRecord).toBeDefined();
      expect(NdefMessage).toBeDefined();
      expect(NdefUriRecord).toBeDefined();
      expect(NdefTextRecord).toBeDefined();
      
      // Test that they're functional
      const record = new NdefRecord();
      const message = new NdefMessage();
      const uriRecord = new NdefUriRecord();
      const textRecord = new NdefTextRecord();
      
      expect(record).toBeInstanceOf(NdefRecord);
      expect(message).toBeInstanceOf(NdefMessage);
      expect(uriRecord).toBeInstanceOf(NdefUriRecord);
      expect(textRecord).toBeInstanceOf(NdefTextRecord);
    });
  });

  describe('Module Interoperability', () => {
    test('should work with classes from different modules together', async () => {
      const { NdefMessage } = await import('../../src/submodule/NdefMessage.js');
      const { NdefUriRecord } = await import('../../src/submodule/NdefUriRecord.js');
      const { NdefTextRecord } = await import('../../src/submodule/NdefTextRecord.js');
      
      // Create a message with records from different modules
      const message = new NdefMessage();
      const uriRecord = new NdefUriRecord();
      const textRecord = new NdefTextRecord();
      
      // Test that they can work together
      expect(() => {
        message.push(uriRecord);
        message.push(textRecord);
      }).not.toThrow();
      
      expect(message.records).toHaveLength(2);
    });

    test('should maintain prototype chain across modules', async () => {
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      const { NdefUriRecord } = await import('../../src/submodule/NdefUriRecord.js');
      
      const uriRecord = new NdefUriRecord();
      
      // URI record should inherit from NdefRecord
      expect(uriRecord).toBeInstanceOf(NdefUriRecord);
      expect(uriRecord).toBeInstanceOf(NdefRecord);
    });
  });

  describe('Dynamic Imports', () => {
    test('should support dynamic imports for code splitting', async () => {
      // Test dynamic import with conditional loading
      const shouldLoadUri = true;
      
      if (shouldLoadUri) {
        const { NdefUriRecord } = await import('../../src/submodule/NdefUriRecord.js');
        expect(NdefUriRecord).toBeDefined();
        
        const record = new NdefUriRecord();
        expect(record).toBeInstanceOf(NdefUriRecord);
      }
    });

    test('should handle import errors gracefully', async () => {
      try {
        await import('../../src/nonexistent-module.js');
        fail('Should have thrown an error for non-existent module');
      } catch (error) {
        expect(error).toBeDefined();
        expect(error.message).toMatch(/Cannot resolve module|Configuration error|Module not found/i);
      }
    });
  });

  describe('Module Metadata', () => {
    test('should have proper module.exports structure', async () => {
      const module = await import('../../src/index.js');
      
      // Check that exports are properly defined
      const exportNames = Object.keys(module);
      expect(exportNames).toContain('NdefRecord');
      expect(exportNames).toContain('NdefMessage');
      expect(exportNames).toContain('NdefUriRecord');
      expect(exportNames).toContain('NdefTextRecord');
      
      // Check that exports are not undefined
      exportNames.forEach(exportName => {
        expect(module[exportName]).toBeDefined();
      });
    });

    test('should support import.meta if available', async () => {
      // This test checks if the environment supports import.meta
      // which is part of the ES2020 specification
      const moduleWithMeta = await import('../../src/index.js');
      
      // We can't directly test import.meta from here, but we can verify
      // the module loads without errors in an ES2020+ environment
      expect(moduleWithMeta).toBeDefined();
    });
  });
});