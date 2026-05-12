using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    public interface ISmsReceiverService
    {
        Task<ParsedSmsResult?> ProcessIncomingSmsAsync(SmsRequestDTO smsData);
        Task<List<Payment>> GetAllAsync();
    }
}
