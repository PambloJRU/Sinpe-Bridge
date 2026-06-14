using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Services
{
    public class PhoneConnectionMonitorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PhoneConnectionMonitorService> _logger;
        private readonly TimeSpan _interval;

        public PhoneConnectionMonitorService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<PhoneConnectionMonitorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var intervalSeconds = configuration.GetValue<int?>("PhoneConnection:CheckIntervalSeconds") ?? 30;
            if (intervalSeconds <= 0)
                intervalSeconds = 30;

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
                    var service = scope.ServiceProvider.GetRequiredService<IPhoneConnectionService>();
                    await service.CheckForTimeoutAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking phone connection.");
                }
            }
        }
    }
}
