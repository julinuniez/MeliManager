import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
  ],
  // ESTO ES LO NUEVO:
  server: {
    port: 5174, // Elegí el número que más te guste (ej: 8080, 3000, 5174)
    strictPort: true // Esto hace que si o si use este puerto o tire error si está ocupado
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})