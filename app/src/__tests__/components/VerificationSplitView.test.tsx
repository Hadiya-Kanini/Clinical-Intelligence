import { render, screen } from '@testing-library/react'
import { createRef } from 'react'
import { describe, it, expect } from 'vitest'
import VerificationSplitView from '../../components/layout/VerificationSplitView'

describe('VerificationSplitView', () => {
  it('renders left and right panes', () => {
    render(
      <VerificationSplitView
        leftPane={<div data-testid="left-content">Left Content</div>}
        rightPane={<div data-testid="right-content">Right Content</div>}
      />
    )
    
    expect(screen.getByTestId('left-content')).toBeInTheDocument()
    expect(screen.getByTestId('right-content')).toBeInTheDocument()
  })

  it('applies correct grid layout', () => {
    const { container } = render(
      <VerificationSplitView
        leftPane={<div>Left</div>}
        rightPane={<div>Right</div>}
      />
    )
    
    const splitView = container.firstChild as HTMLElement
    expect(splitView).toHaveStyle({ display: 'grid' })
    expect(splitView).toHaveStyle({ gridTemplateColumns: '1fr 1fr' })
  })

  it('applies error styling to left pane when leftPaneError is true', () => {
    render(
      <VerificationSplitView
        leftPane={<div>Left</div>}
        rightPane={<div>Right</div>}
        leftPaneError={true}
      />
    )
    
    const leftPane = screen.getByText('Left').parentElement
    expect(leftPane).toHaveStyle({ opacity: '0.6' })
  })

  it('does not apply error styling when leftPaneError is false', () => {
    render(
      <VerificationSplitView
        leftPane={<div>Left</div>}
        rightPane={<div>Right</div>}
        leftPaneError={false}
      />
    )
    
    const leftPane = screen.getByText('Left').parentElement
    expect(leftPane).not.toHaveStyle({ opacity: '0.6' })
  })

  it('forwards refs to pane containers', () => {
    const leftRef = createRef<HTMLDivElement>()
    const rightRef = createRef<HTMLDivElement>()
    
    render(
      <VerificationSplitView
        leftPane={<div>Left</div>}
        rightPane={<div>Right</div>}
        leftPaneRef={leftRef}
        rightPaneRef={rightRef}
      />
    )
    
    expect(leftRef.current).toBeInstanceOf(HTMLDivElement)
    expect(rightRef.current).toBeInstanceOf(HTMLDivElement)
  })

  it('applies overflow auto for scrolling', () => {
    const leftRef = createRef<HTMLDivElement>()
    const rightRef = createRef<HTMLDivElement>()
    
    render(
      <VerificationSplitView
        leftPane={<div>Left</div>}
        rightPane={<div>Right</div>}
        leftPaneRef={leftRef}
        rightPaneRef={rightRef}
      />
    )
    
    expect(leftRef.current).toHaveStyle({ overflowY: 'auto' })
    expect(rightRef.current).toHaveStyle({ overflowY: 'auto' })
  })
})
