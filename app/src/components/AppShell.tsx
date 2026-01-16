import type { ReactNode } from 'react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { NavLink, useLocation, useNavigate } from 'react-router-dom'
import { useSelector } from 'react-redux'
import type { RootState } from '../store'
import { api } from '../lib/apiClient'
import { useInactivityTimeout } from '../hooks/useInactivityTimeout'
import Button from './ui/Button'
import Modal from './ui/Modal'

type AppShellProps = {
  children: ReactNode
}

function getTitle(pathname: string): string {
  if (pathname.startsWith('/documents/upload')) return 'Document upload'
  if (pathname.startsWith('/documents')) return 'Documents'
  if (pathname.startsWith('/patients/')) return 'Patient 360 view'
  if (pathname.startsWith('/export')) return 'Export'
  if (pathname.startsWith('/admin/users')) return 'User management'
  if (pathname.startsWith('/admin')) return 'Admin dashboard'
  if (pathname.startsWith('/dashboard')) return 'Dashboard'
  return 'Clinical Intelligence Platform'
}

export default function AppShell({ children }: AppShellProps): JSX.Element {
  const navigate = useNavigate()
  const location = useLocation()
  const { user } = useSelector((state: RootState) => state.auth)
  const [confirmLogoutOpen, setConfirmLogoutOpen] = useState(false)
  const [sessionExpiredOpen, setSessionExpiredOpen] = useState(false)
  const [isLoggingOut, setIsLoggingOut] = useState(false)

  // Derive admin status from Redux auth state (server-derived)
  const isUserAdmin = user?.role === 'admin'

  // Handle session expiration from inactivity timeout
  const handleInactivityTimeout = useCallback(() => {
    // Clear local auth indicators
    try {
      localStorage.removeItem('ci_auth')
      localStorage.removeItem('ci_token')
      localStorage.removeItem('ci_user_role')
    } catch {
      // Ignore localStorage errors
    }
    // Show session expired modal instead of immediate redirect
    setSessionExpiredOpen(true)
  }, [])

  // Use inactivity timeout hook
  useInactivityTimeout({
    onTimeout: handleInactivityTimeout,
    enabled: true,
  })

  const title = useMemo(() => getTitle(location.pathname), [location.pathname])

  useEffect(() => {
    function handleStorage(event: StorageEvent): void {
      if (event.key !== 'ci_auth') return
      if (event.newValue === '1') return
      navigate('/login', { replace: true, state: { logout: 'success' } })
    }

    window.addEventListener('storage', handleStorage)
    return () => window.removeEventListener('storage', handleStorage)
  }, [navigate])

  async function handleLogout(): Promise<void> {
    if (isLoggingOut) return

    setIsLoggingOut(true)

    try {
      console.log('Attempting logout...')
      const result = await api.post('/api/v1/auth/logout')
      console.log('Logout result:', result)
      
      // Immediate redirect test
      console.log('Logout successful - redirecting immediately...')
      window.location.href = '/login'
      return
      
    } catch (error) {
      console.error('Logout error:', error)
      // Even if API call fails, continue with local cleanup
    } finally {
      console.log('Starting cleanup and navigation...')
      try {
        console.log('Clearing localStorage...')
        window.localStorage.removeItem('ci_auth')
        window.localStorage.removeItem('ci_token')
        window.localStorage.removeItem('ci_user_role')
        console.log('LocalStorage cleared')
      } catch (error) {
        console.error('Error clearing localStorage:', error)
      }

      console.log('Setting loading state to false...')
      setIsLoggingOut(false)
      
      console.log('Closing logout modal...')
      setConfirmLogoutOpen(false)
      
      console.log('Navigating to login page...')
      try {
        navigate('/login', { replace: true, state: { logout: 'success' } })
        console.log('Navigation called successfully')
      } catch (error) {
        console.error('Navigation error:', error)
        // Fallback: use window.location
        console.log('Using fallback navigation...')
        window.location.href = '/login'
      }
    }
  }

  return (
    <div className="ui-shell">
      <aside className="ui-shell__sidebar" aria-label="Primary navigation">
        <div className="ui-shell__brand">
          <div className="ui-shell__brandMark" aria-hidden="true" />
          <div className="ui-shell__brandText">Clinical Intelligence</div>
        </div>

        <nav className="ui-shell__nav">
          <NavLink
            to="/dashboard"
            className={({ isActive }) => `ui-shell__navLink${isActive ? ' is-active' : ''}`}
          >
            Dashboard
          </NavLink>
          <NavLink
            to="/documents/upload"
            className={({ isActive }) => `ui-shell__navLink${isActive ? ' is-active' : ''}`}
          >
            Upload documents
          </NavLink>
          <NavLink
            to="/documents"
            className={({ isActive }) => `ui-shell__navLink${isActive ? ' is-active' : ''}`}
          >
            Document list
          </NavLink>
          <NavLink
            to="/patients/demo"
            className={({ isActive }) => `ui-shell__navLink${isActive ? ' is-active' : ''}`}
          >
            Patient 360 view
          </NavLink>
          <NavLink to="/export" className={({ isActive }) => `ui-shell__navLink${isActive ? ' is-active' : ''}`}>
            Export
          </NavLink>

          {isUserAdmin && (
            <>
              <div style={{ height: 'var(--space-6)' }} />

              <NavLink to="/admin" className={({ isActive }) => `ui-shell__navLink${isActive ? ' is-active' : ''}`}>
                Admin dashboard
              </NavLink>
              <NavLink
                to="/admin/users"
                className={({ isActive }) => `ui-shell__navLink${isActive ? ' is-active' : ''}`}
              >
                User management
              </NavLink>
            </>
          )}
        </nav>
      </aside>

      <div className="ui-shell__content">
        <header className="ui-shell__header" aria-label="Header">
          <div className="ui-shell__headerTitle">{title}</div>
          <div style={{ display: 'flex', gap: 'var(--space-3)', alignItems: 'center' }}>
            <Button variant="secondary" onClick={() => setConfirmLogoutOpen(true)}>
              Log out
            </Button>
          </div>
        </header>

        <main className="ui-shell__main">{children}</main>
      </div>

      <Modal
        open={confirmLogoutOpen}
        title="Confirm logout"
        onClose={() => setConfirmLogoutOpen(false)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setConfirmLogoutOpen(false)}>
              Cancel
            </Button>
            <Button variant="danger" loading={isLoggingOut} onClick={handleLogout}>
              Log out
            </Button>
          </>
        }
      >
        Are you sure you want to log out?
      </Modal>

      <Modal
        open={sessionExpiredOpen}
        title="Session Expired"
        onClose={() => {
          setSessionExpiredOpen(false)
          navigate('/login', { replace: true, state: { logout: 'expired', from: location } })
        }}
        footer={
          <Button
            variant="primary"
            onClick={() => {
              setSessionExpiredOpen(false)
              navigate('/login', { replace: true, state: { logout: 'expired', from: location } })
            }}
          >
            Log in again
          </Button>
        }
        aria-describedby="session-expired-description"
      >
        <p id="session-expired-description">
          Your session has expired due to inactivity. Please log in again to continue.
        </p>
        <p style={{ marginTop: 'var(--space-3)', fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)' }}>
          Note: Any unsaved work may need to be re-entered after logging in.
        </p>
      </Modal>
    </div>
  )
}
