import { Outlet } from 'react-router-dom'
import { AuthInitializer } from './components/AuthInitializer'

export default function App(): JSX.Element {
  return (
    <>
      <AuthInitializer />
      <Outlet />
    </>
  )
}
