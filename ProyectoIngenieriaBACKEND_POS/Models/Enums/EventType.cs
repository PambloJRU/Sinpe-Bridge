namespace ProyectoIngenieriaBACKEND_POS.Models.Enums
{
    /// <summary>
    /// Clasifica qué tipo de evento ocurrió en el sistema.
    /// Se usa junto con RiskLevel para que el administrador pueda
    /// filtrar los logs por categoría y prioridad.
    /// </summary>
    public enum EventType
    {
        // ── Pagos exitosos ─────────────────────────────────────────
        /// <summary>Pago SINPE recibido, validado y orden confirmada correctamente.</summary>
        PaymentConfirmed = 1,

        /// <summary>Pago recibido pero no existía una orden activa que coincida. Se guardó en espera.</summary>
        PaymentWithoutOrder = 2,

        // ── Fraude y seguridad ─────────────────────────────────────
        /// <summary>Se intentó usar una referencia SINPE que ya fue procesada antes.</summary>
        DuplicateReference = 3,

        /// <summary>El pago fue realizado hace más de 15 minutos (fuera de la ventana válida).</summary>
        PaymentExpired = 4,

        /// <summary>El monto del SMS no coincide con ninguna orden pendiente del cliente.</summary>
        AmountMismatch = 5,

        /// <summary>Se recibió un mensaje desde un dispositivo que no es el teléfono registrado del negocio.</summary>
        UnauthorizedDevice = 6,

        // ── Órdenes ────────────────────────────────────────────────
        /// <summary>Una orden cumplió 30 minutos sin recibir pago y fue expirada automáticamente. (Historia 09)</summary>
        OrderExpired = 7,

        // ── Revisión manual ────────────────────────────────────────
        /// <summary>El sistema no pudo decidir automáticamente y envió el pago a revisión manual. (Historia 10)</summary>
        ManualReviewRequired = 8,

        /// <summary>El administrador aprobó manualmente un pago en revisión. (Historia 10)</summary>
        ManualReviewApproved = 9,

        /// <summary>El administrador rechazó manualmente un pago en revisión. (Historia 10)</summary>
        ManualReviewRejected = 10,

        // ── Sistema ────────────────────────────────────────────────
        /// <summary>El teléfono receptor lleva más de 15 minutos sin comunicarse con el sistema.</summary>
        PhoneDisconnected = 11,

        /// <summary>El teléfono receptor volvió a conectarse al sistema.</summary>
        PhoneReconnected = 12
    }
}
