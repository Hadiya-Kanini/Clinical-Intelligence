import type { DataStatus } from '../../lib/patientApi'
import Badge from './Badge'

type DataStatusBadgeProps = {
  status: DataStatus
}

const statusConfig: Record<DataStatus, { variant: 'success' | 'warning' | 'info'; label: string }> = {
  verified: { variant: 'success', label: 'Verified' },
  unverified: { variant: 'warning', label: 'Unverified' },
  modified: { variant: 'info', label: 'Modified' },
}

export default function DataStatusBadge({ status }: DataStatusBadgeProps): JSX.Element {
  const config = statusConfig[status] || statusConfig.unverified
  
  return (
    <Badge 
      variant={config.variant} 
      aria-label={`Data status: ${config.label}`}
      title={config.label}
    >
      {config.label}
    </Badge>
  )
}
