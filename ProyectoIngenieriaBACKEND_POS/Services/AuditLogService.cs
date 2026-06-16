using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
 
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IAuditFileService _auditFileService;

        public AuditLogService(AppDbContext context, IAuditFileService auditFileService)
        {
            _context = context;
            _auditFileService = auditFileService;
        }

        // TAREA 01 Registrar un evento en el log (BD + archivo .txt)
        
        public async Task LogEventAsync(
            EventType eventType,
            RiskLevel riskLevel,
            string description,
            string? additionalData = null,
            int? paymentId = null,
            int? orderId = null)
        {
            //TAREA 02: Validaciones

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La descripción del evento no puede estar vacía.", nameof(description));

            if (description.Length > 1000)
                throw new ArgumentException("La descripción del evento no puede superar los 1000 caracteres.", nameof(description));

            if (!Enum.IsDefined(typeof(EventType), eventType))
                throw new ArgumentException($"El tipo de evento '{eventType}' no es válido.", nameof(eventType));

            if (!Enum.IsDefined(typeof(RiskLevel), riskLevel))
                throw new ArgumentException($"El nivel de riesgo '{riskLevel}' no es válido.", nameof(riskLevel));

            if (paymentId.HasValue)
            {
                var paymentExists = await _context.Payments.AnyAsync(p => p.Id == paymentId.Value);
                if (!paymentExists)
                    throw new ArgumentException($"No existe un pago con Id {paymentId.Value}.", nameof(paymentId));
            }

            if (orderId.HasValue)
            {
                var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderId.Value);
                if (!orderExists)
                    throw new ArgumentException($"No existe una orden con Id {orderId.Value}.", nameof(orderId));
            }

            //TAREA 01: se guarda en bd

            var log = new AuditLog
            {
                CreatedAt      = DateTime.UtcNow,
                EventType      = eventType,
                RiskLevel      = riskLevel,
                Description    = description.Trim(),
                AdditionalData = additionalData,
                PaymentId      = paymentId,
                OrderId        = orderId
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();

            //TAREA 01: Escribir en archivo .txt 
           await _auditFileService.WriteAuditEntryAsync(
                eventType,
                riskLevel,
                description.Trim(),
                additionalData,
                paymentId,
                orderId);
        }

       // TAREA03 — Clasificaciin y consulta de logs por tipo y nivel
        

       
        public async Task<List<AuditLogDTO>> GetAllLogsAsync()
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        public async Task<List<AuditLogDTO>> GetLogsByEventTypeAsync(EventType eventType)
        {
            if (!Enum.IsDefined(typeof(EventType), eventType))
                throw new ArgumentException($"El tipo de evento '{eventType}' no es válido.");

            return await _context.AuditLogs
                .AsNoTracking()
                .Where(l => l.EventType == eventType)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        
       
        public async Task<List<AuditLogDTO>> GetLogsByRiskLevelAsync(RiskLevel riskLevel)
        {
            if (!Enum.IsDefined(typeof(RiskLevel), riskLevel))
                throw new ArgumentException($"El nivel de riesgo '{riskLevel}' no es válido.");

            return await _context.AuditLogs
                .AsNoTracking()
                .Where(l => l.RiskLevel == riskLevel)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        private static AuditLogDTO MapToDto(AuditLog log) => new AuditLogDTO
        {
            Id             = log.Id,
            CreatedAt      = log.CreatedAt,
            EventType      = log.EventType.ToString(),
            RiskLevel      = log.RiskLevel.ToString(),
            Description    = log.Description,
            AdditionalData = log.AdditionalData,
            PaymentId      = log.PaymentId,
            OrderId        = log.OrderId
        };
    }
}
