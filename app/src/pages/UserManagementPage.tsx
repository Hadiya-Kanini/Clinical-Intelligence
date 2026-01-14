import type { ChangeEvent, FormEvent } from 'react'
import { useMemo, useState } from 'react'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Modal from '../components/ui/Modal'
import Select from '../components/ui/Select'
import Table from '../components/ui/Table'
import TextField from '../components/ui/TextField'

type Role = 'standard' | 'admin'

type UserRow = {
  id: string
  name: string
  email: string
  role: Role
  status: 'active' | 'deactivated'
}

export default function UserManagementPage(): JSX.Element {
  const [query, setQuery] = useState('')
  const [users, setUsers] = useState<UserRow[]>([
    { id: 'U-001', name: 'Admin', email: 'admin@hospital.org', role: 'admin', status: 'active' },
    { id: 'U-002', name: 'Clinician One', email: 'clinician1@hospital.org', role: 'standard', status: 'active' },
    { id: 'U-003', name: 'Coder One', email: 'coder1@hospital.org', role: 'standard', status: 'deactivated' },
  ])

  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<UserRow | null>(null)

  const [formName, setFormName] = useState('')
  const [formEmail, setFormEmail] = useState('')
  const [formRole, setFormRole] = useState<Role>('standard')
  const [toast, setToast] = useState('')

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase()
    if (!q) return users
    return users.filter((u) => u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q))
  }, [query, users])

  function openCreate(): void {
    setEditing(null)
    setFormName('')
    setFormEmail('')
    setFormRole('standard')
    setModalOpen(true)
  }

  function openEdit(user: UserRow): void {
    setEditing(user)
    setFormName(user.name)
    setFormEmail(user.email)
    setFormRole(user.role)
    setModalOpen(true)
  }

  function closeModal(): void {
    setModalOpen(false)
  }

  function handleSubmit(e: FormEvent<HTMLFormElement>): void {
    e.preventDefault()

    if (!formName.trim() || !formEmail.trim()) {
      setToast('Name and email are required.')
      setTimeout(() => setToast(''), 2000)
      return
    }

    if (editing) {
      setUsers((current) =>
        current.map((u) => (u.id === editing.id ? { ...u, name: formName.trim(), email: formEmail.trim(), role: formRole } : u))
      )
      setToast('User updated.')
    } else {
      const id = `U-${String(Date.now()).slice(-4)}`
      setUsers((current) => [
        { id, name: formName.trim(), email: formEmail.trim(), role: formRole, status: 'active' },
        ...current,
      ])
      setToast('User created. Credentials email would be sent (demo).')
    }

    setTimeout(() => setToast(''), 2500)
    closeModal()
  }

  function toggleStatus(user: UserRow): void {
    setUsers((current) =>
      current.map((u) => (u.id === user.id ? { ...u, status: u.status === 'active' ? 'deactivated' : 'active' } : u))
    )
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      {toast ? <Alert variant="info">{toast}</Alert> : null}

      <Card
        title="User management"
        headerRight={
          <div style={{ display: 'flex', gap: 'var(--space-3)' }}>
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search users"
              className="ui-textfield__input"
              style={{ width: 260 }}
            />
            <Button onClick={openCreate}>Create user</Button>
          </div>
        }
      >
        {filtered.length === 0 ? (
          <Alert variant="info">No users found.</Alert>
        ) : (
          <Table>
            <thead>
              <tr>
                <th>User</th>
                <th>Role</th>
                <th>Status</th>
                <th style={{ width: 260 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((u) => (
                <tr key={u.id}>
                  <td>
                    <div style={{ fontWeight: 600 }}>{u.name}</div>
                    <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>{u.email}</div>
                  </td>
                  <td>
                    <Badge variant={u.role === 'admin' ? 'info' : 'neutral'}>{u.role}</Badge>
                  </td>
                  <td>
                    <Badge variant={u.status === 'active' ? 'success' : 'warning'}>{u.status}</Badge>
                  </td>
                  <td>
                    <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                      <Button variant="secondary" onClick={() => openEdit(u)}>
                        Edit
                      </Button>
                      <Button variant="secondary" onClick={() => toggleStatus(u)}>
                        {u.status === 'active' ? 'Deactivate' : 'Activate'}
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        )}
      </Card>

      <Modal
        open={modalOpen}
        title={editing ? 'Edit user' : 'Create user'}
        onClose={closeModal}
        footer={
          <>
            <Button variant="secondary" onClick={closeModal}>
              Cancel
            </Button>
            <Button type="submit" form="user-form">
              {editing ? 'Save changes' : 'Create user'}
            </Button>
          </>
        }
      >
        <form id="user-form" onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
          <TextField
            label="Full name"
            value={formName}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setFormName(e.target.value)}
            placeholder="Jane Doe"
            required
          />
          <TextField
            label="Email"
            value={formEmail}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setFormEmail(e.target.value)}
            placeholder="name@hospital.org"
            required
            type="email"
          />
          <Select
            label="Role"
            value={formRole}
            onChange={(e) => setFormRole(e.target.value as Role)}
            options={[
              { value: 'standard', label: 'Standard' },
              { value: 'admin', label: 'Admin' },
            ]}
          />
          <Alert variant="info">Duplicate email checks and SMTP notifications will be wired to the API later.</Alert>
        </form>
      </Modal>
    </div>
  )
}
