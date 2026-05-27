using ProyectoIngenieriaBACKEND_POS.Models.Enums;

namespace ProyectoIngenieriaBACKEND_POS.Models.Entities
{
    /// <summary>
    /// Representa un evento registrado en la bitácora del sistema.
    /// Cada vez que ocurre algo relevante (pago válido, fraude, error),
    /// se inserta una fila en esta tabla.
    ///
    /// Esta tabla NUNCA se modifica después de insertada: es solo lectura
    /// para consulta. Los logs son inmutables por diseño (auditoría real).
    /// </summary>
    public class AuditLog
    {
        /// <summary>Identificador único del registro de log.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha y hora exacta en que ocurrió el evento.
        /// Siempre se guarda en UTC para evitar problemas de zona horaria.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Qué tipo de evento ocurrió.
        /// Ejemplo: DuplicateReference, PaymentConfirmed, OrderExpired.
        /// </summary>
        public EventType EventType { get; set; }

        /// <summary>
        /// Qué tan grave es este evento.
        /// Ejemplo: High para fraude, Low para pago exitoso.
        /// </summary>
        public RiskLevel RiskLevel { get; set; }

        /// <summary>
        /// Texto legible que explica qué pasó.
        /// Ejemplo: "Referencia 2026031412350018427634521 ya fue procesada el 14/03/2026."
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Datos adicionales en formato JSON (opcional).
        /// Se usa para guardar información extra sin crear columnas nuevas.
        /// Ejemplo: { "SenderNumber": "88881234", "AttemptedReference": "202603..." }
        /// </summary>
        public string? AdditionalData { get; set; }

        // ── Claves foráneas opcionales ─────────────────────────────────────────
        // Son opcionales porque no todos los eventos tienen un pago u orden asociado.
        // Ejemplo: PhoneDisconnected no tiene PaymentId ni OrderId.

        /// <summary>ID del pago relacionado con este evento (si aplica).</summary>
        public int? PaymentId { get; set; }

        /// <summary>ID de la orden relacionada con este evento (si aplica).</summary>
        public int? OrderId { get; set; }

        // ── Navegación ─────────────────────────────────────────────────────────
        public virtual Payment? Payment { get; set; }
        public virtual Order? Order { get; set; }
    }
}
