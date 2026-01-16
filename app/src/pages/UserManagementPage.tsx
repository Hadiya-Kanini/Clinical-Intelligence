import type { ChangeEvent, FormEvent } from 'react'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Modal from '../components/ui/Modal'
import Select from '../components/ui/Select'
import Table from '../components/ui/Table'
import TextField from '../components/ui/TextField'
import { isValidEmailRfc5322 } from '../lib/validation/email'
import { 
  adminUsersApi, 
  type AdminUserItem, 
  type AdminUsersListQuery,
  type CreateUserRequest,
  type UpdateUserRequest
} from '../lib/adminUsersApi'

type Role = 'standard' | 'admin'
type SortColumn = 'name' | 'email' | 'role' | 'status'
type SortDirection = 'asc' | 'desc'

type UserRow = {
  id: string
  name: string
  email: string
  role: Role
  status: 'active' | 'inactive' | 'locked'
  displayStatus: 'active' | 'deactivated' | 'locked'
}

const DEFAULT_PAGE_SIZE = 20

export default function UserManagementPage(): JSX.Element {
  const [query, setQuery] = useState('')
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const [users, setUsers] = useState<UserRow[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [sortBy, setSortBy] = useState<SortColumn>('name')
  const [sortDir, setSortDir] = useState<SortDirection>('asc')
  const [page, setPage] = useState(1)
  const [pageSize] = useState(DEFAULT_PAGE_SIZE)
  const [total, setTotal] = useState(0)

  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<UserRow | null>(null)

  const [formName, setFormName] = useState('')
  const [formEmail, setFormEmail] = useState('')
  const [formRole, setFormRole] = useState<Role>('standard')
  const [toast, setToast] = useState('')

  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const fetchUsers = useCallback(async () => {
    setLoading(true)
    setError(null)

    const params: AdminUsersListQuery = {
      page,
      pageSize,
      sortBy,
      sortDir,
    }

    if (debouncedQuery.trim()) {
      params.q = debouncedQuery.trim()
    }

    const result = await adminUsersApi.listUsers(params)

    if (result.success) {
      const mappedUsers: UserRow[] = result.data.items.map((item: AdminUserItem) => ({
        id: item.id,
        name: item.name,
        email: item.email,
        role: item.role as Role,
        status: item.status as UserRow['status'],
        displayStatus: item.status === 'inactive' ? 'deactivated' : item.status as UserRow['displayStatus'],
      }))
      setUsers(mappedUsers)
      setTotal(result.data.total)
    } else {
      if (result.status === 403) {
        setError('Access denied. Admin privileges required.')
      } else {
        setError(result.error.message || 'Failed to load users.')
      }
      setUsers([])
      setTotal(0)
    }

    setLoading(false)
  }, [page, pageSize, sortBy, sortDir, debouncedQuery])

  useEffect(() => {
    fetchUsers()
  }, [fetchUsers])

  useEffect(() => {
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current)
    }

    debounceTimerRef.current = setTimeout(() => {
      setDebouncedQuery(query)
      setPage(1)
    }, 300)

    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current)
      }
    }
  }, [query])

  const totalPages = useMemo(() => Math.ceil(total / pageSize), [total, pageSize])

  function handleSort(column: SortColumn): void {
    if (sortBy === column) {
      setSortDir(sortDir === 'asc' ? 'desc' : 'asc')
    } else {
      setSortBy(column)
      setSortDir('asc')
    }
    setPage(1)
  }

  function getSortIndicator(column: SortColumn): string {
    if (sortBy !== column) return ''
    return sortDir === 'asc' ? ' ▲' : ' ▼'
  }

  function handlePrevPage(): void {
    if (page > 1) {
      setPage(page - 1)
    }
  }

  function handleNextPage(): void {
    if (page < totalPages) {
      setPage(page + 1)
    }
  }

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

  async function handleSubmit(e: FormEvent<HTMLFormElement>): Promise<void> {
    e.preventDefault()

    if (!formName.trim() || !formEmail.trim()) {
      setToast('Name and email are required.')
      setTimeout(() => setToast(''), 2000)
      return
    }

    if (!isValidEmailRfc5322(formEmail.trim())) {
      setToast('Please enter a valid email address.')
      setTimeout(() => setToast(''), 2500)
      return
    }

    try {
      if (editing) {
        // Update existing user
        const updateRequest: UpdateUserRequest = {
          name: formName.trim(),
          email: formEmail.trim(),
          role: formRole,
        }
        
        const result = await adminUsersApi.updateUser(editing.id, updateRequest)
        
        if (result.success) {
          // Update local state with the response
          setUsers((current) =>
            current.map((u) => (u.id === editing.id ? { 
              ...u, 
              name: result.data.name, 
              email: result.data.email, 
              role: result.data.role 
            } : u))
          )
          setToast('User updated successfully.')
        } else {
          setToast(result.error.message || 'Failed to update user.')
        }
      } else {
        // Create new user
        const createRequest: CreateUserRequest = {
          name: formName.trim(),
          email: formEmail.trim(),
          role: formRole,
        }
        
        const result = await adminUsersApi.createUser(createRequest)
        
        if (result.success) {
          // Add new user to local state (new users are always active)
          const newUser: UserRow = {
            id: result.data.id,
            name: result.data.name,
            email: result.data.email,
            role: result.data.role,
            status: 'active',
            displayStatus: 'active',
          }
          setUsers((current) => [newUser, ...current])
          setTotal((current) => current + 1)
          
          // Show appropriate success message
          if (result.data.credentials_email_sent) {
            setToast('User created successfully. Credentials email sent.')
          } else {
            setToast('User created but credentials email failed. Check error logs.')
          }
        } else {
          setToast(result.error.message || 'Failed to create user.')
        }
      }

      setTimeout(() => setToast(''), 3000)
      closeModal()
    } catch (err: any) {
      setToast('An unexpected error occurred. Please try again.')
      setTimeout(() => setToast(''), 3000)
    }
  }

  async function toggleStatus(user: UserRow): Promise<void> {
    try {
      const result = await adminUsersApi.toggleUserStatus(user.id)
      
      if (result.success) {
        // Update local state with the new status from API
        const newDisplayStatus = result.data.status === 'inactive' ? 'deactivated' : result.data.status as UserRow['displayStatus']
        setUsers((current) =>
          current.map((u) => (u.id === user.id ? { ...u, status: result.data.status, displayStatus: newDisplayStatus } : u))
        )
        setToast(`User ${result.data.status === 'active' ? 'activated' : 'deactivated'} successfully.`)
      } else {
        setToast(result.error.message || 'Failed to toggle user status.')
      }
      
      setTimeout(() => setToast(''), 2500)
    } catch (err: any) {
      setToast('An unexpected error occurred. Please try again.')
      setTimeout(() => setToast(''), 2500)
    }
  }

  const renderSortableHeader = (column: SortColumn, label: string) => (
    <th
      onClick={() => handleSort(column)}
      style={{ cursor: 'pointer', userSelect: 'none' }}
      role="columnheader"
      aria-sort={sortBy === column ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault()
          handleSort(column)
        }
      }}
    >
      {label}{getSortIndicator(column)}
    </th>
  )

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
              placeholder="Search by name or email"
              className="ui-textfield__input"
              style={{ width: 260 }}
              aria-label="Search users"
            />
            <Button onClick={openCreate}>Create user</Button>
          </div>
        }
      >
        {loading ? (
          <Alert variant="info">Loading users...</Alert>
        ) : error ? (
          <Alert variant="error">{error}</Alert>
        ) : users.length === 0 ? (
          <Alert variant="info">No users found.</Alert>
        ) : (
          <>
            <Table>
              <thead>
                <tr>
                  {renderSortableHeader('name', 'User')}
                  {renderSortableHeader('role', 'Role')}
                  {renderSortableHeader('status', 'Status')}
                  <th style={{ width: 260 }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id}>
                    <td>
                      <div style={{ fontWeight: 600 }}>{u.name}</div>
                      <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>{u.email}</div>
                    </td>
                    <td>
                      <Badge variant={u.role === 'admin' ? 'info' : 'neutral'}>{u.role}</Badge>
                    </td>
                    <td>
                      <Badge variant={u.displayStatus === 'active' ? 'success' : 'warning'}>{u.displayStatus}</Badge>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                        <Button variant="secondary" onClick={() => openEdit(u)}>
                          Edit
                        </Button>
                        <Button variant="secondary" onClick={() => toggleStatus(u)}>
                          {u.displayStatus === 'active' ? 'Deactivate' : 'Activate'}
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 'var(--space-4)' }}>
              <span style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>
                Showing {(page - 1) * pageSize + 1}–{Math.min(page * pageSize, total)} of {total} users
              </span>
              <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                <Button
                  variant="secondary"
                  onClick={handlePrevPage}
                  disabled={page <= 1}
                  aria-label="Previous page"
                >
                  Previous
                </Button>
                <span style={{ display: 'flex', alignItems: 'center', padding: '0 var(--space-2)' }}>
                  Page {page} of {totalPages || 1}
                </span>
                <Button
                  variant="secondary"
                  onClick={handleNextPage}
                  disabled={page >= totalPages}
                  aria-label="Next page"
                >
                  Next
                </Button>
              </div>
            </div>
          </>
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
        </form>
      </Modal>
    </div>
  )
}

