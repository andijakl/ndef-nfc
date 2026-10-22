import js from '@eslint/js';
import globals from 'globals';

export default [
  {
    ignores: ['dist/', 'types/', 'coverage/', 'doc/'],
  },
  js.configs.recommended,
  {
    rules: {
      'no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    },
  },
  {
    files: ['src/**/*.js'],
    languageOptions: {
      globals: globals.browser,
    },
  },
  {
    files: ['tests/**/*.js', '*.config.js'],
    languageOptions: {
      globals: { ...globals.browser, ...globals.node, ...globals.vitest },
    },
  },
];
