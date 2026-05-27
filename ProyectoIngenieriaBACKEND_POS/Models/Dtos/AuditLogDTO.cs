using ProyectoIngenieriaBACKEND_POS.Models.Enums;

namespace ProyectoIngenieriaBACKEND_POS.Models.Dtos
{
    /// <summary>
    /// Lo que el frontend recibe cuando consulta el historial de eventos.
    /// Convierte los enums a strings legibles para que React no tenga
    /// que saber qué número corresponde a cada tipo de evento.
    /// </summary>
    public class AuditLogDTO
    {
        public int Id { get; set; }

        /// <summary>Fecha/hora del evento en formato ISO 8601. Ejemplo: "2026-03-14T12:35:00Z"</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Nombre del tipo de evento. Ejemplo: "DuplicateReference"</summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>Nombre del nivel de riesgo. Ejemplo: "High"</summary>
        public string RiskLevel { get; set; } = string.Empty;

        /// <summary>Descripción legible de lo que ocurrió.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Datos extra en JSON (puede ser null).</summary>
        public string? AdditionalData { get; set; }

        public int? PaymentId { get; set; }
        public int? OrderId { get; set; }
    }
}
