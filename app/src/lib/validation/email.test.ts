import { describe, expect, it } from 'vitest'
import { isValidEmailRfc5322, normalizeEmail, validateEmailWithDetails } from './email'

describe('normalizeEmail', () => {
  it('should trim whitespace and convert to lowercase', () => {
    expect(normalizeEmail('  User@Domain.COM  ')).toBe('user@domain.com')
  })

  it('should return empty string for null', () => {
    expect(normalizeEmail(null)).toBe('')
  })

  it('should return empty string for undefined', () => {
    expect(normalizeEmail(undefined)).toBe('')
  })

  it('should return empty string for whitespace-only input', () => {
    expect(normalizeEmail('   ')).toBe('')
  })
})

describe('isValidEmailRfc5322', () => {
  describe('valid emails', () => {
    it('should accept standard email format', () => {
      expect(isValidEmailRfc5322('user@domain.com')).toBe(true)
    })

    it('should accept plus addressing (user+tag@domain.com)', () => {
      expect(isValidEmailRfc5322('user+tag@domain.com')).toBe(true)
    })

    it('should accept dots in local part (first.last@domain.com)', () => {
      expect(isValidEmailRfc5322('first.last@domain.com')).toBe(true)
    })

    it('should accept subdomains (user@sub.domain.com)', () => {
      expect(isValidEmailRfc5322('user@sub.domain.com')).toBe(true)
    })

    it('should accept hyphens in domain (user@my-domain.com)', () => {
      expect(isValidEmailRfc5322('user@my-domain.com')).toBe(true)
    })

    it('should accept numeric local part', () => {
      expect(isValidEmailRfc5322('123@domain.com')).toBe(true)
    })

    it('should accept long TLDs', () => {
      expect(isValidEmailRfc5322('user@domain.healthcare')).toBe(true)
    })

    it('should accept multiple plus signs', () => {
      expect(isValidEmailRfc5322('user+tag+another@domain.com')).toBe(true)
    })

    it('should accept underscores in local part', () => {
      expect(isValidEmailRfc5322('user_name@domain.com')).toBe(true)
    })

    it('should accept mixed case (normalized)', () => {
      expect(isValidEmailRfc5322('User@Domain.COM')).toBe(true)
    })
  })

  describe('invalid emails', () => {
    it('should reject missing @ symbol', () => {
      expect(isValidEmailRfc5322('userdomain.com')).toBe(false)
    })

    it('should reject missing domain', () => {
      expect(isValidEmailRfc5322('user@')).toBe(false)
    })

    it('should reject missing local part', () => {
      expect(isValidEmailRfc5322('@domain.com')).toBe(false)
    })

    it('should reject whitespace in email', () => {
      expect(isValidEmailRfc5322('user @domain.com')).toBe(false)
    })

    it('should reject trailing dot in domain', () => {
      expect(isValidEmailRfc5322('user@domain.com.')).toBe(false)
    })

    it('should reject leading dot in local part', () => {
      expect(isValidEmailRfc5322('.user@domain.com')).toBe(false)
    })

    it('should reject trailing dot in local part', () => {
      expect(isValidEmailRfc5322('user.@domain.com')).toBe(false)
    })

    it('should reject consecutive dots', () => {
      expect(isValidEmailRfc5322('user..name@domain.com')).toBe(false)
    })

    it('should reject missing TLD', () => {
      expect(isValidEmailRfc5322('user@domain')).toBe(false)
    })

    it('should reject single character TLD', () => {
      expect(isValidEmailRfc5322('user@domain.c')).toBe(false)
    })

    it('should reject null', () => {
      expect(isValidEmailRfc5322(null)).toBe(false)
    })

    it('should reject undefined', () => {
      expect(isValidEmailRfc5322(undefined)).toBe(false)
    })

    it('should reject empty string', () => {
      expect(isValidEmailRfc5322('')).toBe(false)
    })

    it('should reject whitespace-only string', () => {
      expect(isValidEmailRfc5322('   ')).toBe(false)
    })

    it('should reject email exceeding 254 characters', () => {
      const longEmail = 'a'.repeat(250) + '@b.com'
      expect(isValidEmailRfc5322(longEmail)).toBe(false)
    })

    it('should reject local part exceeding 64 characters', () => {
      const longLocalPart = 'a'.repeat(65) + '@domain.com'
      expect(isValidEmailRfc5322(longLocalPart)).toBe(false)
    })

    it('should reject dot before @ symbol', () => {
      expect(isValidEmailRfc5322('user.@domain.com')).toBe(false)
    })

    it('should reject dot after @ symbol', () => {
      expect(isValidEmailRfc5322('user@.domain.com')).toBe(false)
    })
  })
})

describe('validateEmailWithDetails', () => {
  it('should return error for empty email', () => {
    const result = validateEmailWithDetails('')
    expect(result.isValid).toBe(false)
    expect(result.errorMessage).toBe('Email is required.')
  })

  it('should return error for invalid format', () => {
    const result = validateEmailWithDetails('invalid-email')
    expect(result.isValid).toBe(false)
    expect(result.errorMessage).toBe('Please enter a valid email address.')
    expect(result.normalizedEmail).toBe('invalid-email')
  })

  it('should return success for valid email', () => {
    const result = validateEmailWithDetails('user+tag@domain.com')
    expect(result.isValid).toBe(true)
    expect(result.errorMessage).toBeNull()
    expect(result.normalizedEmail).toBe('user+tag@domain.com')
  })

  it('should normalize email in result', () => {
    const result = validateEmailWithDetails('  USER@DOMAIN.COM  ')
    expect(result.isValid).toBe(true)
    expect(result.normalizedEmail).toBe('user@domain.com')
  })
})
