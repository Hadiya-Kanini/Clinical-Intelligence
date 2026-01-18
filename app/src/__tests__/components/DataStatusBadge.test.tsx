import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import DataStatusBadge from '../../components/ui/DataStatusBadge'

describe('DataStatusBadge', () => {
  it('renders verified status with success variant', () => {
    render(<DataStatusBadge status="verified" />)
    
    const badge = screen.getByText('Verified')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveClass('badge--success')
  })

  it('renders unverified status with warning variant', () => {
    render(<DataStatusBadge status="unverified" />)
    
    const badge = screen.getByText('Unverified')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveClass('badge--warning')
  })

  it('renders modified status with info variant', () => {
    render(<DataStatusBadge status="modified" />)
    
    const badge = screen.getByText('Modified')
    expect(badge).toBeInTheDocument()
    expect(badge).toHaveClass('badge--info')
  })

  it('capitalizes status text correctly', () => {
    const { rerender } = render(<DataStatusBadge status="verified" />)
    expect(screen.getByText('Verified')).toBeInTheDocument()
    
    rerender(<DataStatusBadge status="unverified" />)
    expect(screen.getByText('Unverified')).toBeInTheDocument()
    
    rerender(<DataStatusBadge status="modified" />)
    expect(screen.getByText('Modified')).toBeInTheDocument()
  })
})
