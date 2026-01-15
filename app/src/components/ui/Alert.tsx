import type { ReactNode } from 'react'

type AlertVariant = 'error' | 'success' | 'info' | 'warning'

type AlertProps = {
  children: ReactNode
  variant?: AlertVariant
  role?: string
}

export default function Alert({ children, variant = 'error', role }: AlertProps): JSX.Element {
  const computedRole = role || (variant === 'error' ? 'alert' : 'status')

  return (
    <div
      className={`ui-alert ui-alert--${variant}`}
      role={computedRole}
      aria-live={variant === 'error' ? 'assertive' : 'polite'}
      aria-atomic="true"
    >
      <div>{children}</div>
    </div>
  )
}
