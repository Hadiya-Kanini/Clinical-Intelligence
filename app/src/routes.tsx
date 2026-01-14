import type { ReactElement } from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import App from './App.tsx'
import LoginPage from './pages/LoginPage.tsx'
import ForgotPasswordPage from './pages/ForgotPasswordPage.tsx'
import DashboardPage from './pages/DashboardPage.tsx'
import ProtectedLayout from './components/ProtectedLayout.tsx'
import ResetPasswordPage from './pages/ResetPasswordPage.tsx'
import DocumentUploadPage from './pages/DocumentUploadPage.tsx'
import DocumentListPage from './pages/DocumentListPage.tsx'
import Patient360Page from './pages/Patient360Page.tsx'
import ExportPage from './pages/ExportPage.tsx'
import AdminDashboardPage from './pages/AdminDashboardPage.tsx'
import UserManagementPage from './pages/UserManagementPage.tsx'

export function isAuthenticated(): boolean {
  try {
    return window.localStorage.getItem('ci_auth') === '1'
  } catch {
    return false
  }
}

export function getUserRole(): 'admin' | 'standard' | null {
  try {
    const role = window.localStorage.getItem('ci_user_role')
    if (role === 'admin' || role === 'standard') {
      return role
    }
    return null
  } catch {
    return null
  }
}

export function isAdmin(): boolean {
  return getUserRole() === 'admin'
}

type RequireAuthProps = {
  children: ReactElement
}

export function RequireAuth({ children }: RequireAuthProps): ReactElement {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />
  }

  return children
}

type RequireAdminProps = {
  children: ReactElement
}

export function RequireAdmin({ children }: RequireAdminProps): ReactElement {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />
  }

  if (!isAdmin()) {
    return <Navigate to="/dashboard" replace />
  }

  return children
}

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      {
        index: true,
        element: <Navigate to="/login" replace />,
      },
      {
        path: 'login',
        element: <LoginPage />,
      },
      {
        path: 'forgot-password',
        element: <ForgotPasswordPage />,
      },
      {
        path: 'reset-password',
        element: <ResetPasswordPage />,
      },
      {
        element: (
          <RequireAuth>
            <ProtectedLayout />
          </RequireAuth>
        ),
        children: [
          {
            path: 'dashboard',
            element: <DashboardPage />,
          },
          {
            path: 'documents/upload',
            element: <DocumentUploadPage />,
          },
          {
            path: 'documents',
            element: <DocumentListPage />,
          },
          {
            path: 'patients/:patientId',
            element: <Patient360Page />,
          },
          {
            path: 'export',
            element: <ExportPage />,
          },
          {
            path: 'admin',
            element: (
              <RequireAdmin>
                <AdminDashboardPage />
              </RequireAdmin>
            ),
          },
          {
            path: 'admin/users',
            element: (
              <RequireAdmin>
                <UserManagementPage />
              </RequireAdmin>
            ),
          },
        ],
      },
    ],
  },
])

export default router
