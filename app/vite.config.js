import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const backendUrl = process.env.VITE_BACKEND_URL || 'http://localhost:5000'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: backendUrl,
        changeOrigin: true,
        secure: false,
        credentials: 'include',
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, res) => {
            console.log('Proxy request cookies:', req.headers.cookie);
            // Ensure cookies are forwarded
            if (req.headers.cookie) {
              proxyReq.setHeader('cookie', req.headers.cookie);
            }
          });
          proxy.on('proxyRes', (proxyRes, req, res) => {
            const cookies = proxyRes.headers['set-cookie'];
            console.log('Proxy response cookies:', cookies);
            if (cookies) {
              const rewrittenCookies = cookies.map(cookie => {
                // More permissive cookie handling
                return cookie
                  .replace(/; domain=.*?(;|$)/, ';')
                  .replace(/; path=.*?(;|$)/, '; path=/;')
                  .replace(/; secure/i, '')
                  .replace(/; samesite=.*?(;|$)/i, '; samesite=lax;');
              });
              console.log('Rewritten cookies:', rewrittenCookies);
              proxyRes.headers['set-cookie'] = rewrittenCookies;
            }
            // Add CORS headers for credentials
            proxyRes.headers['access-control-allow-credentials'] = 'true';
            proxyRes.headers['access-control-allow-origin'] = req.headers.origin || 'http://localhost:5173';
          });
        }
      },
      '/health': {
        target: backendUrl,
        changeOrigin: true,
        secure: false,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.js'],
    include: ['src/**/*.{test,spec}.{js,jsx,ts,tsx}'],
    exclude: ['**/node_modules/**', '**/dist/**', '**/src/__tests__/visual/**']
  }
})
