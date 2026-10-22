import terser from '@rollup/plugin-terser';

// Single-file ES module bundles for use from a CDN or a plain <script type="module">.
// npm consumers import the sources in ./src directly.
export default {
  input: 'src/index.js',
  output: [
    {
      file: 'dist/ndef-library.js',
      format: 'es',
    },
    {
      file: 'dist/ndef-library.min.js',
      format: 'es',
      plugins: [terser()],
      sourcemap: true,
    },
  ],
};
