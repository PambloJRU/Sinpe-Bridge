import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'

type Order = {
	id: number
	phone: string
	amount: number
	state: string
}

const apiBaseUrl =
	(import.meta.env.VITE_API_BASE_URL as string | undefined) ??
	'http://localhost:5198'

const ordersEndpoint = `${apiBaseUrl.replace(/\/$/, '')}/api/Orders`

const formatAmount = (value: number) =>
	value.toLocaleString('en-US', {
		minimumFractionDigits: 2,
		maximumFractionDigits: 2,
	})

function OrderHistoryPage() {
	const [orders, setOrders] = useState<Order[]>([])
	const [searchTerm, setSearchTerm] = useState('') // Buscador único para teléfono o estado
	const [currentPage, setCurrentPage] = useState(1) // Página actual
	const itemsPerPage = 15 // Cantidad de elementos por página

	useEffect(() => {
		loadOrders()
	}, [])

	const loadOrders = async () => {
		try {
			const response = await fetch(ordersEndpoint)
			const data = await response.json()
			setOrders(data)
		} catch (error) {
			console.error('Error cargando historial', error)
		}
	}

	// 1. FILTRADO: Buscar por teléfono O por estado (ignorando mayúsculas/minúsculas)
	const filteredOrders = useMemo(() => {
		const term = searchTerm.toLowerCase().trim()
		return orders.filter(order => {
			const phoneMatch = order.phone?.toLowerCase().includes(term)
			const stateMatch = order.state?.toLowerCase().includes(term)
			return phoneMatch || stateMatch
		})
	}, [orders, searchTerm])

	// Al cambiar el término de búsqueda, reiniciamos a la página 1 para evitar inconsistencias
	useEffect(() => {
		setCurrentPage(1)
	}, [searchTerm])

	// 2. PAGINACIÓN: Calcular los índices de los elementos que se van a mostrar
	const totalPages = Math.ceil(filteredOrders.length / itemsPerPage)
	
	const paginatedOrders = useMemo(() => {
		const startIndex = (currentPage - 1) * itemsPerPage
		const endIndex = startIndex + itemsPerPage
		return filteredOrders.slice(startIndex, endIndex)
	}, [filteredOrders, currentPage])

	return (
		<div className="pos-shell">
			<header className="pos-header">
				<div className="pos-brand">
					<p className="pos-kicker">SINPE POS</p>
					<h1>Historial de Ordenes</h1>
					<p className="pos-subtitle">
						Busqueda de ordenes registradas por telefono.
					</p>
				</div>
			</header>

			<div
				style={{
					display: 'flex',
					justifyContent: 'flex-end',
					marginBottom: '15px'
				}}
			>
				<Link to="/">
					<button className="ghost">Volver</button>
				</Link>
			</div>

			<section className="pos-card pos-table">
				<div className="pos-table-top">
					<div>
						<h2>Consultar historial</h2>
						<p className="pos-subtitle">
							Filtre ordenes por numero telefonico.
						</p>
					</div>
				</div>

				<input
					style={{
						width: '100%',
						padding: '14px',
						borderRadius: '14px',
						border: '1px solid #ddd',
						marginBottom: '25px',
						fontSize: '16px'
					}}
					type="text"
					placeholder="Buscar por telefono..."
					value={searchTerm}
					onChange={e => setSearchTerm(e.target.value)}
				/>

				<div className="table-wrapper">
					<table className="payments-table">
						<thead>
							<tr>

								<th>Telefono</th>
								<th>Monto</th>
								<th>Estado</th>
							</tr>
						</thead>

						<tbody>
							{paginatedOrders.length === 0 ? (
								<tr className="table-empty">
									<td colSpan={3}>
										No se encontraron órdenes que coincidan.
									</td>
								</tr>
							) : (
								paginatedOrders.map(order => (
									<tr key={order.id}>
										<td>{order.phone}</td>
										<td className="font-mono amount-cell">
											₡{formatAmount(order.amount)}
										</td>
										<td>
											<span
												className={`status-chip status-${(order.state ?? 'PENDIENTE').toLowerCase()}`}
											>
												{order.state}
											</span>
										</td>
									</tr>
								))
							)}
						</tbody>
					</table>
				</div>
				{totalPages > 1 && (
					<div className="pagination-controls" style={{
						display: 'flex',
						justifyContent: 'space-between',
						alignItems: 'center',
						marginTop: '20px',
						padding: '10px 5px'
					}}>
						<button
							className="ghost"
							onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
							disabled={currentPage === 1}
							style={{ opacity: currentPage === 1 ? 0.5 : 1, cursor: currentPage === 1 ? 'not-allowed' : 'pointer' }}
						>
							← Anterior
						</button>
						
						<span style={{ fontSize: '14px', color: '#666', fontWeight: 500 }}>
							Página {currentPage} de {totalPages} ({filteredOrders.length} resultados)
						</span>

						<button
							className="ghost"
							onClick={() => setCurrentPage(prev => Math.max(prev + 1, totalPages))}
							disabled={currentPage === totalPages}
							style={{ opacity: currentPage === totalPages ? 0.5 : 1, cursor: currentPage === totalPages ? 'not-allowed' : 'pointer' }}
						>
							Siguiente →
						</button>
					</div>
				)}
			</section>
		</div>
	)
}

export default OrderHistoryPage