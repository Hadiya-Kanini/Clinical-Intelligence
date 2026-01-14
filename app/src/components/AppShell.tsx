import type { ReactNode } from 'react'
import { useEffect, useMemo, useState } from 'react'
import { NavLink, useLocation, useNavigate } from 'react-router-dom'
import { isAdmin } from '../routes'
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
  const [confirmLogoutOpen, setConfirmLogoutOpen] = useState(false)
  const [isLoggingOut, setIsLoggingOut] = useState(false)

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
      await fetch('/api/v1/auth/logout', { method: 'POST' })
    } catch {
    } finally {
      try {
        window.localStorage.removeItem('ci_auth')
        window.localStorage.removeItem('ci_token')
        window.localStorage.removeItem('ci_user_role')
      } catch {
      }

      setIsLoggingOut(false)
      setConfirmLogoutOpen(false)
      navigate('/login', { replace: true, state: { logout: 'success' } })
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

          {isAdmin() && (
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
    </div>
  )
}
