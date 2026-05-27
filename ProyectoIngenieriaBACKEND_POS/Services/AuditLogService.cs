using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    /// <summary>
    /// Implementación del servicio de auditoría (Historia 12).
    ///
    /// TAREA 01: Método LogEventAsync que registra eventos en la BD.
    /// TAREA 02: Validaciones antes de guardar (descripción vacía, enums válidos, etc.)
    /// TAREA 03: Clasificación por EventType y RiskLevel con métodos de filtrado.
    ///
    /// Este servicio es INYECTADO en cualquier otro servicio que necesite
    /// registrar algo. Actualmente lo usa SmsReceiverService.
    /// En el futuro lo usarán los servicios de expiración (H09) y revisión manual (H10).
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        // Inyección de dependencia: el contexto de la BD llega automáticamente
        // porque lo registramos en Program.cs con AddScoped.
        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════════════════════════════
        // TAREA 01 — Registrar un evento en el log
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Registra un evento en la tabla AuditLogs de la base de datos.
        ///
        /// FLUJO:
        ///   1. Valida los parámetros de entrada (Tarea 02).
        ///   2. Crea una instancia de AuditLog con los datos recibidos.
        ///   3. La inserta en la BD.
        ///
        /// Si ocurre un error al guardar, se lanza excepción para que el
        /// llamador sepa que el log no se pudo registrar.
        /// </summary>
        public async Task LogEventAsync(
            EventType eventType,
            RiskLevel riskLevel,
            string description,
            string? additionalData = null,
            int? paymentId = null,
            int? orderId = null)
        {
            // ── TAREA 02: Validaciones ──────────────────────────────────────────

            // Validar que la descripción no esté vacía.
            // Sin descripción, el log no sirve para nada — no se sabe qué pasó.
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La descripción del evento no puede estar vacía.", nameof(description));

            // Validar que la descripción no sea demasiado larga para la BD.
            // SQL Server tiene un límite, mejor cortarlo nosotros con un mensaje claro.
            if (description.Length > 1000)
                throw new ArgumentException("La descripción del evento no puede superar los 1000 caracteres.", nameof(description));

            // Validar que el EventType sea un valor definido en el enum.
            // Esto evita que alguien pase un número inválido como (EventType)999.
            if (!Enum.IsDefined(typeof(EventType), eventType))
                throw new ArgumentException($"El tipo de evento '{eventType}' no es válido.", nameof(eventType));

            // Validar que el RiskLevel sea un valor definido en el enum.
            if (!Enum.IsDefined(typeof(RiskLevel), riskLevel))
                throw new ArgumentException($"El nivel de riesgo '{riskLevel}' no es válido.", nameof(riskLevel));

            // Validar que si se proporciona paymentId, el pago exista en la BD.
            // No queremos logs apuntando a pagos inexistentes (clave foránea rota).
            if (paymentId.HasValue)
            {
                var paymentExists = await _context.Payments.AnyAsync(p => p.Id == paymentId.Value);
                if (!paymentExists)
                    throw new ArgumentException($"No existe un pago con Id {paymentId.Value}.", nameof(paymentId));
            }

            // Validar que si se proporciona orderId, la orden exista en la BD.
            if (orderId.HasValue)
            {
                var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderId.Value);
                if (!orderExists)
                    throw new ArgumentException($"No existe una orden con Id {orderId.Value}.", nameof(orderId));
            }

            // ── TAREA 01: Crear y guardar el log ───────────────────────────────

            var log = new AuditLog
            {
                // CreatedAt se inicializa automáticamente en UTC por la entidad,
                // pero lo forzamos aquí para ser explícitos.
                CreatedAt = DateTime.UtcNow,
                EventType = eventType,
                RiskLevel = riskLevel,
                Description = description.Trim(),
                AdditionalData = additionalData,
                PaymentId = paymentId,
                OrderId = orderId
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════════════════════════
        // TAREA 03 — Clasificación y consulta de logs por tipo y nivel
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Retorna todos los logs del más reciente al más antiguo,
        /// convertidos a DTO para que el frontend los pueda usar directamente.
        /// </summary>
        public async Task<List<AuditLogDTO>> GetAllLogsAsync()
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)   // primero los más recientes
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        /// <summary>
        /// Retorna solo los logs de un tipo de evento específico.
        /// Ejemplo: todos los DuplicateReference para ver intentos de fraude.
        /// </summary>
        public async Task<List<AuditLogDTO>> GetLogsByEventTypeAsync(EventType eventType)
        {
            // TAREA 03: Filtrado por tipo — aquí está la clasificación por tipo de evento
            if (!Enum.IsDefined(typeof(EventType), eventType))
                throw new ArgumentException($"El tipo de evento '{eventType}' no es válido.");

            return await _context.AuditLogs
                .AsNoTracking()
                .Where(l => l.EventType == eventType)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        /// <summary>
        /// Retorna solo los logs de un nivel de riesgo específico.
        /// Ejemplo: solo los High y Critical para que el admin vea lo urgente.
        /// </summary>
        public async Task<List<AuditLogDTO>> GetLogsByRiskLevelAsync(RiskLevel riskLevel)
        {
            // TAREA 03: Filtrado por nivel de riesgo
            if (!Enum.IsDefined(typeof(RiskLevel), riskLevel))
                throw new ArgumentException($"El nivel de riesgo '{riskLevel}' no es válido.");

            return await _context.AuditLogs
                .AsNoTracking()
                .Where(l => l.RiskLevel == riskLevel)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        // ════════════════════════════════════════════════════════════════════
        // Método privado de mapeo — entidad → DTO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Convierte una entidad AuditLog al DTO que recibe el frontend.
        /// Convierte los enums a strings legibles (ej: EventType.DuplicateReference → "DuplicateReference").
        /// Es privado porque solo lo usa este servicio internamente.
        /// </summary>
        private static AuditLogDTO MapToDto(AuditLog log) => new AuditLogDTO
        {
            Id = log.Id,
            CreatedAt = log.CreatedAt,
            EventType = log.EventType.ToString(),       // enum a string legible
            RiskLevel = log.RiskLevel.ToString(),       // enum a string legible
            Description = log.Description,
            AdditionalData = log.AdditionalData,
            PaymentId = log.PaymentId,
            OrderId = log.OrderId
        };
    }
}
