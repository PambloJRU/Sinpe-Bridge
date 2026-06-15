using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;
using ProyectoIngenieriaBACKEND_POS.Hubs;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PaymentService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

        public async Task<List<PendingReviewPaymentDTO>> GetPendingReviewPaymentsAsync()
        {
            var payments = await _context.Payments
                .Include(p => p.Client)
                .Include(p => p.Orders)
                .AsNoTracking()
                .Where(p => p.Status == PaymentStatus.PendingReview)
                .Select(p => new PendingReviewPaymentDTO
                {
                    PaymentId = p.Id, 
                    Amount = p.Amount,
                    Reference = p.Reference,
                    ClientPhone = p.Client.Phone,
                    ClientName = p.Client.Name,
                    OrderId = p.Orders.FirstOrDefault().Id,
                    OrderAmount = p.Orders.FirstOrDefault() != null ? p.Orders.FirstOrDefault().Amount : null,
                    Difference = p.Orders.FirstOrDefault() != null ? p.Orders.FirstOrDefault().Amount - p.Amount : null,
                    ReceivedAt = p.ReceivedAt
                })
                .ToListAsync();

            return payments;
        }

        public async Task ReviewPaymentAsync(int paymentId, bool approved)
        {
            var payment = await _context.Payments
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new ArgumentException("Pago no encontrado");

            if (payment.Status != PaymentStatus.PendingReview)
                throw new InvalidOperationException("El pago no está en revisión");

            if (approved)
            {
                payment.Status = PaymentStatus.Valid;
                foreach (var order in payment.Orders)
                {
                    order.State = "PAGADA_REVISADA";
                    _context.Orders.Update(order);
                }
            }
            else
            {
                payment.Status = PaymentStatus.Rejected;

                foreach (var order in payment.Orders)
                {
                    order.PaymentId = null;
                    order.State = "PENDIENTE";
                    _context.Orders.Update(order);
                }
            }

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("PaymentReviewed", new
            {
                paymentId = payment.Id,
                approved = approved
            });

            if (approved)
            {
                foreach (var order in payment.Orders)
                {
                    await _hubContext.Clients.All.SendAsync("OrderStatus", new
                    {
                        orderId = order.Id,
                        state = order.State,
                        paymentId = order.PaymentId
                    });
                }
            }
        }
    }
}
