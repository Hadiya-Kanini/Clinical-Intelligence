import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import { RequireAuth, RequireAdmin } from '../../routes'

/**
 * RBAC Route Guard Tests (US_033 TASK_004)
 * 
 * These tests validate that route guards properly enforce authentication
 * and role-based access control using Redux auth state (server-derived).
 */

function createTestStore(authState: {
  isAuthenticated: boolean
  user: { id: string; email: string; role: 'admin' | 'standard' } | null
}) {
  return configureStore({
    reducer: {
      auth: () => authState,
      ui: () => ({}),
    },
  })
}

// Test components
function ProtectedContent() {
  return <div data-testid="protected-content">Protected Content</div>
}

function AdminContent() {
  return <div data-testid="admin-content">Admin Content</div>
}

function LoginPage() {
  return <div data-testid="login-page">Login Page</div>
}

function DashboardPage() {
  return <div data-testid="dashboard-page">Dashboard Page</div>
}

describe('RequireAuth Route Guard', () => {
  it('redirects to /login when user is not authenticated', () => {
    const store = createTestStore({
      isAuthenticated: false,
      user: null,
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/protected']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/protected"
              element={
                <RequireAuth>
                  <ProtectedContent />
                </RequireAuth>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('login-page')).toBeInTheDocument()
    expect(screen.queryByTestId('protected-content')).not.toBeInTheDocument()
  })

  it('renders children when user is authenticated', () => {
    const store = createTestStore({
      isAuthenticated: true,
      user: { id: '123', email: 'test@example.com', role: 'standard' },
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/protected']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/protected"
              element={
                <RequireAuth>
                  <ProtectedContent />
                </RequireAuth>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('protected-content')).toBeInTheDocument()
    expect(screen.queryByTestId('login-page')).not.toBeInTheDocument()
  })

  it('redirects to /login when isAuthenticated is true but user is null', () => {
    const store = createTestStore({
      isAuthenticated: true,
      user: null,
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/protected']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/protected"
              element={
                <RequireAuth>
                  <ProtectedContent />
                </RequireAuth>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('login-page')).toBeInTheDocument()
  })
})

describe('RequireAdmin Route Guard', () => {
  it('redirects to /login when user is not authenticated', () => {
    const store = createTestStore({
      isAuthenticated: false,
      user: null,
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/admin']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route
              path="/admin"
              element={
                <RequireAdmin>
                  <AdminContent />
                </RequireAdmin>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('login-page')).toBeInTheDocument()
    expect(screen.queryByTestId('admin-content')).not.toBeInTheDocument()
  })

  it('redirects to /dashboard when user is authenticated but not admin', () => {
    const store = createTestStore({
      isAuthenticated: true,
      user: { id: '123', email: 'test@example.com', role: 'standard' },
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/admin']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route
              path="/admin"
              element={
                <RequireAdmin>
                  <AdminContent />
                </RequireAdmin>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('dashboard-page')).toBeInTheDocument()
    expect(screen.queryByTestId('admin-content')).not.toBeInTheDocument()
  })

  it('renders children when user is authenticated as admin', () => {
    const store = createTestStore({
      isAuthenticated: true,
      user: { id: '123', email: 'admin@example.com', role: 'admin' },
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/admin']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route
              path="/admin"
              element={
                <RequireAdmin>
                  <AdminContent />
                </RequireAdmin>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('admin-content')).toBeInTheDocument()
    expect(screen.queryByTestId('login-page')).not.toBeInTheDocument()
    expect(screen.queryByTestId('dashboard-page')).not.toBeInTheDocument()
  })

  it('handles /admin/users route correctly for admin users', () => {
    const store = createTestStore({
      isAuthenticated: true,
      user: { id: '123', email: 'admin@example.com', role: 'admin' },
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/admin/users']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route
              path="/admin/users"
              element={
                <RequireAdmin>
                  <AdminContent />
                </RequireAdmin>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('admin-content')).toBeInTheDocument()
  })

  it('redirects standard user from /admin/users to /dashboard', () => {
    const store = createTestStore({
      isAuthenticated: true,
      user: { id: '123', email: 'test@example.com', role: 'standard' },
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/admin/users']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route
              path="/admin/users"
              element={
                <RequireAdmin>
                  <AdminContent />
                </RequireAdmin>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    expect(screen.getByTestId('dashboard-page')).toBeInTheDocument()
    expect(screen.queryByTestId('admin-content')).not.toBeInTheDocument()
  })
})

describe('Role-Based Access Control Integration', () => {
  it('uses server-derived role from Redux state, not localStorage', () => {
    // This test verifies that the route guards use Redux state
    // and not localStorage for authorization decisions
    
    // Set localStorage to admin (should be ignored)
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key) => {
      if (key === 'ci_user_role') return 'admin'
      if (key === 'ci_auth') return '1'
      return null
    })

    // But Redux state says standard user
    const store = createTestStore({
      isAuthenticated: true,
      user: { id: '123', email: 'test@example.com', role: 'standard' },
    })

    render(
      <Provider store={store}>
        <MemoryRouter initialEntries={['/admin']}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route
              path="/admin"
              element={
                <RequireAdmin>
                  <AdminContent />
                </RequireAdmin>
              }
            />
          </Routes>
        </MemoryRouter>
      </Provider>
    )

    // Should redirect to dashboard because Redux says standard, not admin
    expect(screen.getByTestId('dashboard-page')).toBeInTheDocument()
    expect(screen.queryByTestId('admin-content')).not.toBeInTheDocument()

    vi.restoreAllMocks()
  })
})
