interface SmsModalProps {
    message: string | null;
    onClose: () => void;
}

function SmsModal({ message, onClose }: SmsModalProps) {
    if (!message) return null;

    return (
        <div style={{
            position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.6)', backdropFilter: 'blur(3px)',
            display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 1000
        }} onClick={onClose}>
            <div style={{
                backgroundColor: '#fff', padding: '30px', borderRadius: '16px',
                maxWidth: '500px', width: '90%', boxShadow: '0 20px 40px rgba(0,0,0,0.2)'
            }} onClick={(e) => e.stopPropagation()}>
                <h3 style={{ marginTop: 0, marginBottom: '15px', color: '#333' }}>
                    Comprobante SMS Original
                </h3>
                <div style={{
                    backgroundColor: '#f8f9fa', padding: '20px', borderRadius: '12px',
                    border: '1px solid #e9ecef', fontFamily: 'monospace',
                    fontSize: '15px', lineHeight: '1.6', color: '#495057', wordBreak: 'break-word'
                }}>
                    {message}
                </div>
                <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '25px' }}>
                    <button 
                        className="ghost" 
                        style={{ border: '1px solid #ddd', padding: '10px 20px' }}
                        onClick={onClose}
                    >
                        Cerrar
                    </button>
                </div>
            </div>
        </div>
    );
}

export default SmsModal;