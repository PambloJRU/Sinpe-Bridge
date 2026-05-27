import type { PaymentInfo } from "../model/PaymentInfo"

const apiBaseUrl =
	(import.meta.env.VITE_API_BASE_URL as string | undefined) ??
	'http://localhost:5198'

const paymentEndpoint = `${apiBaseUrl.replace(/\/$/, '')}/api/Payments/list`   

export const getPaymentsInfo = async (): Promise<PaymentInfo[]> => {
    try {
        const response = await fetch(paymentEndpoint);
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || 'Error al obtener la información de los pagos');
        }
        
        const data: PaymentInfo[] = await response.json();
        console.log("PAGOS: " , data)
        return data;
    } catch (error) {
        console.error('Error fetching payments info: ', error);
        throw error;
    }
};
    
