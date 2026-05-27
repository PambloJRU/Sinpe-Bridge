import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { getPaymentsInfo } from '../services/paymentService'
import type { PaymentInfo } from '../model/PaymentInfo'
import SmsModal from '../components/SmsModalProps'

const formatAmount = (value: number) =>
    value.toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    })

function PaymentHistoryPage() {
    const [payments, setPayments] = useState<PaymentInfo[]>([])
    const [searchTerm, setSearchTerm] = useState('') 
    const [currentPage, setCurrentPage] = useState(1) 
    const [selectedMessage, setSelectedMessage] = useState<string | null>(null) // NUEVO: Estado para el modal
    
    const itemsPerPage = 15 

    useEffect(() => {
        loadPayments()
    }, [])

    const loadPayments = async () => {
        try {
            const response = await getPaymentsInfo()
            setPayments(response)
        } catch (error) {
            console.error('Error cargando historial de pagos en el front', error)
        }
    }

    const filteredPayments = useMemo(() => {
        const term = searchTerm.toLowerCase().trim()
        return payments.filter(payment => {
            const amountMatch = payment.amount?.toString().includes(term)
            const clientMatch = payment.clientName?.toLowerCase().includes(term)
            return amountMatch || clientMatch
        })
    }, [payments, searchTerm])

    useEffect(() => {
        setCurrentPage(1)
    }, [searchTerm])

    const totalPages = Math.ceil(filteredPayments.length / itemsPerPage)
    
    const paginatedPayments = useMemo(() => {
        const startIndex = (currentPage - 1) * itemsPerPage
        const endIndex = startIndex + itemsPerPage
        return filteredPayments.slice(startIndex, endIndex)
    }, [filteredPayments, currentPage])

    return (
        <div className="pos-shell">
            <header className="pos-header">
                <div className="pos-brand">
                    <p className="pos-kicker">SINPE POS</p>
                    <h1>Historial de Pagos</h1>
                    <p className="pos-subtitle">
                        Búsqueda de Pagos registrados por Monto o Cliente.
                    </p>
                </div>
            </header>

            <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: '15px' }}>
                <Link to="/">
                    <button className="ghost">Volver</button>
                </Link>
            </div>

            <section className="pos-card pos-table">
                <div className="pos-table-top">
                    <div>
                        <h2>Consultar historial</h2>
                        <p className="pos-subtitle">
                            Filtre pagos por cliente o monto
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
                    placeholder="Buscar por teléfono, nombre o monto..."
                    value={searchTerm}
                    onChange={e => setSearchTerm(e.target.value)}
                />

                <div className="table-wrapper">
                    <table className="payments-table">
                        <thead>
                            <tr>
                                <th>Cliente</th>
                                <th>Monto</th>
                                <th>Referencia</th>
                                <th>Fecha</th>
                                <th style={{ textAlign: 'center' }}>Mensaje</th> {/* NUEVA COLUMNA */}
                            </tr>
                        </thead>

                        <tbody>
                            {paginatedPayments.length === 0 ? (
                                <tr className="table-empty">
                                    <td colSpan={5}>
                                        No se encontraron pagos que coincidan.
                                    </td>
                                </tr>
                            ) : (
                                paginatedPayments.map(payment => (
                                    <tr key={payment.reference}>
                                        <td>{payment.clientName}</td>
                                        <td className="font-mono amount-cell">
                                            ₡{formatAmount(payment.amount)}
                                        </td>
                                        <td>{payment.reference}</td>
                                        <td>{new Date(payment.receivedAt).toLocaleString()}</td>
                                        
                                        {/* NUEVO BOTÓN CON ICONO DE OJO */}
                                        <td style={{ textAlign: 'center' }}>
                                            <button 
                                                className="ghost" 
                                                style={{ padding: '6px 10px', borderRadius: '8px' }}
                                                onClick={() => setSelectedMessage(payment.originalMessage)}
                                                title="Ver SMS Original"
                                            >
                                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                                                    <circle cx="12" cy="12" r="3"></circle>
                                                </svg>
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
                
                {/* CONTROLES DE PAGINACIÓN */}
                {totalPages > 1 && (
                    <div className="pagination-controls" style={{
                        display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '20px', padding: '10px 5px'
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
                            Página {currentPage} de {totalPages} ({filteredPayments.length} resultados)
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
            {/*VENTANA MODAL EMERGENTE PARA EL MENSAJE */}
            <SmsModal 
                message={selectedMessage} 
                onClose={() => setSelectedMessage(null)} 
            />
        </div>
    )
}

export default PaymentHistoryPage