import React, { useId } from 'react'

export default function TextField({
  id,
  label,
  value,
  onChange,
  type = 'text',
  placeholder,
  disabled = false,
  required = false,
  name,
  autoComplete,
  helperText,
  error = false,
  errorText,
}) {
  const generatedId = useId()
  const inputId = id || name || generatedId
  const helperId = helperText && inputId ? `${inputId}-helper` : undefined
  const errorId = errorText && inputId ? `${inputId}-error` : undefined

  const describedBy = [helperId, errorId].filter(Boolean).join(' ') || undefined

  return (
    <div className={`ui-textfield${error ? ' is-error' : ''}`}>
      {label ? (
        <label className="ui-textfield__label" htmlFor={inputId}>
          {label}
        </label>
      ) : null}
      <input
        className="ui-textfield__input"
        id={inputId}
        name={name}
        type={type}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        disabled={disabled}
        required={required}
        autoComplete={autoComplete}
        aria-invalid={error || undefined}
        aria-describedby={describedBy}
      />
      {helperText ? (
        <div className="ui-textfield__helper" id={helperId}>
          {helperText}
        </div>
      ) : null}
      {errorText ? (
        <div className="ui-textfield__error" id={errorId}>
          {errorText}
        </div>
      ) : null}
    </div>
  )
}
