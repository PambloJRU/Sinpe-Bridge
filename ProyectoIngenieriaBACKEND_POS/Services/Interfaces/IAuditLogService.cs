using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    /// <summary>
    /// Contrato del servicio de auditoría.
    /// Cualquier clase del sistema que quiera registrar un evento
    /// depende de esta interface, NO de la implementación concreta.
    /// Eso facilita los tests y mantiene el código desacoplado.
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Registra un evento en la bitácora del sistema.
        /// Este método es el corazón de la Historia 12.
        /// </summary>
        /// <param name="eventType">Qué tipo de evento ocurrió (enum).</param>
        /// <param name="riskLevel">Qué tan grave es (enum).</param>
        /// <param name="description">Texto legible que explica el evento.</param>
        /// <param name="additionalData">JSON opcional con datos extra (puede ser null).</param>
        /// <param name="paymentId">ID del pago relacionado (si aplica).</param>
        /// <param name="orderId">ID de la orden relacionada (si aplica).</param>
        Task LogEventAsync(
            EventType eventType,
            RiskLevel riskLevel,
            string description,
            string? additionalData = null,
            int? paymentId = null,
            int? orderId = null);

        /// <summary>
        /// Retorna todos los logs registrados, del más reciente al más antiguo.
        /// Usado por el controlador para que el administrador consulte la bitácora.
        /// </summary>
        Task<List<AuditLogDTO>> GetAllLogsAsync();

        /// <summary>
        /// Retorna los logs filtrados por tipo de evento.
        /// Ejemplo: solo ver los DuplicateReference.
        /// </summary>
        Task<List<AuditLogDTO>> GetLogsByEventTypeAsync(EventType eventType);

        /// <summary>
        /// Retorna los logs filtrados por nivel de riesgo.
        /// Ejemplo: solo ver los High y Critical.
        /// </summary>
        Task<List<AuditLogDTO>> GetLogsByRiskLevelAsync(RiskLevel riskLevel);
    }
}
