import type { ChangeEvent, FormEvent } from 'react'
import { useEffect, useRef, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'

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

  const navigate = useNavigate()
  const location = useLocation()
  const emailInputRef = useRef<HTMLInputElement>(null)
  const passwordInputRef = useRef<HTMLInputElement>(null)

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
      // Clear the state to prevent message from showing on refresh
      window.history.replaceState({}, document.title)
    } else if (logoutReason === 'success') {
      setLogoutSuccessMessage('You have been successfully logged out.')
      setSessionExpiredMessage('')
      // Clear the state to prevent message from showing on refresh
      window.history.replaceState({}, document.title)
    }
  }, [location.state])

  useEffect(() => {
    if (emailInputRef.current) {
      emailInputRef.current.focus()
    }
  }, [])

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
  }

  const validateForm = (): boolean => {
    const errors = { email: '', password: '' }
    let isValid = true

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!email) {
      errors.email = 'Email is required'
      isValid = false
    } else if (!emailRegex.test(email)) {
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
