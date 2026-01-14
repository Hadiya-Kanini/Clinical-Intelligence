import type { HTMLAttributes, ReactNode } from 'react'

type BadgeVariant = 'success' | 'warning' | 'error' | 'info' | 'neutral'

type BadgeProps = {
  children: ReactNode
  variant?: BadgeVariant
} & Omit<HTMLAttributes<HTMLSpanElement>, 'children'>

export default function Badge({ children, variant = 'neutral', ...props }: BadgeProps): JSX.Element {
  return (
    <span className={`ui-badge ui-badge--${variant}`} {...props}>
      {children}
    </span>
  )
}
