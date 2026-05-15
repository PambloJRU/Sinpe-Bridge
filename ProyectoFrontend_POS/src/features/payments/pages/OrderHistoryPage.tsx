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
	const [phoneSearch, setPhoneSearch] = useState('')

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

	const filteredOrders = useMemo(() => {
		return orders.filter(order =>
			order.phone?.includes(phoneSearch)
		)
	}, [orders, phoneSearch])

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
					value={phoneSearch}
					onChange={e => setPhoneSearch(e.target.value)}
				/>

				<div className="table-wrapper">
					<table className="payments-table">
						<thead>
							<tr>
								<th>ID</th>
								<th>Telefono</th>
								<th>Monto</th>
								<th>Estado</th>
							</tr>
						</thead>

						<tbody>
							{filteredOrders.length === 0 ? (
								<tr className="table-empty">
									<td colSpan={4}>
										No hay ordenes registradas.
									</td>
								</tr>
							) : (
								filteredOrders.map(order => (
									<tr key={order.id}>
										<td>{order.id}</td>
										<td>{order.phone}</td>
										<td>CRC {formatAmount(order.amount)}</td>
										<td>{order.state}</td>
									</tr>
								))
							)}
						</tbody>
					</table>
				</div>
			</section>
		</div>
	)
}

export default OrderHistoryPage