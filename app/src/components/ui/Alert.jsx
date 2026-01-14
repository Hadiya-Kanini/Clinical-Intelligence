import React from 'react'

export default function Alert({ children, variant = 'error', role }) {
  const computedRole = role || (variant === 'error' ? 'alert' : 'status')

  return (
    <div className={`ui-alert ui-alert--${variant}`} role={computedRole}>
      <div>{children}</div>
    </div>
  )
}
