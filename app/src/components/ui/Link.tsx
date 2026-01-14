import type { AnchorHTMLAttributes, ReactNode } from 'react'

type LinkProps = {
  children: ReactNode
  href: string
} & Omit<AnchorHTMLAttributes<HTMLAnchorElement>, 'children' | 'href'>

export default function Link({ children, href, ...props }: LinkProps): JSX.Element {
  return (
    <a className="ui-link" href={href} {...props}>
      {children}
    </a>
  )
}
