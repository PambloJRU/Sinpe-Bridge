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
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IAuditLogService _auditLogService;

        public OrderService(
            AppDbContext context,
            IHubContext<NotificationHub> hubContext,
            IAuditLogService auditLogService)
        {
            _context = context;
            _hubContext = hubContext;
            _auditLogService = auditLogService;
        }

        public async Task<Order> CreateOrderAsync(OrderCreateDTO dto)
        {
            var order = new Order
            {
                Amount = dto.Amount,
                Phone = dto.Phone,
                State = "PENDIENTE",
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await TryMatchExistingPaymentsAsync(order);

            return order;
        }

        private async Task TryMatchExistingPaymentsAsync(Order order)
        {
            var timeoutMinutes = 10;
            var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

            var exactMatch = await _context.Payments
                .Include(p => p.Client)
                .Where(p =>
                    p.Status == PaymentStatus.Pending &&
                    p.Amount == order.Amount &&
                    p.Client.Phone == order.Phone &&
                    p.ReceivedAt >= cutoffTime)
                .FirstOrDefaultAsync();

            if (exactMatch != null)
            {
                await AssociatePaymentToOrderAsync(order, exactMatch, "PAGADA", PaymentStatus.Valid);
                return;
            }

            var phoneMatch = await _context.Payments
                .Include(p => p.Client)
                .Where(p =>
                    p.Status == PaymentStatus.Pending &&
                    p.Client.Phone == order.Phone &&
                    p.ReceivedAt >= cutoffTime)
                .FirstOrDefaultAsync();

            if (phoneMatch != null)
            {
                await AssociatePaymentToOrderAsync(order, phoneMatch, "PAGO_PARCIAL", PaymentStatus.PendingReview);
                return;
            }

            var expiredPayments = await _context.Payments
                .Include(p => p.Client)
                .Where(p =>
                    p.Status == PaymentStatus.Pending &&
                    p.Client.Phone == order.Phone &&
                    p.ReceivedAt < cutoffTime)
                .ToListAsync();

            foreach (var expiredPayment in expiredPayments)
            {
                expiredPayment.Status = PaymentStatus.PendingReview;
                _context.Payments.Update(expiredPayment);

                await _auditLogService.LogEventAsync(
                    EventType.ManualReviewRequired,
                    RiskLevel.Medium,
                    $"Pago expirado movido a revisión manual. Referencia: {expiredPayment.Reference}",
                    paymentId: expiredPayment.Id);
            }

            if (expiredPayments.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task AssociatePaymentToOrderAsync(Order order, Payment payment, string orderState, PaymentStatus paymentStatus)
        {
            Console.WriteLine($"[ORDER] Auto-match: Pago #{payment.Id} (CRC {payment.Amount}) asociado a Orden #{order.Id} -> Estado: {orderState}");

            order.PaymentId = payment.Id;
            order.State = orderState;
            payment.Status = paymentStatus;

            _context.Orders.Update(order);
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("OrderStatus", new
            {
                orderId = order.Id,
                state = order.State,
                paymentId = order.PaymentId,
                autoMatch = true
            });

            await _auditLogService.LogEventAsync(
                EventType.PaymentConfirmed,
                RiskLevel.Low,
                $"Pago asociado automáticamente a orden #{order.Id}. Referencia: {payment.Reference}",
                paymentId: payment.Id,
                orderId: order.Id);
        }

        public async Task<OrderStatusDTO> GetOrderStatusAsync(int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return null;

            return new OrderStatusDTO
            {
                OrderId = order.Id,
                State = order.State,
                PaymentId = order.PaymentId
            };
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }
    }
}
