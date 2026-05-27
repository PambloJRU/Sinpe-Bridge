using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<PaymentInfoDTO>> whyFUNCA()
        {
         var paymentsInfo =  _context.Payments
        .Select(p => new PaymentInfoDTO
        {
            Amount = p.Amount,
            Reference = p.Reference,
            ReceivedAt = p.ReceivedAt,
            OriginalMessage = p.OriginalMessage,
            // Aquí ocurre la magia del INNER JOIN automático de Entity Framework
            ClientName = p.Client.Name
        })
        .ToListAsync();

            return paymentsInfo;
        }

        public async Task<List<PaymentInfoDTO>> GetPaymentsWithClientInfoAsync()
        {
            return await _context.Payments
        .Select(p => new PaymentInfoDTO
        {
            Amount = p.Amount,
            Reference = p.Reference,
            ReceivedAt = p.ReceivedAt,
            OriginalMessage = p.OriginalMessage,
            // Aquí ocurre la magia del INNER JOIN automático de Entity Framework
            ClientName = p.Client.Name
        })
        .ToListAsync(); ;
        }
    }
}
