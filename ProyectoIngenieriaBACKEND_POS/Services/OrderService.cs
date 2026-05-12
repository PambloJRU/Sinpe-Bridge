using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(OrderCreateDTO orderDto)
        {
            var newOrder = new Order
            {
                Phone = orderDto.Phone,
                Amount = orderDto.Amount,
                State = "PENDIENTE" 
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return newOrder;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        //metodo para asociar una orden con un pago
        public async Task<bool> AssociateOrderWithPayment(int orderId)
        {
            int maxRetries = 300; //300 intentos
            int delayMs = 1000; //un segundo
            int attempts = 0;

            //basicamente se busca un pago con los mismos datos que la orden durante 5 minutos
            while (attempts < maxRetries)
            {
                try
                {
                    var order = await _context.Orders.FindAsync(orderId);
                    if (order == null)
                        throw new ArgumentException($"No existe una orden con id: {orderId}");

                    if (order.PaymentId > 0)
                        return true;

                    var payment = await _context.Payments
                        .Where(p =>
                            p.Amount == order.Amount &&
                            p.Status == PaymentStatus.Pending /*&&
                            p.ReceivedAt.Date == DateTime.Now.Date*/)
                        .FirstOrDefaultAsync();

                    if (payment != null)
                    {
                        //validaciones
                        var alreadyAssociated = await _context.Orders
                            .Where(o => o.PaymentId == payment.Id)
                            .FirstOrDefaultAsync();

                        if (alreadyAssociated != null)
                            throw new InvalidOperationException($"El pago ya está asociado a otra orden");

                        if (payment.Amount != order.Amount)
                            throw new InvalidOperationException($"El monto del pago ({payment.Amount}) no coincide con la orden ({order.Amount})");

                        if (payment.ClientId <= 0)
                            throw new InvalidOperationException($"El pago no tiene un cliente asociado válido");

                        if (order.Amount <= 0)
                            throw new InvalidOperationException($"La orden tiene un monto inválido");

                        if (string.IsNullOrEmpty(order.Phone))
                            throw new InvalidOperationException($"La orden no tiene teléfono registrado");

                        //cambios el estado de la orden y del pago
                        order.PaymentId = payment.Id;
                        order.State = "PAGADA";
                        payment.Status = PaymentStatus.Valid;
                        _context.Orders.Update(order);
                        await _context.SaveChangesAsync();
                        return true;
                    }

                    attempts++;
                    if (attempts < maxRetries)
                        await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error al asociar orden con pago: {ex.Message}", ex);
                }
            }

            throw new TimeoutException($"Tiempo agotado esperando pago para la orden {orderId}");
        }
    }
}
