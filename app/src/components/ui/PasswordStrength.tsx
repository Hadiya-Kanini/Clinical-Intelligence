import type { HTMLAttributes } from 'react'

type PasswordStrengthProps = {
  value: string
} & Omit<HTMLAttributes<HTMLDivElement>, 'children'>

type Strength = {
  label: 'Weak' | 'Medium' | 'Strong'
  score: 1 | 2 | 3
}

function getStrength(value: string): Strength {
  const hasLower = /[a-z]/.test(value)
  const hasUpper = /[A-Z]/.test(value)
  const hasNumber = /\d/.test(value)
  const hasSymbol = /[^A-Za-z0-9]/.test(value)

  const lengthScore = value.length >= 12 ? 2 : value.length >= 8 ? 1 : 0
  const varietyScore = [hasLower, hasUpper, hasNumber, hasSymbol].filter(Boolean).length

  const raw = lengthScore + varietyScore

  if (raw >= 5) return { label: 'Strong', score: 3 }
  if (raw >= 3) return { label: 'Medium', score: 2 }
  return { label: 'Weak', score: 1 }
}

export default function PasswordStrength({ value, ...props }: PasswordStrengthProps): JSX.Element {
  const strength = getStrength(value)

  return (
    <div className="ui-passwordStrength" {...props}>
      <div className="ui-passwordStrength__track" aria-hidden="true">
        <div className={`ui-passwordStrength__fill is-${strength.score}`} />
      </div>
      <div className={`ui-passwordStrength__label is-${strength.score}`}>{strength.label}</div>
    </div>
  )
}
