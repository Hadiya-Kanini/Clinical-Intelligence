/**
 * Centralized password complexity validation per FR-009c.
 * Aligned with backend PasswordPolicy.cs for consistent enforcement.
 */

export const PASSWORD_MIN_LENGTH = 8
export const PASSWORD_MAX_LENGTH = 128

export interface PasswordRequirement {
  id: string
  label: string
  met: boolean
}

/**
 * Evaluates password against all complexity requirements.
 * Returns array of requirement objects with met status for UI display.
 */
export function getPasswordRequirements(password: string): PasswordRequirement[] {
  return [
    {
      id: 'length',
      label: `At least ${PASSWORD_MIN_LENGTH} characters`,
      met: password.length >= PASSWORD_MIN_LENGTH,
    },
    {
      id: 'lowercase',
      label: 'Lowercase letter',
      met: /[a-z]/.test(password),
    },
    {
      id: 'uppercase',
      label: 'Uppercase letter',
      met: /[A-Z]/.test(password),
    },
    {
      id: 'number',
      label: 'Number',
      met: /\d/.test(password),
    },
    {
      id: 'special',
      label: 'Special character',
      met: /[^A-Za-z0-9]/.test(password),
    },
  ]
}

/**
 * Validates password against all complexity requirements.
 * @returns true if all requirements are met
 */
export function isPasswordValid(password: string): boolean {
  if (!password || password.length < PASSWORD_MIN_LENGTH) {
    return false
  }

  if (password.length > PASSWORD_MAX_LENGTH) {
    return false
  }

  return getPasswordRequirements(password).every((req) => req.met)
}

/**
 * Returns list of unmet requirement labels for error messaging.
 * Aligned with backend GetMissingRequirements for consistent error reporting.
 */
export function getMissingRequirements(password: string): string[] {
  const missing: string[] = []

  if (!password) {
    return [
      `Password must be at least ${PASSWORD_MIN_LENGTH} characters`,
      'Password must contain a lowercase letter',
      'Password must contain an uppercase letter',
      'Password must contain a number',
      'Password must contain a special character',
    ]
  }

  if (password.length < PASSWORD_MIN_LENGTH) {
    missing.push(`Password must be at least ${PASSWORD_MIN_LENGTH} characters`)
  }

  if (password.length > PASSWORD_MAX_LENGTH) {
    missing.push(`Password must not exceed ${PASSWORD_MAX_LENGTH} characters`)
  }

  if (!/[a-z]/.test(password)) {
    missing.push('Password must contain a lowercase letter')
  }

  if (!/[A-Z]/.test(password)) {
    missing.push('Password must contain an uppercase letter')
  }

  if (!/\d/.test(password)) {
    missing.push('Password must contain a number')
  }

  if (!/[^A-Za-z0-9]/.test(password)) {
    missing.push('Password must contain a special character')
  }

  return missing
}
