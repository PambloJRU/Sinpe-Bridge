using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using System.Globalization;
using System.Text.RegularExpressions;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class SmsReceiverService : ISmsReceiverService
    {
        private readonly AppDbContext _context;

        public SmsReceiverService(AppDbContext context)
        {
            _context = context;
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
                throw new InvalidOperationException("PAYMENT_EXPIRED");
            }

            //HISTORIA 03 TAREA 01,02 y 03
            if (await _context.Payments.AnyAsync(r => r.Reference == result.Reference))
            {
                var clientTemp =  _context.Clients.FirstOrDefault(c => c.Phone == smsData.SenderNumber);

                DuplecateReference dupe = new DuplecateReference
                {
                    Cellphone = smsData.SenderNumber,
                    IdClient = clientTemp?.Id

                };

                _context.DuplecateReferences.Add(dupe);
                await _context.SaveChangesAsync();

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
                OriginalMessage = smsData.MessageBody
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return result;
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