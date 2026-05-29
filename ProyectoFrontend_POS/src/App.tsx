import { BrowserRouter, Routes, Route } from 'react-router-dom'
import PaymentsPage from './features/payments/pages/PaymentsPage'
import OrderHistoryPage from './features/payments/pages/OrderHistoryPage'
import PaymentHistoryPage from './features/payments/pages/PaymentHistoryInfo'
import PaymentsReviewPage from './features/payments/pages/PaymentReviewPage'

function App() {
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/" element={<PaymentsPage />} />
				<Route path="/historial" element={<OrderHistoryPage />} />
				<Route path="/pagos" element={<PaymentHistoryPage/>}/>
				<Route path="/pagos-revision" element={<PaymentsReviewPage/>}/>
			</Routes>
		</BrowserRouter>
	)
}

export default App