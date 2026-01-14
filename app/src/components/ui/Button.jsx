import React from 'react'

export default function Button({
  children,
  type = 'button',
  variant = 'primary',
  disabled = false,
  loading = false,
  onClick,
}) {
  const isDisabled = disabled || loading
  const className = `ui-button ui-button--${variant}`

  return (
    <button
      type={type}
      className={className}
      disabled={isDisabled}
      aria-disabled={isDisabled || undefined}
      aria-busy={loading || undefined}
      onClick={onClick}
    >
      {loading ? <span className="ui-button__spinner" aria-hidden="true" /> : null}
      <span>{children}</span>
    </button>
  )
}
