---
trigger: model_decision
description: Enforces OWASP security best practices including secure coding, authentication, data protection, and vulnerability prevention. Covers access control, encryption, input validation, and secure configurations.
---

# Secure Coding and OWASP Guidelines

Secure by default. When in doubt, choose the more secure option.

## A01: Broken Access Control & A10: SSRF
- Principle of least privilege; deny by default
- Explicit permission checks for resources
- Validate incoming URLs with strict allow-lists
- Sanitize file paths to prevent traversal (e.g., `../../etc/passwd`)

## A02: Cryptographic Failures
- Strong hashing: Argon2 or bcrypt (not MD5/SHA-1)
- HTTPS for all network requests
- AES-256 for data at rest
- Load secrets from env vars or secret management (never hardcode)

## A03: Injection
- Parameterized queries only (no string concatenation)
- Built-in escaping for OS commands (e.g., shlex in Python)
- Context-aware output encoding for XSS prevention
- Use `.textContent` over `.innerHTML`; sanitize with DOMPurify when needed

## A05: Security Misconfiguration & A06: Vulnerable Components
- Disable verbose errors and debug in production
- Security headers: CSP, HSTS, X-Content-Type-Options
- Latest stable library versions
- Run vulnerability scanners (npm audit, pip-audit, Snyk)

## A07: Authentication Failures
- New session ID on login (prevent fixation)
- Session cookies: HttpOnly, Secure, SameSite=Strict
- Rate limiting and account lockout for auth endpoints

## A08: Software and Data Integrity Failures
- Validate before deserializing untrusted data
- Prefer JSON over Pickle; implement strict type checking

## General Guidelines
- Explicitly state security protections (e.g., "Parameterized query prevents SQL injection")
- Explain risks during code reviews