import { useCallback, useEffect, useId, useRef, useState, type ReactNode } from 'react'

type TooltipProps = {
  content: ReactNode
  children: ReactNode
  disabled?: boolean
  position?: 'top' | 'bottom' | 'left' | 'right'
}

export default function Tooltip({
  content,
  children,
  disabled = false,
  position = 'top',
}: TooltipProps): JSX.Element {
  const [isVisible, setIsVisible] = useState(false)
  const [isTouchMode, setIsTouchMode] = useState(false)
  const tooltipId = useId()
  const triggerRef = useRef<HTMLSpanElement>(null)
  const tooltipRef = useRef<HTMLDivElement>(null)

  const showTooltip = useCallback(() => {
    if (!disabled && content) {
      setIsVisible(true)
    }
  }, [disabled, content])

  const hideTooltip = useCallback(() => {
    setIsVisible(false)
  }, [])

  const handleMouseEnter = useCallback(() => {
    if (!isTouchMode) {
      showTooltip()
    }
  }, [isTouchMode, showTooltip])

  const handleMouseLeave = useCallback(() => {
    if (!isTouchMode) {
      hideTooltip()
    }
  }, [isTouchMode, hideTooltip])

  const handleFocus = useCallback(() => {
    showTooltip()
  }, [showTooltip])

  const handleBlur = useCallback(() => {
    hideTooltip()
  }, [hideTooltip])

  const handleTouchStart = useCallback(() => {
    setIsTouchMode(true)
  }, [])

  const handleClick = useCallback(() => {
    if (isTouchMode) {
      setIsVisible((prev) => !prev)
    }
  }, [isTouchMode])

  useEffect(() => {
    if (!isTouchMode || !isVisible) return

    const handleOutsideClick = (event: MouseEvent | TouchEvent) => {
      const target = event.target as Node
      if (
        triggerRef.current &&
        !triggerRef.current.contains(target) &&
        tooltipRef.current &&
        !tooltipRef.current.contains(target)
      ) {
        setIsVisible(false)
      }
    }

    document.addEventListener('mousedown', handleOutsideClick)
    document.addEventListener('touchstart', handleOutsideClick)

    return () => {
      document.removeEventListener('mousedown', handleOutsideClick)
      document.removeEventListener('touchstart', handleOutsideClick)
    }
  }, [isTouchMode, isVisible])

  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isVisible) {
        setIsVisible(false)
      }
    }

    document.addEventListener('keydown', handleEscape)
    return () => document.removeEventListener('keydown', handleEscape)
  }, [isVisible])

  if (disabled || !content) {
    return <>{children}</>
  }

  const positionStyles: Record<string, React.CSSProperties> = {
    top: {
      bottom: '100%',
      left: '50%',
      transform: 'translateX(-50%)',
      marginBottom: 'var(--space-2)',
    },
    bottom: {
      top: '100%',
      left: '50%',
      transform: 'translateX(-50%)',
      marginTop: 'var(--space-2)',
    },
    left: {
      right: '100%',
      top: '50%',
      transform: 'translateY(-50%)',
      marginRight: 'var(--space-2)',
    },
    right: {
      left: '100%',
      top: '50%',
      transform: 'translateY(-50%)',
      marginLeft: 'var(--space-2)',
    },
  }

  return (
    <span
      ref={triggerRef}
      style={{ position: 'relative', display: 'inline-block' }}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
      onFocus={handleFocus}
      onBlur={handleBlur}
      onTouchStart={handleTouchStart}
      onClick={handleClick}
      aria-describedby={isVisible ? tooltipId : undefined}
    >
      {children}
      {isVisible && (
        <div
          ref={tooltipRef}
          id={tooltipId}
          role="tooltip"
          style={{
            position: 'absolute',
            zIndex: 1000,
            padding: 'var(--space-2) var(--space-3)',
            backgroundColor: 'var(--color-neutral-900)',
            color: 'var(--color-neutral-50)',
            borderRadius: 'var(--radius-md)',
            fontSize: 'var(--font-size-body-small)',
            lineHeight: 1.4,
            maxWidth: 280,
            whiteSpace: 'normal',
            wordWrap: 'break-word',
            boxShadow: '0 2px 8px rgba(0, 0, 0, 0.15)',
            pointerEvents: isTouchMode ? 'auto' : 'none',
            ...positionStyles[position],
          }}
        >
          {content}
        </div>
      )}
    </span>
  )
}
