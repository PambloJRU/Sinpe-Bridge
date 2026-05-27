import { BrowserRouter, Routes, Route } from 'react-router-dom'
import PaymentsPage from './features/payments/pages/PaymentsPage'
import OrderHistoryPage from './features/payments/pages/OrderHistoryPage'
import PaymentHistoryPage from './features/payments/pages/PaymentHistoryInfo'

function App() {
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/" element={<PaymentsPage />} />
				<Route path="/historial" element={<OrderHistoryPage />} />
				<Route path="/pagos" element={<PaymentHistoryPage/>}/>
			</Routes>
		</BrowserRouter>
	)
}

export default App