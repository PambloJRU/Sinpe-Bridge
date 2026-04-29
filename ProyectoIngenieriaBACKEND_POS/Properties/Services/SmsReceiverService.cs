using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using Microsoft.EntityFrameworkCore;

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
    }
}