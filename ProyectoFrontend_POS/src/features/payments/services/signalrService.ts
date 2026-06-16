import * as signalR from "@microsoft/signalr"

const apiBaseUrl =
	(import.meta.env.VITE_API_BASE_URL as string | undefined) ??
	'http://localhost:5198'

const hubUrl = `${apiBaseUrl.replace(/\/$/, '')}/hubs/notifications`

let connection: signalR.HubConnection | null = null

export type OrderStatusPayload = {
	orderId: number
	state: string
	paymentId: number | null
	autoMatch?: boolean
}

export type PhoneStatusPayload = {
	isConnected: boolean
	lastHeartbeatUtc: string | null
	minutesSinceLastHeartbeat: number | null
}

export type PaymentReviewedPayload = {
	paymentId: number
	approved: boolean
}

type ListenerCallbacks = {
	onOrderStatus?: (payload: OrderStatusPayload) => void
	onPhoneStatus?: (payload: PhoneStatusPayload) => void
	onPaymentReviewed?: (payload: PaymentReviewedPayload) => void
}

let currentListeners: ListenerCallbacks = {}

export const startSignalRConnection = async (listeners: ListenerCallbacks): Promise<void> => {
	if (connection) {
		await stopSignalRConnection()
	}

	currentListeners = listeners

	connection = new signalR.HubConnectionBuilder()
		.withUrl(hubUrl)
		.withAutomaticReconnect({
			nextRetryDelayInMilliseconds: (retryContext) => {
				if (retryContext.elapsedMilliseconds < 30000) {
					return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 10000)
				}
				return 5000
			}
		})
		.configureLogging(signalR.LogLevel.Warning)
		.build()

	connection.on("OrderStatus", (payload: OrderStatusPayload) => {
		console.log("[SignalR] OrderStatus:", payload)
		currentListeners.onOrderStatus?.(payload)
	})

	connection.on("PhoneStatus", (payload: PhoneStatusPayload) => {
		console.log("[SignalR] PhoneStatus:", payload)
		currentListeners.onPhoneStatus?.(payload)
	})

	connection.on("PaymentReviewed", (payload: PaymentReviewedPayload) => {
		console.log("[SignalR] PaymentReviewed:", payload)
		currentListeners.onPaymentReviewed?.(payload)
	})

	connection.onreconnecting(() => {
		console.log("[SignalR] Reconectando...")
	})

	connection.onreconnected(() => {
		console.log("[SignalR] Reconectado exitosamente")
	})

	connection.onclose((error) => {
		console.warn("[SignalR] Conexion cerrada:", error)
	})

	try {
		await connection.start()
		console.log("[SignalR] Conectado a", hubUrl)
	} catch (err) {
		console.error("[SignalR] Error al conectar:", err)
		throw err
	}
}

export const stopSignalRConnection = async (): Promise<void> => {
	if (connection) {
		await connection.stop()
		connection = null
		currentListeners = {}
		console.log("[SignalR] Desconectado")
	}
}

export const getConnectionState = (): signalR.HubConnectionState | null => {
	return connection?.state ?? null
}
