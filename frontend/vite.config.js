import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  build: {
    outDir: "dist",
    cssCodeSplit: false, 
    rollupOptions: {
      output: {
        manualChunks: () => "everything.js",
        entryFileNames: "assets/app.js",
        chunkFileNames: "assets/app.js",
        assetFileNames: "assets/app.css",
      }
    }
  }
});
