import React from 'react'

export default function Link({ children, href, ...props }) {
  return (
    <a className="ui-link" href={href} {...props}>
      {children}
    </a>
  )
}
