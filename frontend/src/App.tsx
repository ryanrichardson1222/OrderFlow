
import { useCallback, useEffect, useMemo, useState } from 'react'
import './App.css'

interface Order {
  id: number
  userId: number
  customerName: string
  status: string
  total: number
}

type StatusFilter = 'All' | 'Pending' | 'Processing' | 'Shipped' | 'Cancelled'

const filters: StatusFilter[] = [
  'All',
  'Pending',
  'Processing',
  'Shipped',
  'Cancelled',
]

const currency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})

function App() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All')
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null)

    const fetchOrders = useCallback(async (): Promise<Order[]> => {
    const response = await fetch('/api/orders')

    if (!response.ok) {
      throw new Error(
        `API request failed: ${response.status} ${response.statusText}`,
      )
    }

    return response.json()
  }, [])

  const loadOrders = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await fetchOrders()
      setOrders(data)
      setLastUpdated(new Date())
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'An unexpected error occurred.',
      )
    } finally {
      setLoading(false)
    }
  }, [fetchOrders])

  useEffect(() => {
    let ignore = false

    fetchOrders()
      .then((data) => {
        if (!ignore) {
          setOrders(data)
          setLastUpdated(new Date())
        }
      })
      .catch((err: unknown) => {
        if (!ignore) {
          setError(
            err instanceof Error ? err.message : 'An unexpected error occurred.',
          )
        }
      })
      .finally(() => {
        if (!ignore) {
          setLoading(false)
        }
      })

    return () => {
      ignore = true
    }
  }, [fetchOrders])

  const filteredOrders = useMemo(() => {
    const query = search.trim().toLowerCase()

    return orders.filter((order) => {
      const matchesStatus =
        statusFilter === 'All' || order.status === statusFilter

      const matchesSearch =
        !query ||
        order.customerName.toLowerCase().includes(query) ||
        order.id.toString().includes(query) ||
        order.status.toLowerCase().includes(query)

      return matchesStatus && matchesSearch
    })
  }, [orders, search, statusFilter])

  const totalValue = orders.reduce((sum, order) => sum + order.total, 0)
  const pendingCount = orders.filter((order) => order.status === 'Pending').length
  const processingCount = orders.filter(
    (order) => order.status === 'Processing',
  ).length
  const shippedCount = orders.filter((order) => order.status === 'Shipped').length

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">O</div>
          <span>Order<span className="brand-accent">Flow</span></span>
        </div>

        <div className="sidebar-section">
          <span className="sidebar-label">WORKSPACE</span>
          <div className="nav-item active">
            <span className="nav-icon">▦</span>
            Orders
            <span className="nav-count">{orders.length}</span>
          </div>
        </div>

        <div className="sidebar-footer">
          <div className="connection-indicator">
            <span className="connection-dot" />
            API connection
          </div>
          <span className="sidebar-version">OrderFlow · v1.0</span>
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div>
            <span className="breadcrumb">Workspace / Orders</span>
          </div>
          <div className="topbar-right">
            <span className="environment-label">DEVELOPMENT</span>
            <div className="avatar">RR</div>
          </div>
        </header>

        <section className="page-content">
          <div className="page-heading">
            <div>
              <span className="eyebrow">ORDER MANAGEMENT</span>
              <h1>Orders</h1>
              <p className="page-description">
                Monitor and manage customer orders across your operation.
              </p>
            </div>

            <button
              className="refresh-button"
              onClick={() => void loadOrders()}
              disabled={loading}
              type="button"
            >
              <span className={loading ? 'refresh-icon spinning' : 'refresh-icon'}>
                ↻
              </span>
              {loading ? 'Refreshing...' : 'Refresh data'}
            </button>
          </div>

          <section className="stats-grid" aria-label="Order statistics">
            <article className="stat-card">
              <div className="stat-top">
                <span className="stat-label">Total orders</span>
                <span className="stat-icon blue">▦</span>
              </div>
              <strong className="stat-value">{orders.length}</strong>
              <span className="stat-detail">Active order records</span>
            </article>

            <article className="stat-card">
              <div className="stat-top">
                <span className="stat-label">Total order value</span>
                <span className="stat-icon purple">$</span>
              </div>
              <strong className="stat-value">{currency.format(totalValue)}</strong>
              <span className="stat-detail">Combined order value</span>
            </article>

            <article className="stat-card">
              <div className="stat-top">
                <span className="stat-label">Pending orders</span>
                <span className="stat-icon amber">◷</span>
              </div>
              <strong className="stat-value">{pendingCount}</strong>
              <span className="stat-detail">Awaiting processing</span>
            </article>

            <article className="stat-card">
              <div className="stat-top">
                <span className="stat-label">Shipped orders</span>
                <span className="stat-icon green">✓</span>
              </div>
              <strong className="stat-value">{shippedCount}</strong>
              <span className="stat-detail">
                {processingCount} currently processing
              </span>
            </article>
          </section>

          <section className="orders-panel">
            <div className="panel-heading">
              <div>
                <h2>All orders</h2>
                <p>Review and search your order records.</p>
              </div>
              <span className="record-count">
                {filteredOrders.length} records
              </span>
            </div>

            <div className="table-toolbar">
              <div className="search-box">
                <span className="search-icon">⌕</span>
                <input
                  aria-label="Search orders"
                  type="search"
                  placeholder="Search by customer, ID, or status..."
                  value={search}
                  onChange={(event) => setSearch(event.target.value)}
                />
              </div>

              <div className="filter-group" aria-label="Filter orders by status">
                {filters.map((filter) => (
                  <button
                    key={filter}
                    type="button"
                    className={statusFilter === filter ? 'filter-button selected' : 'filter-button'}
                    onClick={() => setStatusFilter(filter)}
                    aria-pressed={statusFilter === filter}
                  >
                    {filter}
                  </button>
                ))}
              </div>
            </div>

            {error && (
              <div className="error-state" role="alert">
                <strong>Unable to load orders</strong>
                <span>{error}</span>
                <button type="button" onClick={() => void loadOrders()}>
                  Try again
                </button>
              </div>
            )}

            {!error && loading && orders.length === 0 && (
              <div className="empty-state">Loading orders from the API...</div>
            )}

            {!error && !loading && filteredOrders.length === 0 && (
              <div className="empty-state">
                {orders.length === 0
                  ? 'No orders have been returned by the API.'
                  : 'No orders match your search or selected filter.'}
              </div>
            )}

            {(!error && filteredOrders.length > 0) && (
              <div className="table-wrapper">
                <table className="orders-table">
                  <thead>
                    <tr>
                      <th>ORDER ID</th>
                      <th>CUSTOMER</th>
                      <th>USER ID</th>
                      <th>STATUS</th>
                      <th className="amount-column">ORDER TOTAL</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredOrders.map((order) => (
                      <tr key={order.id}>
                        <td className="order-id">#{order.id}</td>
                        <td className="customer-name">{order.customerName}</td>
                        <td className="muted-cell">{order.userId}</td>
                        <td>
                          <span
                            className={`status-badge status-${order.status.toLowerCase()}`}
                          >
                            <span className="status-dot" />
                            {order.status}
                          </span>
                        </td>
                        <td className="amount-column amount">
                          {currency.format(order.total)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <div className="panel-footer">
              <span>
                {lastUpdated
                  ? `Last updated ${lastUpdated.toLocaleTimeString()}`
                  : 'Waiting for API response'}
              </span>
              <span>Data source: OrderFlow API</span>
            </div>
          </section>

          <footer className="page-footer">
            <span>OrderFlow Operations Dashboard</span>
            <span>Built with React · TypeScript · .NET 10 · SQL Server</span>
          </footer>
        </section>
      </main>
    </div>
  )
}

export default App
