import React, { useState } from 'react'
import Button from '../components/ui/Button.jsx'
import TextField from '../components/ui/TextField.jsx'
import Alert from '../components/ui/Alert.jsx'
import logoUrl from '../assets/logo.svg'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState({})
  const [submitError, setSubmitError] = useState('')
  const [hasSubmitted, setHasSubmitted] = useState(false)
  const [isLogoVisible, setIsLogoVisible] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)

  function validate({ nextEmail, nextPassword }) {
    const nextErrors = {}
    const normalizedEmail = nextEmail.trim()

    if (!normalizedEmail) {
      nextErrors.email = 'Email is required.'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalizedEmail)) {
      nextErrors.email = 'Enter a valid email address.'
    }

    if (!nextPassword) {
      nextErrors.password = 'Password is required.'
    }

    return nextErrors
  }

  function handleEmailChange(e) {
    const nextEmail = e.target.value
    setEmail(nextEmail)

    if (hasSubmitted) {
      setErrors(validate({ nextEmail, nextPassword: password }))
    }
  }

  function handlePasswordChange(e) {
    const nextPassword = e.target.value
    setPassword(nextPassword)

    if (hasSubmitted) {
      setErrors(validate({ nextEmail: email, nextPassword }))
    }
  }

  function focusFirstError(nextErrors) {
    const firstInvalidId = nextErrors.email ? 'login-email' : nextErrors.password ? 'login-password' : null

    if (!firstInvalidId) return

    const schedule = typeof requestAnimationFrame === 'function' ? requestAnimationFrame : (cb) => setTimeout(cb, 0)

    schedule(() => {
      const el = document.getElementById(firstInvalidId)
      if (el && typeof el.focus === 'function') el.focus()
    })
  }

  function handleSubmit(e) {
    e.preventDefault()

    if (isSubmitting) return
    setHasSubmitted(true)

    const nextErrors = validate({ nextEmail: email, nextPassword: password })
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) {
      setSubmitError('Please correct the highlighted fields and try again.')
      focusFirstError(nextErrors)
      return
    }

    setSubmitError('')
    setIsSubmitting(true)

    fetch('/api/v1/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: email.trim(),
        password,
      }),
    })
      .then(async (res) => {
        if (res.ok) {
          await res.json().catch(() => null)
          console.log('Login successful')
          return
        }

        const payload = await res.json().catch(() => null)
        const code = payload?.error?.code

        if (code === 'invalid_credentials') {
          setSubmitError('Incorrect email or password. Please try again.')
          return
        }

        if (code === 'account_locked') {
          setSubmitError('Your account is temporarily locked. Please try again later or contact support.')
          return
        }

        if (code === 'rate_limited') {
          setSubmitError('Too many login attempts. Please wait and try again.')
          return
        }

        setSubmitError('Unable to log in right now. Please try again.')
      })
      .catch(() => {
        setSubmitError('We could not reach the server. Please check your connection and try again.')
      })
      .finally(() => {
        setIsSubmitting(false)
      })
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
        aria-label="Login"
      >
        <header style={{ marginBottom: 'var(--space-6)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', margin: '0 0 var(--space-2) 0' }}>
            {isLogoVisible ? (
              <img
                src={logoUrl}
                alt="Clinical Intelligence"
                width={28}
                height={28}
                onError={() => setIsLogoVisible(false)}
                style={{ display: 'block', flex: '0 0 auto' }}
              />
            ) : null}
            <div
              style={{
                fontSize: 'var(--font-size-h4)',
                fontWeight: 'var(--font-weight-h4)',
                lineHeight: 'var(--line-height-h4)',
                margin: 0,
                color: 'var(--color-text)',
              }}
            >
              Clinical Intelligence
            </div>
          </div>
          <h1
            style={{
              fontSize: 'var(--font-size-h2)',
              fontWeight: 'var(--font-weight-h2)',
              lineHeight: 'var(--line-height-h2)',
              margin: '0 0 var(--space-2) 0',
            }}
          >
            Log in
          </h1>
          <p style={{ margin: 0, color: 'var(--color-text-muted)' }}>
            Sign in with your work email to access clinical insights.
          </p>
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
          {submitError ? <Alert>{submitError}</Alert> : null}

          <TextField
            id="login-email"
            label="Email"
            name="email"
            type="email"
            placeholder="name@hospital.org"
            helperText="Use your work email."
            autoComplete="username"
            required
            value={email}
            onChange={handleEmailChange}
            error={Boolean(errors.email)}
            errorText={errors.email}
          />

          <TextField
            id="login-password"
            label="Password"
            name="password"
            type="password"
            placeholder="Enter your password"
            autoComplete="current-password"
            required
            value={password}
            onChange={handlePasswordChange}
            error={Boolean(errors.password)}
            errorText={errors.password}
          />

          <div
            style={{
              display: 'flex',
              justifyContent: 'flex-end',
              marginTop: 'var(--space-2)',
            }}
          >
            <Button type="submit" loading={isSubmitting}>
              Log in
            </Button>
          </div>
        </form>
      </section>
    </main>
  )
}
