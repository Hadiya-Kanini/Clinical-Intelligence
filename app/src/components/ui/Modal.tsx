import type { ReactNode } from 'react'
import { useEffect } from 'react'

type ModalProps = {
  open: boolean
  title: ReactNode
  children: ReactNode
  footer?: ReactNode
  onClose: () => void
}

export default function Modal({ open, title, children, footer, onClose }: ModalProps): JSX.Element | null {
  useEffect(() => {
    if (!open) return

    function handleKeyDown(event: KeyboardEvent): void {
      if (event.key !== 'Escape') return
      onClose()
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [open, onClose])

  if (!open) return null

  return (
    <div className="ui-modalBackdrop" role="presentation" onMouseDown={onClose}>
      <div
        className="ui-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="ui-modal-title"
        onMouseDown={(e) => e.stopPropagation()}
      >
        <header className="ui-modal__header">
          <h2 className="ui-modal__title" id="ui-modal-title">
            {title}
          </h2>
          <button type="button" className="ui-button ui-button--secondary" onClick={onClose}>
            Close
          </button>
        </header>
        <div className="ui-modal__content">{children}</div>
        {footer ? <footer className="ui-modal__footer">{footer}</footer> : null}
      </div>
    </div>
  )
}
