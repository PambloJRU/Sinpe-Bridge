using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    public interface IPaymentService
    {
       public Task<List<PaymentInfoDTO>> GetPaymentsWithClientInfoAsync();
    }
}
