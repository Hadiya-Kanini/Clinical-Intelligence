import type { ChangeEvent, FormEvent } from 'react'
import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import axios from 'axios'
import Alert from '../components/ui/Alert'
import Button from '../components/ui/Button'
import PasswordStrength from '../components/ui/PasswordStrength'
import TextField from '../components/ui/TextField'
import { getPasswordRequirements, getMissingRequirements, isPasswordValid } from '../lib/validation/passwordPolicy'

type Errors = {
  password?: string
  confirmPassword?: string
}

type Status = 'idle' | 'loading' | 'success' | 'tokenExpired' | 'tokenUsed' | 'error'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5062'

export default function ResetPasswordPage(): JSX.Element {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()

  const token = searchParams.get('token')

  const [password, setPassword] = useState<string>('')
  const [confirmPassword, setConfirmPassword] = useState<string>('')
  const [hasSubmitted, setHasSubmitted] = useState<boolean>(false)
  const [status, setStatus] = useState<Status>(token ? 'idle' : 'tokenExpired')
  const [errors, setErrors] = useState<Errors>({})

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

  const [errorMessage, setErrorMessage] = useState<string>('')
  const [countdown, setCountdown] = useState<number>(3)

  const passwordRequirements = useMemo(() => getPasswordRequirements(password), [password])

  useEffect(() => {
    if (status !== 'success') return

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
  }, [status, navigate])

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

    if (!token) {
      setStatus('tokenExpired')
      return
    }

    setHasSubmitted(true)

    const nextErrors = validate(password, confirmPassword, true)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) return

    setStatus('loading')

    try {
      await axios.post(`${API_BASE_URL}/api/v1/auth/reset-password`, {
        token,
        newPassword: password,
      })
      setStatus('success')
    } catch (error) {
      if (axios.isAxiosError(error)) {
        const errorCode = error.response?.data?.error?.code

        if (errorCode === 'token_expired') {
          setStatus('tokenExpired')
          setErrorMessage('Reset link has expired. Please request a new one.')
        } else if (errorCode === 'invalid_token') {
          setStatus('tokenUsed')
          setErrorMessage('This reset link has already been used or is invalid.')
        } else if (errorCode === 'password_requirements_not_met') {
          setStatus('error')
          const details = error.response?.data?.error?.details || []
          setErrorMessage(details.join(' '))
        } else {
          setStatus('error')
          setErrorMessage(error.response?.data?.error?.message || 'An error occurred. Please try again.')
        }
      } else {
        setStatus('error')
        setErrorMessage('Network error. Please check your connection and try again.')
      }
    }
  }

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

        {status === 'tokenExpired' ? (
          <Alert variant="error">
            This reset link is invalid or has expired.{' '}
            <Link className="ui-link" to="/forgot-password">
              Request a new one
            </Link>
          </Alert>
        ) : null}

        {status === 'tokenUsed' ? (
          <Alert variant="error">
            This reset link has already been used.{' '}
            <Link className="ui-link" to="/forgot-password">
              Request a new one
            </Link>
          </Alert>
        ) : null}

        {status === 'error' ? <Alert variant="error">{errorMessage}</Alert> : null}

        {status === 'success' ? (
          <Alert variant="success">Password updated. Redirecting to login in {countdown}…</Alert>
        ) : null}

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
          />

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Link className="ui-link" to="/login">
              Back to login
            </Link>
            <Button type="submit" disabled={status === 'loading' || status === 'success' || status === 'tokenExpired' || status === 'tokenUsed'}>
              {status === 'loading' ? 'Updating...' : 'Update password'}
            </Button>
          </div>
        </form>
      </section>
    </main>
  )
}
