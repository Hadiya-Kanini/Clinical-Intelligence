import { Outlet } from 'react-router-dom'
import AppShell from './AppShell'

export default function ProtectedLayout(): JSX.Element {
  return (
    <AppShell>
      <Outlet />
    </AppShell>
  )
}
