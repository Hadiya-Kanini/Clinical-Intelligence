/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Clinical Intelligence wireframe color palette
        primary: {
          100: '#e6f2ff',
          300: '#66b3ff',
          500: '#007bff',
          700: '#0056b3',
          900: '#004085',
        },
        success: {
          100: '#d4edda',
          500: '#28a745',
          700: '#155724',
        },
        error: {
          100: '#f8d7da',
          500: '#dc3545',
          700: '#721c24',
        },
        neutral: {
          50: '#ffffff',
          100: '#f8f9fa',
          300: '#dee2e6',
          500: '#6c757d',
          700: '#495057',
          900: '#212529',
        }
      },
      fontFamily: {
        primary: ['IBM Plex Sans', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'sans-serif'],
        mono: ['JetBrains Mono', 'Courier New', 'monospace'],
      },
      fontSize: {
        'h1': '2.441rem',
        'h2': '1.953rem',
        'body': '1rem',
        'small': '0.8rem',
      },
      fontWeight: {
        'regular': '400',
        'medium': '500',
        'semibold': '600',
        'bold': '700',
      },
      spacing: {
        '2': '0.5rem',
        '3': '0.75rem',
        '4': '1rem',
        '5': '1.5rem',
        '6': '2rem',
      },
      borderRadius: {
        'small': '0.25rem',
        'medium': '0.5rem',
      },
      boxShadow: {
        'medium': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        'focus': '0 0 0 3px rgba(0, 123, 255, 0.25)',
      },
      animation: {
        'spin': 'spin 0.6s linear infinite',
      },
      keyframes: {
        spin: {
          to: { transform: 'rotate(360deg)' },
        },
      },
    },
  },
  plugins: [],
}
