using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class SmsReceiverService : ISmsReceiverService
    {
        private static readonly Regex SinpeSmsRegex = new Regex(
            "^SINPE Movil: Ha recibido una transferencia de (?<payer>[A-Z ]+) por (?<amount>[0-9]+(?:[.,][0-9]{1,2})?) colones\\. Ref: (?<reference>[0-9]+)\\.$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public async Task<ParsedSmsResult?> ProcessIncomingSmsAsync(SmsRequestDTO smsData)
        {


            if (string.IsNullOrWhiteSpace(smsData.MessageBody))
            {
                
                return null;
            }

            var match = SinpeSmsRegex.Match(smsData.MessageBody.Trim());
            if (!match.Success)
            {
                return null;
            }

            var amountText = match.Groups["amount"].Value;
            if (!TryParseAmount(amountText, out var amount))
            {
                return null;
            }

            var result = new ParsedSmsResult
            {
                Amount = amount,
                PayerName = match.Groups["payer"].Value.Trim(),
                Reference = match.Groups["reference"].Value.Trim()
            };

            Console.WriteLine($"[NUEVO SMS RECIBIDO] De: {smsData.SenderNumber} a las {smsData.ReceivedAt}");
            Console.WriteLine($"Contenido: {smsData.MessageBody}");

            return await Task.FromResult(result);
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
