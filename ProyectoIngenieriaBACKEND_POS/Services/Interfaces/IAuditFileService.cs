using ProyectoIngenieriaBACKEND_POS.Models.Enums;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    /// <summary>
    /// Contrato del servicio que escribe eventos de auditoría en archivos .txt.
    ///
    /// Se separa del IAuditLogService (base de datos) para respetar el
    /// principio de responsabilidad única: una clase guarda en BD,
    /// otra guarda en disco.
    ///
    /// AuditLogService depende de esta interfaz, no de la implementación,
    /// lo que facilita pruebas y desacoplamiento.
    /// </summary>
    public interface IAuditFileService
    {
        /// <summary>
        /// Escribe una entrada de auditoría en el archivo .txt del día actual.
        /// El archivo se genera en: Logs/Auditoria/auditoria-YYYY-MM-DD.txt
        ///
        /// El método es thread-safe: usa un lock para evitar colisiones
        /// cuando múltiples peticiones llegan al mismo tiempo.
        /// </summary>
        /// <param name="eventType">Tipo de evento (enum → string legible).</param>
        /// <param name="riskLevel">Nivel de riesgo (enum → string legible).</param>
        /// <param name="description">Descripción legible del evento.</param>
        /// <param name="additionalData">Datos JSON extras (puede ser null).</param>
        /// <param name="paymentId">ID del pago relacionado (si aplica).</param>
        /// <param name="orderId">ID de la orden relacionada (si aplica).</param>
        Task WriteAuditEntryAsync(
            EventType eventType,
            RiskLevel riskLevel,
            string description,
            string? additionalData = null,
            int? paymentId = null,
            int? orderId = null);
    }
}
