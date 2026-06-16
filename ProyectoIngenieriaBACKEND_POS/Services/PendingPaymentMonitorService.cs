using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Data;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class PendingPaymentMonitorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PendingPaymentMonitorService> _logger;
        private readonly TimeSpan _interval;

        public PendingPaymentMonitorService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<PendingPaymentMonitorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var intervalSeconds = configuration.GetValue<int?>("PendingPayment:CheckIntervalSeconds") ?? 60;
            if (intervalSeconds <= 0)
                intervalSeconds = 60;

            _interval = TimeSpan.FromSeconds(intervalSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

                    var timeoutMinutes = 10;
                    var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

                    var expiredPayments = await context.Payments
                        .Where(p => p.Status == PaymentStatus.Pending && p.ReceivedAt <= cutoffTime)
                        .ToListAsync(stoppingToken);

                    if (expiredPayments.Count > 0)
                    {
                        foreach (var payment in expiredPayments)
                        {
                            payment.Status = PaymentStatus.PendingReview;
                            context.Payments.Update(payment);

                            await auditLogService.LogEventAsync(
                                EventType.ManualReviewRequired,
                                RiskLevel.Medium,
                                $"Pago expirado sin orden asociada (>{timeoutMinutes} min). Referencia: {payment.Reference}",
                                paymentId: payment.Id);
                        }

                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Moved {Count} expired pending payments to PendingReview", expiredPayments.Count);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking pending payments.");
                }
            }
        }
    }
}
