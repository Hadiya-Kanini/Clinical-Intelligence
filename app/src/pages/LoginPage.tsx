import type { ChangeEvent, FormEvent } from 'react'
import { useEffect, useRef, useState } from 'react'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { isValidEmailRfc5322 } from '../lib/validation/email'

export default function LoginPage(): JSX.Element {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [fieldErrors, setFieldErrors] = useState({
    email: '',
    password: '',
  })
  const [sessionExpiredMessage, setSessionExpiredMessage] = useState('')
  const [logoutSuccessMessage, setLogoutSuccessMessage] = useState('')
  const [sessionInvalidatedMessage, setSessionInvalidatedMessage] = useState('')
  const [accountLockedMessage, setAccountLockedMessage] = useState('')
  const [rateLimitMessage, setRateLimitMessage] = useState('')
  const [lockoutUnlockTime, setLockoutUnlockTime] = useState<Date | null>(null)
  const [lockoutCountdown, setLockoutCountdown] = useState('')

  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams()
  const emailInputRef = useRef<HTMLInputElement>(null)
  const passwordInputRef = useRef<HTMLInputElement>(null)
  const lockoutTimerRef = useRef<NodeJS.Timeout | null>(null)

  useEffect(() => {
    const isAuthenticated = localStorage.getItem('ci_auth') === '1'
    if (isAuthenticated) {
      const from = location.state?.from?.pathname || '/dashboard'
      navigate(from, { replace: true })
    }
  }, [navigate, location])

  // Check for logout redirect state (success or expired)
  useEffect(() => {
    const logoutReason = location.state?.logout
    if (logoutReason === 'expired') {
      setSessionExpiredMessage('Your session has expired due to inactivity. Please log in again.')
      setLogoutSuccessMessage('')
      setSessionInvalidatedMessage('')
      // Clear the state to prevent message from showing on refresh
      window.history.replaceState({}, document.title)
    } else if (logoutReason === 'success') {
      setLogoutSuccessMessage('You have been successfully logged out.')
      setSessionExpiredMessage('')
      setSessionInvalidatedMessage('')
      // Clear the state to prevent message from showing on refresh
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  // Check for session invalidated from URL query parameter (redirected by axios interceptor)
  useEffect(() => {
    const authParam = searchParams.get('auth')
    if (authParam === 'session_invalidated') {
      setSessionInvalidatedMessage('Your session was ended because you signed in on another device.')
      setSessionExpiredMessage('')
      setLogoutSuccessMessage('')
      // Clear the query parameter to prevent message from showing on refresh
      setSearchParams({}, { replace: true })
    } else if (authParam === 'expired') {
      setSessionExpiredMessage('Your session has expired. Please log in again.')
      setSessionInvalidatedMessage('')
      setLogoutSuccessMessage('')
      // Clear the query parameter to prevent message from showing on refresh
      setSearchParams({}, { replace: true })
    }
  }, [searchParams, setSearchParams])

  useEffect(() => {
    if (emailInputRef.current) {
      emailInputRef.current.focus()
    }
  }, [])

  // Lockout countdown timer with auto-retry enablement (UXR-009)
  useEffect(() => {
    if (!lockoutUnlockTime) {
      if (lockoutTimerRef.current) {
        clearInterval(lockoutTimerRef.current)
        lockoutTimerRef.current = null
      }
      return
    }

    const updateCountdown = () => {
      const now = new Date()
      const diff = lockoutUnlockTime.getTime() - now.getTime()

      if (diff <= 0) {
        // Lockout expired - enable retry without refresh
        setLockoutCountdown('')
        setAccountLockedMessage('')
        setLockoutUnlockTime(null)
        if (lockoutTimerRef.current) {
          clearInterval(lockoutTimerRef.current)
          lockoutTimerRef.current = null
        }
        return
      }

      const minutes = Math.floor(diff / 60000)
      const seconds = Math.floor((diff % 60000) / 1000)

      if (minutes > 0) {
        setLockoutCountdown(`${minutes}m ${seconds}s`)
      } else {
        setLockoutCountdown(`${seconds}s`)
      }
    }

    // Initial update
    updateCountdown()

    // Set interval for countdown updates
    lockoutTimerRef.current = setInterval(updateCountdown, 1000)

    // Cleanup on unmount or when lockoutUnlockTime changes
    return () => {
      if (lockoutTimerRef.current) {
        clearInterval(lockoutTimerRef.current)
        lockoutTimerRef.current = null
      }
    }
  }, [lockoutUnlockTime])

  const handleInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    
    if (name === 'email') {
      setEmail(value)
    } else if (name === 'password') {
      setPassword(value)
    }
    
    // Clear field error when user starts typing
    if (fieldErrors[name as keyof typeof fieldErrors]) {
      setFieldErrors(prev => ({ ...prev, [name]: '' }))
    }
    
    // Clear global error when user starts typing
    if (error) {
      setError('')
    }
    
    // Clear logout/session messages when user starts typing
    if (logoutSuccessMessage) {
      setLogoutSuccessMessage('')
    }
    if (sessionExpiredMessage) {
      setSessionExpiredMessage('')
    }
    if (sessionInvalidatedMessage) {
      setSessionInvalidatedMessage('')
    }
    if (accountLockedMessage) {
      setAccountLockedMessage('')
      setLockoutUnlockTime(null)
    }
    if (rateLimitMessage) {
      setRateLimitMessage('')
    }
  }

  const validateForm = (): boolean => {
    const errors = { email: '', password: '' }
    let isValid = true

    // Email validation using RFC 5322 compliant validator
    if (!email) {
      errors.email = 'Email is required'
      isValid = false
    } else if (!isValidEmailRfc5322(email)) {
      errors.email = 'Please enter a valid email address'
      isValid = false
    }

    // Password validation
    if (!password) {
      errors.password = 'Password is required'
      isValid = false
    }

    setFieldErrors(errors)
    return isValid
  }

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (!validateForm()) {
      return
    }

    setIsLoading(true)
    setError('')

    try {
      const response = await fetch('/api/v1/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      })

      if (!response.ok) {
        const errorData = await response.json()
        
        // Handle HTTP 429 rate limiting with Retry-After header (UXR-010)
        if (response.status === 429) {
          const retryAfterHeader = response.headers.get('Retry-After')
          let retryMessage = 'Too many login attempts.'
          
          if (retryAfterHeader) {
            // Parse Retry-After header (seconds or HTTP date format)
            const retrySeconds = parseInt(retryAfterHeader, 10)
            
            if (!isNaN(retrySeconds)) {
              // Seconds format (e.g., "60")
              if (retrySeconds < 60) {
                retryMessage = `Too many login attempts. Try again in ${retrySeconds} seconds.`
              } else if (retrySeconds < 3600) {
                const minutes = Math.ceil(retrySeconds / 60)
                retryMessage = `Too many login attempts. Try again in ${minutes} minute${minutes > 1 ? 's' : ''}.`
              } else {
                const retryTime = new Date(Date.now() + retrySeconds * 1000)
                retryMessage = `Too many login attempts. Try again at ${retryTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}.`
              }
            } else {
              // HTTP date format (e.g., "Wed, 21 Oct 2015 07:28:00 GMT")
              const retryDate = new Date(retryAfterHeader)
              if (!isNaN(retryDate.getTime())) {
                retryMessage = `Too many login attempts. Try again at ${retryDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}.`
              }
            }
          }
          
          setRateLimitMessage(retryMessage)
          setError('')
          return
        }
        
        // Handle account_locked error with unlock timeframe and countdown (UXR-009)
        if (errorData.error?.code === 'account_locked' && errorData.error?.details) {
          const details = errorData.error.details as string[]
          let unlockDate: Date | null = null
          
          // Parse unlock_at from details
          const unlockAtDetail = details.find((d: string) => d.startsWith('unlock_at:'))
          if (unlockAtDetail) {
            const unlockAtStr = unlockAtDetail.replace('unlock_at:', '')
            unlockDate = new Date(unlockAtStr)
          } else {
            // Fallback to remaining_seconds if unlock_at not present
            const remainingDetail = details.find((d: string) => d.startsWith('remaining_seconds:'))
            if (remainingDetail) {
              const remainingSeconds = parseInt(remainingDetail.replace('remaining_seconds:', ''), 10)
              unlockDate = new Date(Date.now() + remainingSeconds * 1000)
            }
          }
          
          if (unlockDate && !isNaN(unlockDate.getTime())) {
            setLockoutUnlockTime(unlockDate)
            setAccountLockedMessage('Your account is temporarily locked due to too many failed login attempts.')
          } else {
            setAccountLockedMessage('Your account is temporarily locked. Please try again later.')
          }
          setError('')
          return
        }
        
        throw new Error(errorData.error?.message || 'Login failed')
      }

      const data = await response.json()
      
      // Store authentication state
      localStorage.setItem('ci_auth', '1')
      localStorage.setItem('ci_token', data.token)
      localStorage.setItem('ci_user_role', data.user?.role || 'standard')
      
      // Redirect to intended page
      const from = location.state?.from?.pathname || '/dashboard'
      navigate(from, { replace: true })
      
    } catch (err: any) {
      setError(err.message || 'Login failed')
    } finally {
      setIsLoading(false)
    }
  }

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword)
  }

  return (
    <main
      style={{
        minHeight: '100vh',
        padding: 'var(--space-6)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <div className="login-container">
        <div className="logo-section">
          <div className="logo">
            <svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2L2 7v10c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V7l-10-5zm0 18c-3.31 0-6-2.69-6-6s2.69-6 6-6 6 2.69 6 6-2.69 6-6 6zm-1-9h2v2h-2v-2zm0 4h2v2h-2v-2z" />
            </svg>
          </div>
          <h1>Clinical Intelligence Platform</h1>
          <p className="subtitle">Secure Healthcare Data Management</p>
        </div>

        <form onSubmit={handleSubmit} aria-label="Login form">
          <div className="form-group">
            <label htmlFor="email">
              Email Address <span className="required" aria-label="required">*</span>
            </label>
            <input
              ref={emailInputRef}
              type="email"
              id="email"
              name="email"
              value={email}
              onChange={handleInputChange}
              placeholder="your.email@hospital.com"
              required
              aria-required="true"
              aria-describedby={fieldErrors.email ? 'email-error' : undefined}
              autoComplete="email"
              className={fieldErrors.email ? 'error' : ''}
            />
            {fieldErrors.email && (
              <div id="email-error" className="error-message visible" role="alert">
                {fieldErrors.email}
              </div>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="password">
              Password <span className="required" aria-label="required">*</span>
            </label>
            <div className="password-wrapper">
              <input
                ref={passwordInputRef}
                type={showPassword ? 'text' : 'password'}
                id="password"
                name="password"
                value={password}
                onChange={handleInputChange}
                placeholder="Enter your password"
                required
                aria-required="true"
                aria-describedby={fieldErrors.password ? 'password-error' : undefined}
                autoComplete="current-password"
                className={fieldErrors.password ? 'error' : ''}
              />
              <button
                type="button"
                className="toggle-password"
                aria-label="Toggle password visibility"
                onClick={togglePasswordVisibility}
              >
                {showPassword ? 'Hide' : 'Show'}
              </button>
            </div>
            {fieldErrors.password && (
              <div id="password-error" className="error-message visible" role="alert">
                {fieldErrors.password}
              </div>
            )}
          </div>

          {logoutSuccessMessage && (
            <div className="info-message logout-success visible" role="status" aria-live="polite">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor" aria-hidden="true">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
              </svg>
              <span>{logoutSuccessMessage}</span>
            </div>
          )}

          {sessionExpiredMessage && (
            <div className="info-message session-expired visible" role="status" aria-live="polite">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor" aria-hidden="true">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
              </svg>
              <span>{sessionExpiredMessage}</span>
            </div>
          )}

          {sessionInvalidatedMessage && (
            <div className="info-message session-invalidated visible" role="status" aria-live="polite">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor" aria-hidden="true">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
              </svg>
              <span>{sessionInvalidatedMessage}</span>
            </div>
          )}

          {rateLimitMessage && (
            <div className="error-message rate-limited visible" role="alert" aria-live="assertive">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor" aria-hidden="true">
                <path d="M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z"/>
              </svg>
              <div className="error-content">
                <span>{rateLimitMessage}</span>
                <span className="support-link">If you need assistance, please <a href="mailto:support@hospital.com">contact support</a>.</span>
              </div>
            </div>
          )}

          {accountLockedMessage && (
            <div className="error-message account-locked visible" role="alert" aria-live="assertive">
              <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor" aria-hidden="true">
                <path d="M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1 1.71 0 3.1 1.39 3.1 3.1v2z"/>
              </svg>
              <div className="error-content">
                <span>{accountLockedMessage}</span>
                {lockoutCountdown && (
                  <span className="countdown" aria-live="polite">Time remaining: {lockoutCountdown}</span>
                )}
                <span className="support-link">If you need assistance, please <a href="mailto:support@hospital.com">contact support</a>.</span>
              </div>
            </div>
          )}

          {error && (
            <div className="error-message visible" role="alert">
              {error}
            </div>
          )}

          <button type="submit" className="button-primary" disabled={isLoading}>
            <span className="spinner" aria-hidden="true"></span>
            <span>{isLoading ? 'Signing in...' : 'Sign In'}</span>
          </button>

          <div className="forgot-password">
            <Link to="/forgot-password">Forgot your password?</Link>
          </div>
        </form>

        <div className="divider">
          <span>Security Information</span>
        </div>

        <div className="info-box">
          <strong>Account Security:</strong> After 5 failed login attempts, your account will be locked for 30 minutes. Contact your administrator if you need assistance.
        </div>
      </div>
    </main>
  )
}
