import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import { api } from '../../lib/apiClient'

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
    const result = await api.post<{ user: User }>('/api/v1/auth/login', credentials)
    
    if (!result.success) {
      return rejectWithValue(result.error.message || 'Login failed')
    }
    
    // Clear any legacy localStorage keys
    localStorage.removeItem('ci_auth')
    localStorage.removeItem('ci_token')
    localStorage.removeItem('ci_user_role')
    
    return { user: result.data.user }
  }
)

export const logoutAsync = createAsyncThunk(
  'auth/logout',
  async (_, { rejectWithValue }) => {
    const result = await api.post('/api/v1/auth/logout')
    
    // Clear any legacy localStorage keys regardless of result
    localStorage.removeItem('ci_auth')
    localStorage.removeItem('ci_token')
    localStorage.removeItem('ci_user_role')
    
    if (!result.success) {
      return rejectWithValue(result.error.message || 'Logout failed')
    }
    
    return null
  }
)

export const checkAuthAsync = createAsyncThunk(
  'auth/checkAuth',
  async (_, { rejectWithValue }) => {
    // Cookie-based auth: no token check needed, cookie is sent automatically
    const result = await api.get<User>('/api/v1/auth/me')
    
    if (!result.success) {
      // Clear any legacy localStorage keys on auth failure
      localStorage.removeItem('ci_auth')
      localStorage.removeItem('ci_token')
      localStorage.removeItem('ci_user_role')
      return rejectWithValue('Invalid session')
    }
    
    return result.data
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
