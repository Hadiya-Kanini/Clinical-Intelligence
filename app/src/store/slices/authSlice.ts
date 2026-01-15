import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import axios from 'axios'

interface User {
  id: string
  email: string
  role: 'admin' | 'standard'
}

interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,
}

// Async thunks
export const loginAsync = createAsyncThunk(
  'auth/login',
  async (credentials: { email: string; password: string }, { rejectWithValue }) => {
    try {
      const response = await axios.post('/api/v1/auth/login', credentials, {
        withCredentials: true
      })
      const { user } = response.data
      
      // Clear any legacy localStorage keys
      localStorage.removeItem('ci_auth')
      localStorage.removeItem('ci_token')
      localStorage.removeItem('ci_user_role')
      
      return { user }
    } catch (error: any) {
      const errorMessage = error.response?.data?.error?.message || 'Login failed'
      return rejectWithValue(errorMessage)
    }
  }
)

export const logoutAsync = createAsyncThunk(
  'auth/logout',
  async (_, { rejectWithValue }) => {
    try {
      await axios.post('/api/v1/auth/logout', {}, {
        withCredentials: true
      })
      
      // Clear any legacy localStorage keys
      localStorage.removeItem('ci_auth')
      localStorage.removeItem('ci_token')
      localStorage.removeItem('ci_user_role')
      
      return null
    } catch (error: any) {
      const errorMessage = error.response?.data?.error?.message || 'Logout failed'
      return rejectWithValue(errorMessage)
    }
  }
)

export const checkAuthAsync = createAsyncThunk(
  'auth/checkAuth',
  async (_, { rejectWithValue }) => {
    try {
      // Cookie-based auth: no token check needed, cookie is sent automatically
      const response = await axios.get('/api/v1/auth/me', {
        withCredentials: true
      })
      
      return response.data
    } catch (error: any) {
      // Clear any legacy localStorage keys on auth failure
      localStorage.removeItem('ci_auth')
      localStorage.removeItem('ci_token')
      localStorage.removeItem('ci_user_role')
      return rejectWithValue('Invalid session')
    }
  }
)

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null
    },
  },
  extraReducers: (builder) => {
    builder
      // Login
      .addCase(loginAsync.pending, (state) => {
        state.isLoading = true
        state.error = null
      })
      .addCase(loginAsync.fulfilled, (state, action) => {
        state.isLoading = false
        state.isAuthenticated = true
        state.user = action.payload.user
        state.error = null
      })
      .addCase(loginAsync.rejected, (state, action) => {
        state.isLoading = false
        state.error = action.payload as string
        state.isAuthenticated = false
        state.user = null
      })
      // Logout
      .addCase(logoutAsync.pending, (state) => {
        state.isLoading = true
      })
      .addCase(logoutAsync.fulfilled, (state) => {
        state.isLoading = false
        state.isAuthenticated = false
        state.user = null
        state.error = null
      })
      .addCase(logoutAsync.rejected, (state, action) => {
        state.isLoading = false
        state.error = action.payload as string
        // Still logout on error
        state.isAuthenticated = false
        state.user = null
      })
      // Check auth
      .addCase(checkAuthAsync.fulfilled, (state, action) => {
        state.isAuthenticated = true
        state.user = action.payload
        state.error = null
      })
      .addCase(checkAuthAsync.rejected, (state) => {
        state.isAuthenticated = false
        state.user = null
      })
  },
})

export const { clearError } = authSlice.actions
export default authSlice.reducer
