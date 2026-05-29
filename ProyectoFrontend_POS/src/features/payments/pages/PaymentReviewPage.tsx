import { useState, useEffect } from 'react'

type PendingReviewPayment = {
	paymentId: number
	amount: number
	reference: string
	clientPhone: string
	clientName: string
	orderId?: number
	orderAmount?: number
	difference?: number
	receivedAt: string
}

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined) ??
	'http://localhost:5198'
const paymentsEndpoint = `${apiBaseUrl.replace(/\/$/, '')}/api/Payments`

function PaymentsReviewPage() {
	const [payments, setPayments] = useState<PendingReviewPayment[]>([])
	const [loading, setLoading] = useState(true)
	const [message, setMessage] = useState('')

	useEffect(() => {
		fetchPendingReviewPayments()
	}, [])

	const fetchPendingReviewPayments = async () => {
		try {
			const response = await fetch(`${paymentsEndpoint}/pending-review`)
			if (response.ok) {
				const data = await response.json()
				setPayments(data)
			}
		} catch (error) {
			console.error('Error fetching payments:', error)
		} finally {
			setLoading(false)
		}
	}

	const handleReview = async (paymentId: number, approved: boolean) => {
		try {
			const response = await fetch(`${paymentsEndpoint}/${paymentId}/review`, {
				method: 'PUT',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({ approved }),
			})

			if (response.ok) {
				setMessage(approved ? 'Pago aprobado' : 'Pago rechazado')
				setPayments((prev) => prev.filter((p) => p.paymentId !== paymentId))
				setTimeout(() => setMessage(''), 3000)
			}
		} catch (error) {
			console.error('Error reviewing payment:', error)
			setMessage('Error al procesar la solicitud')
		}
	}

	if (loading) return <div className="pos-shell">Cargando...</div>

	return (
		<div className="pos-shell">
			<header className="pos-header">
				<div className="pos-brand">
					<p className="pos-kicker">SINPE POS</p>
					<h1>Pagos en Revisión</h1>
					<p className="pos-subtitle">
						Revisa y aprueba o rechaza pagos sospechosos.
					</p>
				</div>
			</header>

			<section className="pos-card pos-table">
				{message && <p className="form-error">{message}</p>}

				<div className="table-wrapper">
					<table className="payments-table">
						<thead>
							<tr>
								<th>Monto</th>
								<th>Teléfono</th>
								<th>Orden</th>
								<th>Diferencia</th>
								<th>Referencia</th>
								<th>Fecha</th>
								<th>Acciones</th>
							</tr>
						</thead>
						<tbody>
							{payments.length === 0 ? (
								<tr className="table-empty">
									<td colSpan={7}>Sin pagos en revisión.</td>
								</tr>
							) : (
								payments.map((payment) => (
									<tr key={payment.paymentId}>
										<td>CRC {payment.amount.toLocaleString()}</td>
										<td>{payment.clientPhone}</td>
										<td>{payment.orderAmount ? `CRC ${payment.orderAmount.toLocaleString()}` : '-'}</td>
										<td style={{ color: payment.difference && payment.difference > 0 ? '#ff6b6b' : '#51cf66' }}>
											{payment.difference ? `CRC ${payment.difference.toLocaleString()}` : '-'}
										</td>
										<td style={{ fontSize: '0.85em', maxWidth: '150px', overflow: 'hidden', textOverflow: 'ellipsis' }}>
											{payment.reference}
										</td>
										<td>{new Date(payment.receivedAt).toLocaleString()}</td>
										<td>
											<div className="table-actions">
												<button
													type="button"
													className="ghost"
													onClick={() => handleReview(payment.paymentId, true)}
												>
													Aprobar
												</button>
												<button
													type="button"
													className="ghost"
													onClick={() => handleReview(payment.paymentId, false)}
													style={{ color: '#ff6b6b' }}
												>
													Rechazar
												</button>
											</div>
										</td>
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

export default PaymentsReviewPage