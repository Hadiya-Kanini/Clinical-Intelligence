import { forwardRef, type ReactNode, type RefObject } from 'react'

type VerificationSplitViewProps = {
  leftPane: ReactNode
  rightPane: ReactNode
  leftPaneRef?: RefObject<HTMLDivElement>
  rightPaneRef?: RefObject<HTMLDivElement>
  leftPaneError?: boolean
}

const VerificationSplitView = forwardRef<HTMLDivElement, VerificationSplitViewProps>(
  function VerificationSplitView(
    { leftPane, rightPane, leftPaneRef, rightPaneRef, leftPaneError = false },
    ref
  ) {
    return (
      <div
        ref={ref}
        style={{
          display: 'grid',
          gridTemplateColumns: 'minmax(400px, 1fr) minmax(400px, 1fr)',
          gap: 'var(--space-6)',
          minHeight: 600,
          width: '100%',
        }}
      >
        <div
          ref={leftPaneRef}
          style={{
            display: 'flex',
            flexDirection: 'column',
            minWidth: 0,
            overflow: 'auto',
            border: leftPaneError ? '2px solid var(--color-error)' : undefined,
            borderRadius: 'var(--radius-md)',
          }}
          data-testid="verification-left-pane"
        >
          {leftPane}
        </div>

        <div
          ref={rightPaneRef}
          style={{
            display: 'flex',
            flexDirection: 'column',
            gap: 'var(--space-4)',
            minWidth: 0,
            overflow: 'auto',
          }}
          data-testid="verification-right-pane"
        >
          {rightPane}
        </div>
      </div>
    )
  }
)

export default VerificationSplitView
