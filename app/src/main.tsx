import React from 'react'
import ReactDOM from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { Provider } from 'react-redux'
import axios from 'axios'
import { store } from './store'
import router from './routes.tsx'
import './index.css'

// Configure axios to send cookies with all requests
axios.defaults.withCredentials = true

// Add response interceptor to handle session invalidation (401 with session_invalidated code)
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const errorCode = error.response?.data?.error?.code
      
      // Clear legacy localStorage keys
      localStorage.removeItem('ci_auth')
      localStorage.removeItem('ci_token')
      localStorage.removeItem('ci_user_role')
      
      // Determine the redirect state based on error code
      const authState = errorCode === 'session_invalidated' ? 'session_invalidated' : 'expired'
      
      // Redirect to login with appropriate state
      window.location.href = `/login?auth=${authState}`
    }
    return Promise.reject(error)
  }
)

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Provider store={store}>
      <RouterProvider router={router} />
    </Provider>
  </React.StrictMode>
)
