import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { vi } from 'vitest'
import { BrowserRouter } from 'react-router-dom'
import LoginPage from '../LoginPage.jsx'

// Mock the UI components to focus on LoginPage logic
vi.mock('../../components/ui/TextField.jsx', () => {
  return {
    default: function MockTextField({ label, helperText, error, errorText, id, name, ...props }) {
      const inputId = id || name
      const helperId = helperText && inputId ? `${inputId}-helper` : undefined
      const errorId = errorText && inputId ? `${inputId}-error` : undefined
      const describedBy = [helperId, errorId].filter(Boolean).join(' ') || undefined

      return (
        <div data-testid={`textfield-${label.toLowerCase()}`}>
          <label htmlFor={inputId}>{label}</label>
          <input
            {...props}
            id={inputId}
            name={name}
            aria-invalid={error || undefined}
            aria-describedby={describedBy}
          />
          {helperText ? <div id={helperId}>{helperText}</div> : null}
          {errorText ? <div id={errorId}>{errorText}</div> : null}
        </div>
      )
    },
  }
})

vi.mock('../../components/ui/Button.jsx', () => {
  return {
    default: function MockButton({ children, ...props }) {
      return <button {...props}>{children}</button>
    },
  }
})

function renderWithRouter(component) {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  )
}

describe('LoginPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  test('renders login form with required elements', () => {
    renderWithRouter(<LoginPage />)
    
    // Check branding
    expect(screen.getByText('Clinical Intelligence')).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'Clinical Intelligence' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Log in' })).toBeInTheDocument()
    expect(screen.getByText('Sign in with your work email to access clinical insights.')).toBeInTheDocument()
    
    // Check form fields
    expect(screen.getByTestId('textfield-email')).toBeInTheDocument()
    expect(screen.getByTestId('textfield-password')).toBeInTheDocument()
    
    // Check submit button
    const submitButton = screen.getByRole('button', { name: 'Log in' })
    expect(submitButton).toHaveAttribute('type', 'submit')
  })

  test('form submission calls backend login endpoint', async () => {
    fetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ token: 'test-token', expires_in: 900 }),
    })

    const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {})
    renderWithRouter(<LoginPage />)

    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'name@hospital.org' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'password' },
    })

    const submitButton = screen.getByRole('button', { name: 'Log in' })
    fireEvent.click(submitButton)

    await waitFor(() => expect(fetch).toHaveBeenCalled())
    expect(fetch).toHaveBeenCalledWith(
      '/api/v1/auth/login',
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
        }),
      })
    )

    await waitFor(() => expect(consoleSpy).toHaveBeenCalledWith('Login successful'))

    consoleSpy.mockRestore()
  })

  test('shows actionable authentication error message for invalid credentials', async () => {
    fetch.mockResolvedValue({
      ok: false,
      json: () =>
        Promise.resolve({
          error: { code: 'invalid_credentials', message: 'Invalid email or password.', details: [] },
        }),
    })

    renderWithRouter(<LoginPage />)

    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'name@hospital.org' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'wrong' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(
      await screen.findByText('Incorrect email or password. Please try again.')
    ).toBeInTheDocument()
  })

  test('shows lockout message when account is locked', async () => {
    fetch.mockResolvedValue({
      ok: false,
      json: () =>
        Promise.resolve({
          error: { code: 'account_locked', message: 'Account temporarily locked.', details: [] },
        }),
    })

    renderWithRouter(<LoginPage />)

    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'name@hospital.org' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(
      await screen.findByText('Your account is temporarily locked. Please try again later or contact support.')
    ).toBeInTheDocument()
  })

  test('shows retry guidance when rate limited', async () => {
    fetch.mockResolvedValue({
      ok: false,
      json: () =>
        Promise.resolve({
          error: { code: 'rate_limited', message: 'Too many requests.', details: [] },
        }),
    })

    renderWithRouter(<LoginPage />)

    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'name@hospital.org' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByText('Too many login attempts. Please wait and try again.')).toBeInTheDocument()
  })

  test('shows recoverable network error without losing entered email', async () => {
    fetch.mockRejectedValue(new Error('Network error'))

    renderWithRouter(<LoginPage />)

    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'name@hospital.org' },
    })
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'password' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(
      await screen.findByText('We could not reach the server. Please check your connection and try again.')
    ).toBeInTheDocument()

    expect(screen.getByLabelText('Email')).toHaveValue('name@hospital.org')
  })

  test('shows validation errors and accessible attributes on submit', async () => {
    renderWithRouter(<LoginPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(
      screen.getByText('Please correct the highlighted fields and try again.')
    ).toBeInTheDocument()
    expect(screen.getByText('Email is required.')).toBeInTheDocument()
    expect(screen.getByText('Password is required.')).toBeInTheDocument()

    const emailInput = screen.getByLabelText('Email')
    const passwordInput = screen.getByLabelText('Password')

    expect(emailInput).toHaveAttribute('aria-invalid', 'true')
    expect(emailInput.getAttribute('aria-describedby')).toContain('login-email-error')
    expect(passwordInput).toHaveAttribute('aria-invalid', 'true')
    expect(passwordInput.getAttribute('aria-describedby')).toContain('login-password-error')

    await waitFor(() => expect(emailInput).toHaveFocus())
  })

  test('keyboard navigation works correctly', async () => {
    renderWithRouter(<LoginPage />)
    const user = userEvent.setup()
    
    const emailField = screen.getByTestId('textfield-email').querySelector('input')
    const passwordField = screen.getByTestId('textfield-password').querySelector('input')
    const submitButton = screen.getByRole('button', { name: 'Log in' })
    
    // Tab navigation
    emailField.focus()
    expect(emailField).toHaveFocus()
    
    await user.tab() // Move to password field
    expect(passwordField).toHaveFocus()
    
    await user.tab() // Move to submit button
    expect(submitButton).toHaveFocus()
  })

  test('responsive layout elements are present', () => {
    renderWithRouter(<LoginPage />)
    
    // Check main container with responsive styles
    const main = screen.getByRole('main')
    expect(main).toBeInTheDocument()
    
    // Check card container
    const section = screen.getByRole('region', { name: 'Login' })
    expect(section).toBeInTheDocument()
  })

  test('form has proper accessibility attributes', () => {
    renderWithRouter(<LoginPage />)
    
    // Check email field
    const emailField = screen.getByTestId('textfield-email').querySelector('input')
    expect(emailField).toHaveAttribute('type', 'email')
    expect(emailField).toHaveAttribute('autoComplete', 'username')
    expect(emailField).toHaveAttribute('required')
    
    // Check password field
    const passwordField = screen.getByTestId('textfield-password').querySelector('input')
    expect(passwordField).toHaveAttribute('type', 'password')
    expect(passwordField).toHaveAttribute('autoComplete', 'current-password')
    expect(passwordField).toHaveAttribute('required')
    
    // Check submit button
    const submitButton = screen.getByRole('button', { name: 'Log in' })
    expect(submitButton).toHaveAttribute('type', 'submit')
  })

  test('hides broken logo image while keeping brand title visible', async () => {
    renderWithRouter(<LoginPage />)

    expect(screen.getByText('Clinical Intelligence')).toBeInTheDocument()

    const logo = screen.getByRole('img', { name: 'Clinical Intelligence' })
    fireEvent.error(logo)

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'Clinical Intelligence' })).not.toBeInTheDocument()
    })

    expect(screen.getByText('Clinical Intelligence')).toBeInTheDocument()
  })
})
