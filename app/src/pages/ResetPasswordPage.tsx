import type { ChangeEvent, FormEvent } from 'react'
import { useEffect, useMemo, useState, useRef } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import Alert from '../components/ui/Alert'
import Button from '../components/ui/Button'
import PasswordStrength from '../components/ui/PasswordStrength'
import TextField from '../components/ui/TextField'
import { getPasswordRequirements, getMissingRequirements, isPasswordValid } from '../lib/validation/passwordPolicy'
import { api } from '../lib/apiClient'

type Errors = {
  password?: string
  confirmPassword?: string
}

type TokenStatus = 'validating' | 'valid' | 'invalid' | 'expired' | 'used' | 'missing'
type SubmitStatus = 'idle' | 'loading' | 'success' | 'error'

export default function ResetPasswordPage(): JSX.Element {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()

  const token = searchParams.get('token')

  const [password, setPassword] = useState<string>('')
  const [confirmPassword, setConfirmPassword] = useState<string>('')
  const [hasSubmitted, setHasSubmitted] = useState<boolean>(false)
  const [tokenStatus, setTokenStatus] = useState<TokenStatus>(token ? 'validating' : 'missing')
  const [submitStatus, setSubmitStatus] = useState<SubmitStatus>('idle')
  const [errors, setErrors] = useState<Errors>({})
  const [errorMessage, setErrorMessage] = useState<string>('')
  const [countdown, setCountdown] = useState<number>(3)
  const validationAttempted = useRef(false)

  const passwordRequirements = useMemo(() => getPasswordRequirements(password), [password])

  function validate(nextPassword: string, nextConfirm: string, checkAllRequirements = false): Errors {
    const nextErrors: Errors = {}

    if (!nextPassword) {
      nextErrors.password = 'Password is required.'
    } else if (checkAllRequirements && !isPasswordValid(nextPassword)) {
      const missing = getMissingRequirements(nextPassword)
      nextErrors.password = missing.length > 0 ? missing.join('. ') + '.' : 'Password does not meet requirements.'
    }

    if (!nextConfirm) {
      nextErrors.confirmPassword = 'Please confirm your password.'
    } else if (nextPassword && nextConfirm !== nextPassword) {
      nextErrors.confirmPassword = 'Passwords do not match.'
    }

    return nextErrors
  }

  useEffect(() => {
    if (!token || validationAttempted.current) return

    validationAttempted.current = true

    async function validateToken(): Promise<void> {
      const result = await api.get<{ valid: boolean; expiresAt?: string }>(
        `/api/v1/auth/reset-password/validate?token=${encodeURIComponent(token!)}`,
        { skipCsrf: true }
      )

      if (result.success && result.data.valid) {
        setTokenStatus('valid')
      } else if (!result.success) {
        const errorCode = result.error.code
        if (errorCode === 'token_expired') {
          setTokenStatus('expired')
        } else if (errorCode === 'token_used') {
          setTokenStatus('used')
        } else {
          setTokenStatus('invalid')
        }
      }
    }

    validateToken()
  }, [token])

  useEffect(() => {
    if (submitStatus !== 'success') return

    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer)
          navigate('/login', { replace: true, state: { reset: 'success' } })
          return 0
        }
        return prev - 1
      })
    }, 1000)

    return () => clearInterval(timer)
  }, [submitStatus, navigate])

  function handlePasswordChange(e: ChangeEvent<HTMLInputElement>): void {
    const next = e.target.value
    setPassword(next)

    if (hasSubmitted) {
      setErrors(validate(next, confirmPassword))
    }
  }

  function handleConfirmChange(e: ChangeEvent<HTMLInputElement>): void {
    const next = e.target.value
    setConfirmPassword(next)

    if (hasSubmitted) {
      setErrors(validate(password, next))
    }
  }

  async function handleSubmit(e: FormEvent<HTMLFormElement>): Promise<void> {
    e.preventDefault()
    setErrorMessage('')

    if (!token || tokenStatus !== 'valid') {
      return
    }

    setHasSubmitted(true)

    const nextErrors = validate(password, confirmPassword, true)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) return

    setSubmitStatus('loading')

    const result = await api.post<{ message: string }>(
      '/api/v1/auth/reset-password',
      { token, newPassword: password },
      { skipCsrf: true }
    )

    if (result.success) {
      setSubmitStatus('success')
    } else {
      const errorCode = result.error.code

      if (errorCode === 'token_expired') {
        setTokenStatus('expired')
        setErrorMessage('Reset link has expired. Please request a new one.')
      } else if (errorCode === 'token_used') {
        setTokenStatus('used')
        setErrorMessage('This reset link has already been used.')
      } else if (errorCode === 'invalid_token') {
        setTokenStatus('invalid')
        setErrorMessage('This reset link is invalid.')
      } else if (errorCode === 'password_requirements_not_met') {
        setSubmitStatus('error')
        const details = result.error.details || []
        setErrorMessage(details.join(' '))
      } else if (errorCode === 'network_error') {
        setSubmitStatus('error')
        setErrorMessage('Network error. Please check your connection and try again.')
      } else {
        setSubmitStatus('error')
        setErrorMessage(result.error.message || 'An error occurred. Please try again.')
      }
    }
  }

  const isTokenInvalid = tokenStatus === 'invalid' || tokenStatus === 'expired' || tokenStatus === 'used' || tokenStatus === 'missing'
  const isFormDisabled = submitStatus === 'loading' || submitStatus === 'success' || isTokenInvalid

  return (
    <main
      style={{
        minHeight: '100vh',
        padding: 'var(--space-6)',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'flex-start',
      }}
    >
      <section
        style={{
          width: '100%',
          maxWidth: 520,
          marginTop: 'var(--space-12)',
          background: 'var(--color-surface)',
          border: '1px solid var(--color-border)',
          borderRadius: 'var(--radius-lg)',
          boxShadow: 'var(--elevation-1)',
          padding: 'var(--space-6)',
        }}
        aria-label="Reset password"
      >
        <header style={{ marginBottom: 'var(--space-6)' }}>
          <h1
            style={{
              fontSize: 'var(--font-size-h2)',
              fontWeight: 'var(--font-weight-h2)',
              lineHeight: 'var(--line-height-h2)',
              margin: '0 0 var(--space-2) 0',
            }}
          >
            Reset password
          </h1>
          <p style={{ margin: 0, color: 'var(--color-text-muted)' }}>Choose a new password for your account.</p>
        </header>

        {tokenStatus === 'validating' ? (
          <Alert variant="info">Validating your reset link…</Alert>
        ) : null}

        {tokenStatus === 'missing' || tokenStatus === 'invalid' ? (
          <Alert variant="error">
            This reset link is invalid or has expired.{' '}
            <Link className="ui-link" to="/forgot-password">
              Request a new one
            </Link>
          </Alert>
        ) : null}

        {tokenStatus === 'expired' ? (
          <Alert variant="error">
            This reset link has expired.{' '}
            <Link className="ui-link" to="/forgot-password">
              Request a new one
            </Link>
          </Alert>
        ) : null}

        {tokenStatus === 'used' ? (
          <Alert variant="error">
            This reset link has already been used.{' '}
            <Link className="ui-link" to="/forgot-password">
              Request a new one
            </Link>
          </Alert>
        ) : null}

        {submitStatus === 'error' && tokenStatus === 'valid' ? (
          <Alert variant="error">{errorMessage}</Alert>
        ) : null}

        {submitStatus === 'success' ? (
          <Alert variant="success">Password updated. Redirecting to login in {countdown}…</Alert>
        ) : null}

        {tokenStatus === 'valid' && submitStatus !== 'success' ? (
          <form
            onSubmit={handleSubmit}
            noValidate
            style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)', marginTop: 'var(--space-4)' }}
          >
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 'var(--space-4)' }}>
              <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>Password strength</div>
              <PasswordStrength value={password} />
            </div>

            <TextField
              id="reset-password-new"
              label="New password"
              name="password"
              type="password"
              placeholder="Enter a new password"
              required
              value={password}
              onChange={handlePasswordChange}
              error={Boolean(errors.password)}
              errorText={errors.password}
              disabled={isFormDisabled}
            />

            <ul
              style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)', margin: 0, padding: 0, listStyle: 'none' }}
              aria-label="Password requirements"
            >
              {passwordRequirements.map((req) => (
                <li
                  key={req.id}
                  style={{
                    fontSize: 'var(--font-size-body-small)',
                    color: req.met ? 'var(--color-success-dark)' : 'var(--color-text-muted)',
                  }}
                  aria-label={`${req.label}: ${req.met ? 'met' : 'not met'}`}
                >
                  {req.met ? '✓' : '○'} {req.label}
                </li>
              ))}
            </ul>

            <TextField
              id="reset-password-confirm"
              label="Confirm password"
              name="confirmPassword"
              type="password"
              placeholder="Re-enter your new password"
              required
              value={confirmPassword}
              onChange={handleConfirmChange}
              error={Boolean(errors.confirmPassword)}
              errorText={errors.confirmPassword}
              disabled={isFormDisabled}
            />

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Link className="ui-link" to="/login">
                Back to login
              </Link>
              <Button type="submit" disabled={isFormDisabled}>
                {submitStatus === 'loading' ? 'Updating...' : 'Update password'}
              </Button>
            </div>
          </form>
        ) : null}

        {isTokenInvalid ? (
          <div style={{ marginTop: 'var(--space-4)', display: 'flex', justifyContent: 'center' }}>
            <Link className="ui-link" to="/login">
              Back to login
            </Link>
          </div>
        ) : null}
      </section>
    </main>
  )
}
