using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class AuditFileService : IAuditFileService
    {
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AuditFileService> _logger;

        public AuditFileService(IWebHostEnvironment env, ILogger<AuditFileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task WriteAuditEntryAsync(
            EventType eventType,
            RiskLevel riskLevel,
            string description,
            string? additionalData = null,
            int? paymentId = null,
            int? orderId = null)
        {
            var folder = Path.Combine(_env.ContentRootPath, "Logs", "Auditoria");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var filePath = Path.Combine(folder, $"auditoria-{today}.txt");

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var entry = BuildEntry(timestamp, eventType, riskLevel, description,
                                   additionalData, paymentId, orderId);

            await _fileLock.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(filePath, entry, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "No se pudo escribir el log de auditoría en disco. Archivo: {FilePath}", filePath);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private static string BuildEntry(
            string timestamp,
            EventType eventType,
            RiskLevel riskLevel,
            string description,
            string? additionalData,
            int? paymentId,
            int? orderId)
        {
            var auditData = TryReadAuditData(additionalData);

            var sb = new StringBuilder();

            sb.AppendLine("------------------------------------------------------------------");
            sb.AppendLine(" SINPE BRIDGE - REGISTRO DE AUDITORÍA");
            sb.AppendLine("------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"FECHA/HORA DEL EVENTO : {timestamp} UTC");
            sb.AppendLine($"TIPO DE EVENTO        : {TranslateEventType(eventType)}");
            sb.AppendLine($"NIVEL DE RIESGO       : {TranslateRiskLevel(riskLevel)}");
            sb.AppendLine($"REFERENCIA SINPE      : {GetValue(auditData, "Reference")}");
            sb.AppendLine($"MONTO                 : {FormatAmount(GetValue(auditData, "Amount"))}");
            sb.AppendLine($"TELÉFONO DEL CLIENTE  : {GetValue(auditData, "Phone")}");
            sb.AppendLine($"NOMBRE DEL CLIENTE    : {GetValue(auditData, "PayerName")}");
            sb.AppendLine($"MONTO DE LA ORDEN     : {FormatAmount(GetValue(auditData, "OrderAmount"))}");
            sb.AppendLine($"PAGO ID               : {(paymentId.HasValue ? paymentId.Value.ToString() : "-")}");
            sb.AppendLine($"ORDEN ID              : {(orderId.HasValue ? orderId.Value.ToString() : "-")}");
            sb.AppendLine($"ORIGEN DEL EVENTO     : {TranslateSource(GetValue(auditData, "Source"))}");
            sb.AppendLine("DESCRIPCIÓN  :");
            sb.AppendLine($"  {description}");

            var reason = GetValue(auditData, "Reason");
            if (reason != "-")
            {
                sb.AppendLine("DETALLE DEL MOTIVO    :");
                sb.AppendLine($"  {reason}");
            }

            sb.AppendLine("------------------------------------------------------------------");
            sb.AppendLine();

            return sb.ToString();
        }

        private static Dictionary<string, string> TryReadAuditData(string? additionalData)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(additionalData))
                return result;

            try
            {
                using var document = JsonDocument.Parse(additionalData);

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    result[property.Name] = property.Value.ValueKind == JsonValueKind.Null
                        ? "-"
                        : property.Value.ToString();
                }
            }
            catch
            {
                result["RawData"] = additionalData;
            }

            return result;
        }

        private static string GetValue(Dictionary<string, string> data, string key)
        {
            if (data.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;

            return "-";
        }

        private static string FormatAmount(string value)
        {
            if (value == "-" || string.IsNullOrWhiteSpace(value))
                return "-";

            if (decimal.TryParse(value, out var amount))
                return $"₡{amount:N2}";

            return value;
        }

        private static string TranslateEventType(EventType eventType)
        {
            return eventType switch
            {
                EventType.PaymentConfirmed => "Pago Confirmado",
                EventType.PaymentWithoutOrder => "Pago Sin Orden Asociada",
                EventType.DuplicateReference => "Referencia Duplicada",
                EventType.PaymentExpired => "Pago Fuera del Tiempo Permitido",
                EventType.AmountMismatch => "Monto Incorrecto",
                EventType.UnauthorizedDevice => "Dispositivo No Autorizado",
                EventType.OrderExpired => "Orden Vencida",
                EventType.ManualReviewRequired => "Revisión Manual Requerida",
                EventType.ManualReviewApproved => "Revisión Manual Aprobada",
                EventType.ManualReviewRejected => "Revisión Manual Rechazada",
                EventType.PhoneDisconnected => "Teléfono Receptor Desconectado",
                EventType.PhoneReconnected => "Teléfono Receptor Reconectado",
                _ => eventType.ToString()
            };
        }

        private static string TranslateRiskLevel(RiskLevel riskLevel)
        {
            return riskLevel switch
            {
                RiskLevel.Low => "Bajo",
                RiskLevel.Medium => "Medio",
                RiskLevel.High => "Alto",
                RiskLevel.Critical => "Crítico",
                _ => riskLevel.ToString()
            };
        }

        private static string TranslateSource(string source)
        {
            return source switch
            {
                "SmsReceiverService" => "Servicio de Recepción de SMS",
                "PhoneConnectionService" => "Servicio de Conexión Telefónica",
                "-" => "-",
                _ => source
            };
        }
    }
}