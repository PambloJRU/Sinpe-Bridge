import { BrowserRouter, Routes, Route } from 'react-router-dom'
import PaymentsPage from '../features/payments/pages/PaymentsPage'
import OrderHistoryPage from '../features/payments/pages/OrderHistoryPage'

function AppRouter() {
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/" element={<PaymentsPage />} />
				<Route path="/history" element={<OrderHistoryPage />} />
			</Routes>
		</BrowserRouter>
	)
}

export default AppRouter