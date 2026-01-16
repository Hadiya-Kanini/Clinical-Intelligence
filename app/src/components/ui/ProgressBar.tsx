import type { HTMLAttributes } from 'react'

type ProgressBarVariant = 'default' | 'success' | 'error' | 'cancelled'

type ProgressBarProps = {
  value: number
  max?: number
  label?: string
  showPercentage?: boolean
  variant?: ProgressBarVariant
} & Omit<HTMLAttributes<HTMLDivElement>, 'children'>

export default function ProgressBar({ value, max = 100, label, showPercentage = false, variant = 'default', ...props }: ProgressBarProps): JSX.Element {
  const safeMax = max <= 0 ? 100 : max
  const percent = Math.max(0, Math.min(100, (value / safeMax) * 100))
  const roundedPercent = Math.round(percent)

  const variantClass = variant !== 'default' ? ` ui-progress--${variant}` : ''

  return (
    <div
      className={`ui-progress${variantClass}`}
      role="progressbar"
      aria-valuenow={roundedPercent}
      aria-valuemin={0}
      aria-valuemax={100}
      aria-label={label || `Progress: ${roundedPercent}%`}
      {...props}
    >
      <div className="ui-progress__header">
        {showPercentage ? (
          <span className="ui-progress__percentage" data-testid="progress-percentage">
            {roundedPercent}%
          </span>
        ) : null}
        {label ? <span className="ui-progress__label">{label}</span> : null}
      </div>
      <div className="ui-progress__track">
        <div className="ui-progress__fill" style={{ width: `${percent}%` }} />
      </div>
    </div>
  )
}
