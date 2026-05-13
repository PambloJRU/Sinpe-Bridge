import { useMemo, useState, type FormEvent } from 'react'

type PaymentStatus = 'Pending' | 'Valid' | 'Rejected'
type SyncState = 'idle' | 'syncing' | 'failed'

type Payment = {
	id: number
	amount: number
	phone: string
	reference: string
	status: PaymentStatus
	syncState: SyncState
	syncMessage?: string
}

const formatAmount = (value: number) =>
	value.toLocaleString('en-US', {
		minimumFractionDigits: 2,
		maximumFractionDigits: 2,
	})

const padNumber = (value: number, size = 2) => value.toString().padStart(size, '0')

const generateReference = (date: Date) => {
	const stamp = `${date.getFullYear()}${padNumber(date.getMonth() + 1)}${padNumber(
		date.getDate(),
	)}${padNumber(date.getHours())}${padNumber(date.getMinutes())}${padNumber(date.getSeconds())}`
	const extra = Math.floor(100000 + Math.random() * 900000).toString()

	return `${stamp}${extra}`
}

const statusLabels: Record<PaymentStatus, string> = {
	Pending: 'Pendiente',
	Valid: 'Valido',
	Rejected: 'Rechazado',
}

const referencePlaceholder = 'Esperando SMS'
const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined) ??
	'http://localhost:5198'
const ordersEndpoint = `${apiBaseUrl.replace(/\/$/, '')}/api/Orders`

const getReferenceLabel = (payment: Payment) => {
	if (payment.reference) return payment.reference
	if (payment.status === 'Valid') return 'Asociada'
	if (payment.status === 'Rejected') return 'No recibida'
	return referencePlaceholder
}

function PaymentsPage() {
	const [payments, setPayments] = useState<Payment[]>([])
	const [amountInput, setAmountInput] = useState('')
	const [phoneInput, setPhoneInput] = useState('')
	const [formError, setFormError] = useState('')
	const [nextId, setNextId] = useState(1)

	const stats = useMemo(() => {
		return payments.reduce(
			(accumulator, payment) => {
				accumulator.total += 1

				if (payment.status === 'Pending') accumulator.pending += 1
				if (payment.status === 'Valid') accumulator.valid += 1
				if (payment.status === 'Rejected') accumulator.rejected += 1

				return accumulator
			},
			{ total: 0, pending: 0, valid: 0, rejected: 0 },
		)
	}, [payments])

	const updatePayment = (id: number, update: Partial<Payment>) => {
		setPayments((previous) =>
			previous.map((payment) => (payment.id === id ? { ...payment, ...update } : payment)),
		)
	}

	const submitOrder = async (id: number, phone: string, amount: number) => {
		try {
			const response = await fetch(ordersEndpoint, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({ phone, amount }),
			})

			const responseText = await response.text()

			if (!response.ok) {
				throw new Error(responseText || 'No se pudo crear la orden.')
			}

			updatePayment(id, {
				status: 'Valid',
				syncState: 'idle',
				syncMessage: '',
			})
		} catch (error) {
			const message = error instanceof Error ? error.message : 'Error de conexion.'
			updatePayment(id, {
				status: 'Rejected',
				syncState: 'failed',
				syncMessage: message,
			})
		}
	}

	const handleCreate = (event: FormEvent<HTMLFormElement>) => {
		event.preventDefault()
		const rawAmount = amountInput.trim().replace(',', '.')
		const amount = Number(rawAmount)
		const phoneValue = phoneInput.trim()

		if (!Number.isFinite(amount) || amount <= 0) {
			setFormError('Ingrese un monto valido para continuar.')
			return
		}

		if (!phoneValue || phoneValue.length < 7) {
			setFormError('Ingrese un telefono valido para continuar.')
			return
		}

		const createdId = nextId
		const newPayment: Payment = {
			id: createdId,
			amount,
			phone: phoneValue,
			reference: '',
			status: 'Pending',
			syncState: 'syncing',
		}

		setPayments((previous) => [newPayment, ...previous])
		setNextId((value) => value + 1)
		setAmountInput('')
		setPhoneInput('')
		setFormError('')

		void submitOrder(createdId, phoneValue, amount)
	}

	const clearForm = () => {
		setAmountInput('')
		setPhoneInput('')
		setFormError('')
	}

	const updateStatus = (id: number, status: PaymentStatus) => {
		setPayments((previous) =>
			previous.map((payment) => {
				if (payment.id !== id) return payment
				const reference =
					status === 'Valid' && !payment.reference
						? generateReference(new Date())
						: payment.reference

				return { ...payment, status, reference, syncState: 'idle', syncMessage: '' }
			}),
		)
	}

	return (
		<div className="pos-shell">
			<header className="pos-header">
				<div className="pos-brand">
					<p className="pos-kicker">SINPE POS</p>
					<h1>Ordenes</h1>
					<p className="pos-subtitle">
						Cree ordenes con monto y telefono. La referencia llega con el SMS.
					</p>
				</div>
				<div className="pos-card pos-summary">
					<p className="summary-title">Resumen</p>
					<div className="pos-stats">
						<span className="stat">Total {stats.total}</span>
						<span className="stat">Pendientes {stats.pending}</span>
						<span className="stat">Validos {stats.valid}</span>
						<span className="stat">Rechazados {stats.rejected}</span>
					</div>
					<p className="summary-note">Se permiten multiples ordenes activas.</p>
				</div>
			</header>

			<section className="pos-card pos-table">
				<div className="pos-table-top">
					<div>
						<h2>Crear orden</h2>
						<p className="pos-subtitle">Monto y telefono del cliente.</p>
					</div>
				</div>

				<form className="pos-form-inline" onSubmit={handleCreate}>
					<label className="field-inline">
						<span>Monto</span>
						<div className="input-row">
							<span className="input-prefix">CRC</span>
							<input
								value={amountInput}
								onChange={(event) => setAmountInput(event.target.value)}
								inputMode="decimal"
								placeholder="1500.00"
							/>
						</div>
					</label>

					<label className="field-inline">
						<span>Telefono</span>
						<input
							value={phoneInput}
							onChange={(event) => setPhoneInput(event.target.value)}
							inputMode="tel"
							placeholder="2627-3342"
						/>
					</label>

					<div className="pos-actions">
						<button type="submit" className="primary">
							Crear orden
						</button>
						<button type="button" className="ghost" onClick={clearForm}>
							Limpiar
						</button>
					</div>
				</form>

				{formError ? <p className="form-error">{formError}</p> : null}
				<p className="helper">Formato sugerido: 2627-3342. Use el mismo del SMS.</p>

				<div className="table-divider" />

				<div className="pos-table-header">
					<h2>Ordenes</h2>
					<div className="pos-stats">
						<span className="stat">Total {stats.total}</span>
						<span className="stat">Pendientes {stats.pending}</span>
						<span className="stat">Validos {stats.valid}</span>
						<span className="stat">Rechazados {stats.rejected}</span>
					</div>
				</div>

				<div className="table-wrapper">
					<table className="payments-table">
						<thead>
							<tr>
								<th>Monto</th>
								<th>Telefono</th>
								<th>Referencia</th>
								<th>Estado</th>
								<th>Acciones</th>
							</tr>
						</thead>
						<tbody>
							{payments.length === 0 ? (
								<tr className="table-empty">
									<td colSpan={5}>Sin ordenes aun. Use Crear orden.</td>
								</tr>
							) : (
								payments.map((payment) => (
									<tr key={payment.id}>
										<td className="cell-amount">CRC {formatAmount(payment.amount)}</td>
										<td>{payment.phone}</td>
										<td className="cell-reference">
											{getReferenceLabel(payment)}
										</td>
										<td>
											<span
												className={`status-chip status-${payment.status.toLowerCase()}`}
											>
												{statusLabels[payment.status]}
											</span>
										</td>
										<td>
											<div className="table-actions">
												<button
													type="button"
													className="ghost"
													onClick={() => updateStatus(payment.id, 'Valid')}
													disabled={
														payment.status !== 'Pending' || payment.syncState === 'syncing'
													}
												>
													Validar
												</button>
												<button
													type="button"
													className="ghost"
													onClick={() => updateStatus(payment.id, 'Rejected')}
													disabled={
														payment.status !== 'Pending' || payment.syncState === 'syncing'
													}
												>
													Rechazar
												</button>
											</div>
											{payment.syncState === 'syncing' ? (
												<span className="sync-note sync-note-live">Procesando pago...</span>
											) : null}
											{payment.syncState === 'failed' && payment.syncMessage ? (
												<span className="sync-note sync-note-error">
													{payment.syncMessage}
												</span>
											) : null}
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

export default PaymentsPage