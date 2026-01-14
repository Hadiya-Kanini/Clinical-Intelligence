import type { HTMLAttributes } from 'react'

type ProgressBarProps = {
  value: number
  max?: number
  label?: string
} & Omit<HTMLAttributes<HTMLDivElement>, 'children'>

export default function ProgressBar({ value, max = 100, label, ...props }: ProgressBarProps): JSX.Element {
  const safeMax = max <= 0 ? 100 : max
  const percent = Math.max(0, Math.min(100, (value / safeMax) * 100))

  return (
    <div className="ui-progress" {...props}>
      <div className="ui-progress__track">
        <div className="ui-progress__fill" style={{ width: `${percent}%` }} />
      </div>
      {label ? <div className="ui-progress__label">{label}</div> : null}
    </div>
  )
}
