import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import Tooltip from '../../components/ui/Tooltip'

describe('Tooltip', () => {
  it('renders children correctly', () => {
    render(
      <Tooltip content="Test tooltip content">
        <button>Hover me</button>
      </Tooltip>
    )
    
    expect(screen.getByRole('button', { name: 'Hover me' })).toBeInTheDocument()
  })

  it('shows tooltip content on mouse enter', async () => {
    render(
      <Tooltip content="Test tooltip content">
        <button>Hover me</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button', { name: 'Hover me' })
    fireEvent.mouseEnter(trigger)
    
    expect(await screen.findByRole('tooltip')).toBeInTheDocument()
    expect(screen.getByText('Test tooltip content')).toBeInTheDocument()
  })

  it('hides tooltip content on mouse leave', async () => {
    render(
      <Tooltip content="Test tooltip content">
        <button>Hover me</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button', { name: 'Hover me' })
    fireEvent.mouseEnter(trigger)
    
    expect(await screen.findByRole('tooltip')).toBeInTheDocument()
    
    fireEvent.mouseLeave(trigger)
    
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument()
  })

  it('shows tooltip content on focus', async () => {
    render(
      <Tooltip content="Test tooltip content">
        <button>Focus me</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button', { name: 'Focus me' })
    fireEvent.focus(trigger)
    
    expect(await screen.findByRole('tooltip')).toBeInTheDocument()
  })

  it('hides tooltip content on blur', async () => {
    render(
      <Tooltip content="Test tooltip content">
        <button>Focus me</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button', { name: 'Focus me' })
    fireEvent.focus(trigger)
    
    expect(await screen.findByRole('tooltip')).toBeInTheDocument()
    
    fireEvent.blur(trigger)
    
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument()
  })

  it('hides tooltip on Escape key press', async () => {
    render(
      <Tooltip content="Test tooltip content">
        <button>Hover me</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button', { name: 'Hover me' })
    fireEvent.mouseEnter(trigger)
    
    expect(await screen.findByRole('tooltip')).toBeInTheDocument()
    
    fireEvent.keyDown(document, { key: 'Escape' })
    
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument()
  })

  it('applies correct position class', () => {
    const { rerender } = render(
      <Tooltip content="Test content" position="top">
        <button>Button</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button')
    fireEvent.mouseEnter(trigger)
    
    const tooltip = screen.getByRole('tooltip')
    expect(tooltip).toHaveStyle({ bottom: '100%' })
    
    rerender(
      <Tooltip content="Test content" position="bottom">
        <button>Button</button>
      </Tooltip>
    )
    
    fireEvent.mouseEnter(trigger)
    expect(screen.getByRole('tooltip')).toHaveStyle({ top: '100%' })
  })

  it('has correct ARIA attributes', async () => {
    render(
      <Tooltip content="Accessible tooltip">
        <button>Accessible button</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button')
    expect(trigger).toHaveAttribute('aria-describedby')
    
    fireEvent.mouseEnter(trigger)
    
    const tooltip = await screen.findByRole('tooltip')
    expect(tooltip).toHaveAttribute('id')
  })

  it('does not render tooltip when content is empty', () => {
    render(
      <Tooltip content="">
        <button>No tooltip</button>
      </Tooltip>
    )
    
    const trigger = screen.getByRole('button')
    fireEvent.mouseEnter(trigger)
    
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument()
  })
})
