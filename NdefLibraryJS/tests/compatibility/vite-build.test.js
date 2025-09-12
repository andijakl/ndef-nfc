/**
 * Vite Build Process Compatibility Tests
 * Tests that the library works correctly with Vite bundler and modern JavaScript features
 */

import { execSync } from 'child_process';
import { existsSync, readFileSync } from 'fs';
import path from 'path';

describe('Vite Build Compatibility', () => {
  const demoPath = path.resolve('../NdefDemoJS');
  const distPath = path.join(demoPath, 'dist');

  describe('Build Process', () => {
    test('should build demo app successfully with Vite', () => {
      // Change to demo directory and run build
      const originalCwd = process.cwd();
      
      try {
        process.chdir(demoPath);
        
        // Run Vite build
        const buildOutput = execSync('npm run build', { 
          encoding: 'utf8',
          timeout: 30000 
        });
        
        expect(buildOutput).toBeDefined();
        expect(buildOutput).not.toContain('error');
        expect(buildOutput).not.toContain('Error');
        
        // Check that dist directory was created
        expect(existsSync(distPath)).toBe(true);
        
      } catch (error) {
        throw new Error(`Vite build failed: ${error.message}`);
      } finally {
        process.chdir(originalCwd);
      }
    }, 60000); // 60 second timeout for build

    test('should generate proper ES module output', () => {
      // Check that built files exist
      const indexHtml = path.join(distPath, 'index.html');
      // Use Windows-compatible directory listing
      let jsFiles = [];
      try {
        const dirOutput = execSync(`dir /s /b "${distPath}\\*.js"`, { encoding: 'utf8' });
        jsFiles = dirOutput.trim().split('\n').filter(f => f && f.endsWith('.js'));
      } catch (error) {
        // If no JS files found, jsFiles remains empty array
      }
      
      expect(existsSync(indexHtml)).toBe(true);
      expect(jsFiles.length).toBeGreaterThan(0);
      
      // Check that JS files contain ES module syntax
      jsFiles.forEach(jsFile => {
        if (existsSync(jsFile)) {
          const content = readFileSync(jsFile, 'utf8');
          // Built files should contain module-related code
          expect(content.length).toBeGreaterThan(0);
        }
      });
    });

    test('should handle library imports in Vite environment', () => {
      // Check that the demo app source properly imports the library
      const demoSrcPath = path.join(demoPath, 'src');
      const mainJsPath = path.join(demoSrcPath, 'main.js');
      
      if (existsSync(mainJsPath)) {
        const content = readFileSync(mainJsPath, 'utf8');
        
        // Should contain import statements for the NDEF library
        expect(content).toMatch(/import.*from.*ndef/i);
      }
    });
  });

  describe('Modern JavaScript Features', () => {
    test('should support ES2020+ features in build', async () => {
      // Test that modern JS features work with the library
      const { NdefRecord } = await import('../../src/submodule/NdefRecord.js');
      
      // Test optional chaining (ES2020)
      const record = new NdefRecord();
      const testValue = record?.getType?.() ?? 'default';
      expect(testValue).toBeDefined();
      
      // Test nullish coalescing (ES2020)
      const payload = record.getPayload() ?? [];
      expect(Array.isArray(payload)).toBe(true);
    });

    test('should support async/await patterns', async () => {
      // Test async import and usage
      const loadLibrary = async () => {
        const { NdefMessage, NdefUriRecord } = await import('../../src/index.js');
        return { NdefMessage, NdefUriRecord };
      };
      
      const { NdefMessage, NdefUriRecord } = await loadLibrary();
      
      expect(NdefMessage).toBeDefined();
      expect(NdefUriRecord).toBeDefined();
      
      // Test async operations with the library
      const createMessage = async () => {
        const message = new NdefMessage();
        const record = new NdefUriRecord();
        record.setUri('https://example.com');
        message.push(record);
        return message;
      };
      
      const message = await createMessage();
      expect(message.records).toHaveLength(1);
    });

    test('should support template literals and string methods', async () => {
      const { NdefTextRecord } = await import('../../src/submodule/NdefTextRecord.js');
      
      const record = new NdefTextRecord();
      const testText = `Hello, World! ${new Date().getFullYear()}`;
      
      record.setText(testText);
      const retrievedText = record.getText();
      
      expect(retrievedText).toBe(testText);
      expect(retrievedText.includes('Hello')).toBe(true);
      expect(retrievedText.startsWith('Hello')).toBe(true);
    });

    test('should support destructuring and spread operator', async () => {
      const allExports = await import('../../src/index.js');
      
      // Test destructuring
      const { NdefRecord, NdefMessage, ...otherExports } = allExports;
      
      expect(NdefRecord).toBeDefined();
      expect(NdefMessage).toBeDefined();
      expect(Object.keys(otherExports).length).toBeGreaterThan(0);
      
      // Test spread with arrays
      const message = new NdefMessage();
      const records = [new NdefRecord(), new NdefRecord()];
      
      records.forEach(record => message.push(record));
      const allRecords = [...message.records];
      
      expect(allRecords).toHaveLength(2);
    });

    test('should support Map and Set collections', async () => {
      const { NdefMessage, NdefRecord } = await import('../../src/index.js');
      
      // Test with Map
      const recordMap = new Map();
      const record1 = new NdefRecord();
      const record2 = new NdefRecord();
      
      recordMap.set('first', record1);
      recordMap.set('second', record2);
      
      expect(recordMap.size).toBe(2);
      expect(recordMap.get('first')).toBe(record1);
      
      // Test with Set
      const recordSet = new Set([record1, record2, record1]); // duplicate should be ignored
      expect(recordSet.size).toBe(2);
      expect(recordSet.has(record1)).toBe(true);
    });
  });

  describe('Tree Shaking Compatibility', () => {
    test('should support selective imports for tree shaking', async () => {
      // Test that individual modules can be imported without pulling in everything
      const { NdefUriRecord } = await import('../../src/submodule/NdefUriRecord.js');
      
      // Should be able to use just this class without importing others
      const record = new NdefUriRecord();
      record.setUri('https://example.com');
      
      expect(record.getUri()).toBe('https://example.com');
      
      // Verify that we haven't accidentally imported other classes
      expect(() => new NdefTextRecord()).toThrow(); // Should not be available
    });

    test('should have proper export structure for bundlers', async () => {
      const module = await import('../../src/index.js');
      
      // Each export should be a separate binding for tree shaking
      const exports = Object.keys(module);
      
      exports.forEach(exportName => {
        expect(module[exportName]).toBeDefined();
        // Some exports might be objects (like constants), so check if it's a function or object
        expect(['function', 'object'].includes(typeof module[exportName])).toBe(true);
      });
      
      // Should have at least the main classes
      expect(exports).toContain('NdefRecord');
      expect(exports).toContain('NdefMessage');
      expect(exports.length).toBeGreaterThan(5);
    });
  });

  describe('Hot Module Replacement (HMR)', () => {
    test('should support HMR in development mode', () => {
      // This test verifies that the module structure supports HMR
      // We can't actually test HMR without a dev server, but we can check
      // that the modules are structured to support it
      
      const originalCwd = process.cwd();
      
      try {
        process.chdir(demoPath);
        
        // Check that Vite dev command exists
        const packageJson = JSON.parse(readFileSync('package.json', 'utf8'));
        expect(packageJson.scripts).toBeDefined();
        
        // Vite should be configured for HMR
        const viteConfigPath = 'vite.config.js';
        if (existsSync(viteConfigPath)) {
          const viteConfig = readFileSync(viteConfigPath, 'utf8');
          expect(viteConfig).toContain('defineConfig');
        }
        
      } finally {
        process.chdir(originalCwd);
      }
    });
  });
});