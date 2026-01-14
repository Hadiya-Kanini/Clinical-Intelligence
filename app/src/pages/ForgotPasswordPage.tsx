import type { ChangeEvent, FormEvent } from 'react'
import { useEffect, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import Button from '../components/ui/Button.tsx'
import TextField from '../components/ui/TextField.tsx'
import Alert from '../components/ui/Alert.tsx'

type ForgotPasswordErrors = {
  email?: string
}

type LocationState = {
  email?: unknown
}

type Status = 'idle' | 'success'

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
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalizedEmail)) {
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

  function handleSubmit(e: FormEvent<HTMLFormElement>): void {
    e.preventDefault()
    setHasSubmitted(true)

    const nextErrors = validate(email)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) {
      setStatus('idle')
      return
    }

    setStatus('success')
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
            <Button type="submit">Send reset link</Button>
          </div>
        </form>
      </section>
    </main>
  )
}
