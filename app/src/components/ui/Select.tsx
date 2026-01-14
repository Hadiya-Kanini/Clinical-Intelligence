import type { ChangeEvent, ReactNode, SelectHTMLAttributes } from 'react'
import { useId } from 'react'

type SelectOption = {
  value: string
  label: ReactNode
}

type SelectProps = {
  id?: string
  label?: ReactNode
  value: string
  onChange: (e: ChangeEvent<HTMLSelectElement>) => void
  options: SelectOption[]
  helperText?: ReactNode
  error?: boolean
  errorText?: ReactNode
} & Omit<SelectHTMLAttributes<HTMLSelectElement>, 'id' | 'value' | 'onChange' | 'children'>

export default function Select({
  id,
  label,
  value,
  onChange,
  options,
  disabled = false,
  required = false,
  name,
  helperText,
  error = false,
  errorText,
  ...props
}: SelectProps): JSX.Element {
  const generatedId = useId()
  const inputId = id || name || generatedId
  const helperId = helperText && inputId ? `${inputId}-helper` : undefined
  const errorId = errorText && inputId ? `${inputId}-error` : undefined

  const describedBy = [helperId, errorId].filter(Boolean).join(' ') || undefined

  return (
    <div className={`ui-select${error ? ' is-error' : ''}`}>
      {label ? (
        <label className="ui-select__label" htmlFor={inputId}>
          {label}
        </label>
      ) : null}
      <select
        className="ui-select__input"
        id={inputId}
        name={name}
        value={value}
        onChange={onChange}
        disabled={disabled}
        required={required}
        aria-invalid={error || undefined}
        aria-describedby={describedBy}
        {...props}
      >
        {options.map((opt) => (
          <option key={String(opt.value)} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {helperText ? (
        <div className="ui-select__helper" id={helperId}>
          {helperText}
        </div>
      ) : null}
      {errorText ? (
        <div className="ui-select__error" id={errorId}>
          {errorText}
        </div>
      ) : null}
    </div>
  )
}
