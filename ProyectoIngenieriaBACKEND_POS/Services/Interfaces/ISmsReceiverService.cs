using ProyectoIngenieriaBACKEND_POS.Models.Dtos;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    public interface ISmsReceiverService
    {
        Task<ParsedSmsResult?> ProcessIncomingSmsAsync(SmsRequestDTO smsData);
    }
}
