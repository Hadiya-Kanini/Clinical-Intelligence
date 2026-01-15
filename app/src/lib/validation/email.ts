/**
 * RFC 5322-compliant email validation utility.
 * Provides consistent email format validation across all frontend components.
 *
 * Supports:
 * - Standard emails (user@domain.com)
 * - Plus addressing (user+tag@domain.com)
 * - Subdomains (user@sub.domain.com)
 * - Dots in local part (first.last@domain.com)
 * - Hyphens in domain (user@my-domain.com)
 * - Numeric domains (user@123.com)
 *
 * IDN Handling: Accepts ASCII and punycode domains only.
 * Raw unicode domains are rejected for security and consistency.
 */

const RFC5322_EMAIL_REGEX =
  /^(?!.*\.\.)(?!.*\.$)(?!^\.)(?!.*@\.)(?!.*\.@)[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$/

/**
 * Normalizes an email address by trimming whitespace and converting to lowercase.
 * @param email - The email address to normalize.
 * @returns Normalized email address, or empty string if input is null/undefined/whitespace.
 */
export function normalizeEmail(email: string | null | undefined): string {
  if (!email || typeof email !== 'string') {
    return ''
  }
  return email.trim().toLowerCase()
}

/**
 * Validates whether the provided email address conforms to RFC 5322 format.
 * @param email - The email address to validate (will be normalized internally).
 * @returns True if the email format is valid; otherwise, false.
 */
export function isValidEmailRfc5322(email: string | null | undefined): boolean {
  const normalized = normalizeEmail(email)

  if (!normalized) {
    return false
  }

  if (normalized.length > 254) {
    return false
  }

  const atIndex = normalized.indexOf('@')
  if (atIndex < 0) {
    return false
  }

  const localPart = normalized.slice(0, atIndex)
  if (localPart.length > 64) {
    return false
  }

  return RFC5322_EMAIL_REGEX.test(normalized)
}

/**
 * Validation result type for detailed email validation.
 */
export type EmailValidationResult = {
  isValid: boolean
  normalizedEmail: string
  errorMessage: string | null
}

/**
 * Validates email format and returns a validation result with details.
 * @param email - The email address to validate.
 * @returns A validation result object containing isValid, normalizedEmail, and errorMessage.
 */
export function validateEmailWithDetails(email: string | null | undefined): EmailValidationResult {
  const normalized = normalizeEmail(email)

  if (!normalized) {
    return {
      isValid: false,
      normalizedEmail: '',
      errorMessage: 'Email is required.',
    }
  }

  if (!isValidEmailRfc5322(normalized)) {
    return {
      isValid: false,
      normalizedEmail: normalized,
      errorMessage: 'Please enter a valid email address.',
    }
  }

  return {
    isValid: true,
    normalizedEmail: normalized,
    errorMessage: null,
  }
}
