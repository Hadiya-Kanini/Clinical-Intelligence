import type { ChangeEvent, FormEvent } from 'react'
import { useEffect, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import Button from '../components/ui/Button.tsx'
import TextField from '../components/ui/TextField.tsx'
import Alert from '../components/ui/Alert.tsx'
import { isValidEmailRfc5322 } from '../lib/validation/email'
import { api } from '../lib/apiClient'

type ForgotPasswordErrors = {
  email?: string
}

type LocationState = {
  email?: unknown
}

type Status = 'idle' | 'loading' | 'success' | 'error' | 'rate_limited'

/**
 * Format seconds into a human-readable timeframe string.
 * @param seconds - Number of seconds to format
 * @returns Formatted string like "5 minutes" or "1 hour"
 */
function formatRetryTimeframe(seconds: number): string {
  if (seconds >= 3600) {
    const hours = Math.ceil(seconds / 3600)
    return hours === 1 ? '1 hour' : `${hours} hours`
  }
  if (seconds >= 60) {
    const minutes = Math.ceil(seconds / 60)
    return minutes === 1 ? '1 minute' : `${minutes} minutes`
  }
  return seconds === 1 ? '1 second' : `${seconds} seconds`
}

export default function ForgotPasswordPage(): JSX.Element {
  const location = useLocation()
  const state = (location.state || {}) as LocationState

  const [email, setEmail] = useState<string>('')
  const [errors, setErrors] = useState<ForgotPasswordErrors>({})
  const [hasSubmitted, setHasSubmitted] = useState<boolean>(false)
  const [status, setStatus] = useState<Status>('idle')

  useEffect(() => {
    const stateEmail = state.email

    if (typeof stateEmail !== 'string') return
    if (!stateEmail.trim()) return

    setEmail((current) => (current ? current : stateEmail))
  }, [state.email])

  function validate(nextEmail: string): ForgotPasswordErrors {
    const nextErrors: ForgotPasswordErrors = {}
    const normalizedEmail = nextEmail.trim()

    if (!normalizedEmail) {
      nextErrors.email = 'Email is required.'
    } else if (!isValidEmailRfc5322(normalizedEmail)) {
      nextErrors.email = 'Enter a valid email address.'
    }

    return nextErrors
  }

  function handleEmailChange(e: ChangeEvent<HTMLInputElement>): void {
    const nextEmail = e.target.value
    setEmail(nextEmail)

    if (hasSubmitted) {
      setErrors(validate(nextEmail))
    }
  }

  const [errorMessage, setErrorMessage] = useState<string>('')

  async function handleSubmit(e: FormEvent<HTMLFormElement>): Promise<void> {
    e.preventDefault()
    setHasSubmitted(true)
    setErrorMessage('')

    const nextErrors = validate(email)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) {
      setStatus('idle')
      return
    }

    setStatus('loading')

    const result = await api.post('/api/v1/auth/forgot-password', {
      email: email.trim().toLowerCase(),
    }, { skipCsrf: true })

    if (result.success) {
      setStatus('success')
    } else {
      if (result.status === 429 || result.error.code === 'rate_limited') {
        setStatus('rate_limited')
        // Display retry timeframe from Retry-After header (UXR-010)
        const retrySeconds = result.retryAfterSeconds
        if (retrySeconds && retrySeconds > 0) {
          const timeframe = formatRetryTimeframe(retrySeconds)
          setErrorMessage(`Too many reset requests. Please try again in ${timeframe}.`)
        } else {
          setErrorMessage('Too many password reset requests. Please try again later.')
        }
      } else if (result.error.code === 'network_error') {
        setStatus('error')
        setErrorMessage('Network error. Please check your connection and try again.')
      } else {
        setStatus('error')
        setErrorMessage(result.error.message || 'An error occurred. Please try again.')
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
          maxWidth: 420,
          marginTop: 'var(--space-12)',
          background: 'var(--color-surface)',
          border: '1px solid var(--color-border)',
          borderRadius: 'var(--radius-lg)',
          boxShadow: 'var(--elevation-1)',
          padding: 'var(--space-6)',
        }}
        aria-label="Forgot password"
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
            Forgot password
          </h1>
          <p style={{ margin: 0, color: 'var(--color-text-muted)' }}>Enter your email and we'll send a reset link.</p>
        </header>

        <form
          onSubmit={handleSubmit}
          noValidate
          style={{
            display: 'flex',
            flexDirection: 'column',
            gap: 'var(--space-4)',
          }}
        >
          {status === 'success' ? (
            <Alert variant="success">If an account exists for that email, we'll send a password reset link.</Alert>
          ) : null}

          {status === 'error' || status === 'rate_limited' ? (
            <Alert variant="error">{errorMessage}</Alert>
          ) : null}

          <TextField
            id="forgot-password-email"
            label="Email"
            name="email"
            type="email"
            placeholder="name@hospital.org"
            autoComplete="username"
            required
            value={email}
            onChange={handleEmailChange}
            error={Boolean(errors.email)}
            errorText={errors.email}
          />

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Link className="ui-link" to="/login" state={{ email: email.trim() || undefined }}>
              Back to login
            </Link>
            <Button type="submit" disabled={status === 'loading' || status === 'success'}>
              {status === 'loading' ? 'Sending...' : 'Send reset link'}
            </Button>
          </div>
        </form>
      </section>
    </main>
  )
}
