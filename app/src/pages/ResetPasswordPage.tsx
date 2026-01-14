import type { ChangeEvent, FormEvent } from 'react'
import { useMemo, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import Alert from '../components/ui/Alert'
import Button from '../components/ui/Button'
import PasswordStrength from '../components/ui/PasswordStrength'
import TextField from '../components/ui/TextField'

type Errors = {
  password?: string
  confirmPassword?: string
}

type Status = 'idle' | 'success' | 'tokenExpired'

export default function ResetPasswordPage(): JSX.Element {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()

  const token = searchParams.get('token')

  const [password, setPassword] = useState<string>('')
  const [confirmPassword, setConfirmPassword] = useState<string>('')
  const [hasSubmitted, setHasSubmitted] = useState<boolean>(false)
  const [status, setStatus] = useState<Status>(token ? 'idle' : 'tokenExpired')
  const [errors, setErrors] = useState<Errors>({})

  function validate(nextPassword: string, nextConfirm: string): Errors {
    const nextErrors: Errors = {}

    if (!nextPassword) {
      nextErrors.password = 'Password is required.'
    } else if (nextPassword.length < 8) {
      nextErrors.password = 'Password must be at least 8 characters.'
    }

    if (!nextConfirm) {
      nextErrors.confirmPassword = 'Please confirm your password.'
    } else if (nextPassword && nextConfirm !== nextPassword) {
      nextErrors.confirmPassword = 'Passwords do not match.'
    }

    return nextErrors
  }

  const passwordRequirements = useMemo(() => {
    return [
      { label: 'At least 8 characters', met: password.length >= 8 },
      { label: 'Lowercase letter', met: /[a-z]/.test(password) },
      { label: 'Uppercase letter', met: /[A-Z]/.test(password) },
      { label: 'Number', met: /\\d/.test(password) },
      { label: 'Symbol', met: /[^A-Za-z0-9]/.test(password) },
    ]
  }, [password])

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

    if (!token) {
      setStatus('tokenExpired')
      return
    }

    setHasSubmitted(true)

    const nextErrors = validate(password, confirmPassword)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) return

    setStatus('success')

    setTimeout(() => {
      navigate('/login', { replace: true, state: { reset: 'success' } })
    }, 800)
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
          <Alert variant="error">This reset link is invalid or has expired. Please request a new one.</Alert>
        ) : null}
        {status === 'success' ? <Alert variant="success">Password updated. Redirecting to login…</Alert> : null}

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

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)' }}>
            {passwordRequirements.map((req) => (
              <div
                key={req.label}
                style={{
                  fontSize: 'var(--font-size-body-small)',
                  color: req.met ? 'var(--color-success-dark)' : 'var(--color-text-muted)',
                }}
              >
                {req.met ? '✓' : '○'} {req.label}
              </div>
            ))}
          </div>

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
            <Button type="submit" disabled={status !== 'idle'}>
              Update password
            </Button>
          </div>
        </form>
      </section>
    </main>
  )
}
