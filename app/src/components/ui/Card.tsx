import type { HTMLAttributes, ReactNode } from 'react'

type CardProps = {
  title?: ReactNode
  headerRight?: ReactNode
  children: ReactNode
} & Omit<HTMLAttributes<HTMLElement>, 'children'>

export default function Card({ title, headerRight, children, ...props }: CardProps): JSX.Element {
  return (
    <section className="ui-card" {...props}>
      {title || headerRight ? (
        <header className="ui-card__header">
          {title ? <h2 className="ui-card__title">{title}</h2> : <span />}
          {headerRight ? <div className="ui-card__headerRight">{headerRight}</div> : null}
        </header>
      ) : null}
      <div className="ui-card__content">{children}</div>
    </section>
  )
}
