/**
 * Basic setup test to verify Jest configuration
 */

describe('Jest Setup', () => {
  test('should have proper test environment', () => {
    expect(typeof describe).toBe('function');
    expect(typeof test).toBe('function');
    expect(typeof expect).toBe('function');
  });

  test('should have mocked Web NFC API', () => {
    expect(global.NDEFReader).toBeDefined();
    expect(typeof global.NDEFReader).toBe('function');
    
    const reader = new global.NDEFReader();
    expect(reader).toHaveProperty('scan');
    expect(reader).toHaveProperty('write');
    expect(reader).toHaveProperty('stop');
  });

  test('should have mocked Web Share API', () => {
    expect(global.navigator.share).toBeDefined();
    expect(typeof global.navigator.share).toBe('function');
  });

  test('should support ES6 features', () => {
    const testArray = [1, 2, 3];
    const doubled = testArray.map(x => x * 2);
    expect(doubled).toEqual([2, 4, 6]);
    
    const { length } = testArray;
    expect(length).toBe(3);
  });

  test('should support async/await', async () => {
    const promise = Promise.resolve('test');
    const result = await promise;
    expect(result).toBe('test');
  });
});