import { defineConfig } from 'vite'

export default defineConfig({
  root: 'src',
  build: {
    outDir: '../dist',
    emptyOutDir: true
  },
  server: {
    host: '0.0.0.0', // Allow external connections
    port: 5173
  }
})