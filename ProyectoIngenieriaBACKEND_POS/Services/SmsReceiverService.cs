using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class SmsReceiverService : ISmsReceiverService
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public SmsReceiverService(
            AppDbContext context,
            IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        private static readonly Regex SinpeSmsRegex = new Regex(
            "^SINPE Movil: Ha recibido una transferencia de (?<payer>[A-Za-zÁÉÍÓÚáéíóúÑñ ]+) por (?<amount>[0-9]+(?:[.,][0-9]{1,2})?) colones\\. Ref: (?<reference>[0-9]+)\\.$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public async Task<ParsedSmsResult?> ProcessIncomingSmsAsync(SmsRequestDTO smsData)
        {
            if (string.IsNullOrWhiteSpace(smsData.MessageBody))
                return null;

            var match = SinpeSmsRegex.Match(smsData.MessageBody.Trim());

            if (!match.Success)
                return null;

            var amountText = match.Groups["amount"].Value;

            if (!TryParseAmount(amountText, out var amount))
                return null;

            var result = new ParsedSmsResult
            {
                Amount = amount,
                PayerName = match.Groups["payer"].Value.Trim(),
                Reference = match.Groups["reference"].Value.Trim()
            };

            result.PaymentDateTime = ExtractPaymentDateTime(result.Reference);

            // HISTORIA 04
            var paymentDateTime = ExtractPaymentDateTime(result.Reference);

            if (!IsPaymentWithin15Minutes(result.PaymentDateTime))
            {
                await _auditLogService.LogEventAsync(
                    EventType.PaymentExpired,
                    RiskLevel.Medium,
                    $"Pago rechazado por antigüedad. Referencia: {result.Reference}",
                    BuildAuditData(
                        result.Reference,
                        result.Amount,
                        smsData.SenderNumber,
                        "Pago fuera del tiempo permitido de 15 minutos.",
                        "SmsReceiverService",
                        result.PayerName,
                        smsData.ReceivedAt,
                        result.PaymentDateTime
                    )
                );

                throw new InvalidOperationException("PAYMENT_EXPIRED");
            }

            //HISTORIA 03 TAREA 01,02 y 03
            if (await _context.Payments.AnyAsync(r => r.Reference == result.Reference))
            {
                var clientTemp = _context.Clients.FirstOrDefault(c => c.Phone == smsData.SenderNumber);

                DuplecateReference dupe = new DuplecateReference
                {
                    Cellphone = smsData.SenderNumber,
                    IdClient = clientTemp?.Id
                };

                _context.DuplecateReferences.Add(dupe);
                await _context.SaveChangesAsync();

                await _auditLogService.LogEventAsync(
                    EventType.DuplicateReference,
                    RiskLevel.High,
                    $"Referencia duplicada detectada: {result.Reference} desde {smsData.SenderNumber}",
                    BuildAuditData(
                        result.Reference,
                        result.Amount,
                        smsData.SenderNumber,
                        "La referencia SINPE ya había sido registrada previamente.",
                        "SmsReceiverService",
                        result.PayerName,
                        smsData.ReceivedAt,
                        result.PaymentDateTime
                    )
                );

                throw new InvalidOperationException("DUPLICATE_REFERENCE");
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Phone == smsData.SenderNumber);

            if (client == null)
            {
                client = new Client
                {
                    Name = result.PayerName,
                    Phone = smsData.SenderNumber
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            var payment = new Payment
            {
                ClientId = client.Id,
                Amount = result.Amount,
                Reference = result.Reference,
                ReceivedAt = smsData.ReceivedAt,
                OriginalMessage = smsData.MessageBody,
                Status = PaymentStatus.Pending
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            //acá se busca una orden de pago que coincida con los datos del pago recibido y se le cambia el estado a Válido
            try
            {
                var order = await _context.Orders
                    .AsNoTracking()
                    .Where(o =>
                        o.Amount == payment.Amount &&
                        o.Phone == client.Phone &&
                        !o.PaymentId.HasValue &&
                        o.State == "PENDIENTE")
                    .FirstOrDefaultAsync();

                Console.WriteLine($"[SMS] Búsqueda exacta: Amount={payment.Amount}, Phone={client.Phone}");
                Console.WriteLine($"[SMS] Orden encontrada: {(order != null ? order.Id : "NO")}");

                if (order != null)
                {
                    Console.WriteLine($"[SMS] Coincidencia exacta, aprobando pago");
                    order.PaymentId = payment.Id;
                    order.State = "PAGADA";
                    payment.Status = PaymentStatus.Valid;
                    _context.Orders.Update(order);
                    _context.Payments.Update(payment);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"[SMS] Sin coincidencia exacta, buscando por teléfono");
                    var orderByPhone = await _context.Orders
                        .Where(o =>
                            o.Phone == client.Phone &&
                            !o.PaymentId.HasValue &&
                            o.State == "PENDIENTE")
                        .FirstOrDefaultAsync();

                    if (orderByPhone != null)
                    {
                        orderByPhone.PaymentId = payment.Id;
                        orderByPhone.State = "PAGO_PARCIAL";
                        payment.Status = PaymentStatus.PendingReview;
                        _context.Orders.Update(orderByPhone);
                        _context.Payments.Update(payment);
                        await _context.SaveChangesAsync();

                        await _auditLogService.LogEventAsync(
                            EventType.AmountMismatch,
                            RiskLevel.High,
                            $"Monto incorrecto detectado. Referencia: {payment.Reference}",
                            BuildAuditData(
                                payment.Reference,
                                payment.Amount,
                                client.Phone,
                                "El teléfono coincide con una orden pendiente, pero el monto recibido no coincide.",
                                "SmsReceiverService",
                                result.PayerName,
                                smsData.ReceivedAt,
                                result.PaymentDateTime,
                                orderByPhone.Amount
                            ),
                            payment.Id,
                            orderByPhone.Id
                        );

                        await _auditLogService.LogEventAsync(
                            EventType.ManualReviewRequired,
                            RiskLevel.Medium,
                            $"Pago enviado a revisión manual por diferencia de monto. Referencia: {payment.Reference}",
                            BuildAuditData(
                                payment.Reference,
                                payment.Amount,
                                client.Phone,
                                "Se requiere revisión manual porque el monto recibido no coincide con el monto de la orden.",
                                "SmsReceiverService",
                                result.PayerName,
                                smsData.ReceivedAt,
                                result.PaymentDateTime,
                                orderByPhone.Amount
                            ),
                            payment.Id,
                            orderByPhone.Id
                        );
                    }
                    else
                    {
                        payment.Status = PaymentStatus.PendingReview;
                        _context.Payments.Update(payment);
                        await _context.SaveChangesAsync();

                        await _auditLogService.LogEventAsync(
                            EventType.PaymentWithoutOrder,
                            RiskLevel.Medium,
                            $"Pago recibido sin orden asociada. Referencia: {payment.Reference}",
                            BuildAuditData(
                                payment.Reference,
                                payment.Amount,
                                client.Phone,
                                "No existe ninguna orden pendiente asociada al teléfono y monto del pago.",
                                "SmsReceiverService",
                                result.PayerName,
                                smsData.ReceivedAt,
                                result.PaymentDateTime
                            ),
                            payment.Id
                        );

                        await _auditLogService.LogEventAsync(
                            EventType.ManualReviewRequired,
                            RiskLevel.Medium,
                            $"Pago enviado a revisión manual por no tener orden asociada. Referencia: {payment.Reference}",
                            BuildAuditData(
                                payment.Reference,
                                payment.Amount,
                                client.Phone,
                                "Se requiere revisión manual porque el pago no pudo asociarse automáticamente a una orden.",
                                "SmsReceiverService",
                                result.PayerName,
                                smsData.ReceivedAt,
                                result.PaymentDateTime
                            ),
                            payment.Id
                        );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private static string BuildAuditData(
            string reference,
            decimal amount,
            string phone,
            string reason,
            string source,
            string? payerName = null,
            DateTime? receivedAt = null,
            DateTime? paymentDateTime = null,
            decimal? orderAmount = null)
        {
            return JsonSerializer.Serialize(new
            {
                Reference = reference,
                Amount = amount,
                Phone = phone,
                Reason = reason,
                Source = source,
                PayerName = payerName,
                ReceivedAt = receivedAt,
                PaymentDateTime = paymentDateTime,
                OrderAmount = orderAmount
            });
        }

        private static bool TryParseAmount(string amountText, out decimal amount)
        {
            var normalized = amountText.Replace(',', '.');

            return decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out amount);
        }

        private static DateTime ExtractPaymentDateTime(string reference)
        {
            if (reference.Length < 14)
                throw new InvalidOperationException("INVALID_REFERENCE_FORMAT");

            var datePart = reference.Substring(0, 14);

            return DateTime.ParseExact(
                datePart,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture
            );
        }

        private static bool IsPaymentWithin15Minutes(DateTime paymentDateTime)
        {
            var currentTime = DateTime.Now;
            var difference = currentTime - paymentDateTime;

            return difference.TotalMinutes >= 0 &&
                   difference.TotalMinutes <= 15;
        }
    }
}