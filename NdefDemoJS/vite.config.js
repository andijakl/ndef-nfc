import { defineConfig } from 'vite';
import basicSsl from '@vitejs/plugin-basic-ssl';

// Web NFC is only available in secure contexts (https:// or http://localhost).
// "npm run dev:https" serves the demo with a self-signed certificate, so it can be
// opened from an Android phone in the same network.
export default defineConfig(({ mode }) => ({
  root: 'src',
  // Relative asset paths, so the build can be hosted in any sub folder (e.g. GitHub Pages)
  base: './',
  plugins: mode === 'https' ? [basicSsl()] : [],
  build: {
    outDir: '../dist',
    emptyOutDir: true,
  },
  server: {
    host: '0.0.0.0', // Allow connections from other devices in the network
    port: 5173,
  },
  preview: {
    host: '0.0.0.0',
    port: 4173,
  },
}));
