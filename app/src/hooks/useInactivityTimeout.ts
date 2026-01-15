import { useCallback, useEffect, useRef } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'

/**
 * Default inactivity timeout in milliseconds (15 minutes).
 * Can be overridden via VITE_INACTIVITY_TIMEOUT_MS environment variable for testing.
 */
const DEFAULT_INACTIVITY_TIMEOUT_MS = 15 * 60 * 1000

/**
 * Get the configured inactivity timeout from environment or use default.
 * Allows test environments to use shorter timeouts.
 */
function getInactivityTimeout(): number {
  const envTimeout = import.meta.env.VITE_INACTIVITY_TIMEOUT_MS
  if (envTimeout) {
    const parsed = parseInt(envTimeout, 10)
    if (!isNaN(parsed) && parsed > 0) {
      return parsed
    }
  }
  return DEFAULT_INACTIVITY_TIMEOUT_MS
}

/**
 * Activity events that reset the inactivity timer.
 */
const ACTIVITY_EVENTS: (keyof WindowEventMap)[] = [
  'mousemove',
  'keydown',
  'click',
  'scroll',
  'touchstart',
  'mousedown',
]

export interface UseInactivityTimeoutOptions {
  /**
   * Callback fired when session expires due to inactivity.
   * If not provided, default behavior clears auth and navigates to login.
   */
  onTimeout?: () => void
  
  /**
   * Whether the hook is enabled. Set to false to disable timeout tracking.
   * @default true
   */
  enabled?: boolean
}

/**
 * Hook that tracks user inactivity and triggers session expiration after timeout.
 * 
 * Activity is tracked via DOM events (mousemove, keydown, click, scroll, touchstart)
 * and route changes. When the timeout triggers:
 * - Clears local auth indicators (ci_auth, ci_token, ci_user_role)
 * - Navigates to /login with session-expired state
 * 
 * @param options Configuration options for the hook
 */
export function useInactivityTimeout(options: UseInactivityTimeoutOptions = {}): void {
  const { onTimeout, enabled = true } = options
  const navigate = useNavigate()
  const location = useLocation()
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const lastActivityRef = useRef<number>(Date.now())

  /**
   * Clear local auth state and navigate to login with expired state.
   */
  const handleSessionExpired = useCallback(() => {
    // Clear local auth indicators
    try {
      localStorage.removeItem('ci_auth')
      localStorage.removeItem('ci_token')
      localStorage.removeItem('ci_user_role')
    } catch {
      // Ignore localStorage errors
    }

    if (onTimeout) {
      onTimeout()
    } else {
      // Default behavior: navigate to login with session-expired state
      navigate('/login', {
        replace: true,
        state: {
          logout: 'expired',
          from: location,
        },
      })
    }
  }, [navigate, location, onTimeout])

  /**
   * Reset the inactivity timer.
   */
  const resetTimer = useCallback(() => {
    lastActivityRef.current = Date.now()

    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }

    if (enabled) {
      const timeout = getInactivityTimeout()
      timeoutRef.current = setTimeout(handleSessionExpired, timeout)
    }
  }, [enabled, handleSessionExpired])

  /**
   * Handle activity events - reset the timer.
   */
  const handleActivity = useCallback(() => {
    resetTimer()
  }, [resetTimer])

  // Set up event listeners for activity tracking
  useEffect(() => {
    if (!enabled) {
      return
    }

    // Initial timer setup
    resetTimer()

    // Add event listeners for activity
    ACTIVITY_EVENTS.forEach((event) => {
      window.addEventListener(event, handleActivity, { passive: true })
    })

    // Cleanup
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }

      ACTIVITY_EVENTS.forEach((event) => {
        window.removeEventListener(event, handleActivity)
      })
    }
  }, [enabled, handleActivity, resetTimer])

  // Reset timer on route changes
  useEffect(() => {
    if (enabled) {
      resetTimer()
    }
  }, [location.pathname, enabled, resetTimer])
}

/**
 * Manually trigger activity reset from external sources (e.g., API calls).
 * This can be called by the API client to reset the inactivity timer on successful requests.
 */
export function resetInactivityTimer(): void {
  // Dispatch a custom event that the hook can listen to
  window.dispatchEvent(new CustomEvent('ci:activity'))
}

export default useInactivityTimeout
