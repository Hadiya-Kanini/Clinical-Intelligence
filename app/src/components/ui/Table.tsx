import type { HTMLAttributes, ReactNode } from 'react'

type TableProps = {
  children: ReactNode
} & Omit<HTMLAttributes<HTMLTableElement>, 'children'>

export default function Table({ children, className = '', ...props }: TableProps): JSX.Element {
  return (
    <table className={`ui-table${className ? ` ${className}` : ''}`} {...props}>
      {children}
    </table>
  )
}
