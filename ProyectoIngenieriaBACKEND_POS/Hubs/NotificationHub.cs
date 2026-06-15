using Microsoft.AspNetCore.SignalR;

namespace ProyectoIngenieriaBACKEND_POS.Hubs;

public class NotificationHub : Hub
{
    public async Task SendOrderStatus(int orderId, string state, int? paymentId)
    {
        await Clients.All.SendAsync("OrderStatus", new
        {
            orderId,
            state,
            paymentId
        });
    }

    public async Task SendPhoneStatus(bool isConnected, DateTime? lastHeartbeatUtc, double? minutesSinceLastHeartbeat)
    {
        await Clients.All.SendAsync("PhoneStatus", new
        {
            isConnected,
            lastHeartbeatUtc,
            minutesSinceLastHeartbeat
        });
    }

    public async Task SendPaymentReviewed(int paymentId, bool approved)
    {
        await Clients.All.SendAsync("PaymentReviewed", new
        {
            paymentId,
            approved
        });
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", new { message = "Conectado a notificaciones en tiempo real" });
        await base.OnConnectedAsync();
    }
}
