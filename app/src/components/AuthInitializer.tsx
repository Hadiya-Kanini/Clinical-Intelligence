import { useEffect, useRef } from 'react'
import { useDispatch } from 'react-redux'
import { checkAuthAsync } from '../store/slices/authSlice'
import type { AppDispatch } from '../store'

/**
 * Component that checks authentication status on app initialization.
 * This should be rendered once at the app root to ensure auth state is properly loaded.
 */
export function AuthInitializer(): null {
  const dispatch = useDispatch<AppDispatch>()
  const hasChecked = useRef(false)

  useEffect(() => {
    // Check authentication status when the app loads, but only once
    if (!hasChecked.current) {
      hasChecked.current = true
      dispatch(checkAuthAsync())
    }
  }, []) // Empty dependency array - dispatch is stable and doesn't need to be included

  return null
}
