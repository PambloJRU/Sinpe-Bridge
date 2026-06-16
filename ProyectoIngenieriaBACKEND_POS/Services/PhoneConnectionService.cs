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
    public class PhoneConnectionService : IPhoneConnectionService
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PhoneConnectionService(
            AppDbContext context,
            IAuditLogService auditLogService,
            IConfiguration configuration,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _auditLogService = auditLogService;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        public async Task<PhoneConnectionStatusDTO> RegisterHeartbeatAsync(PhoneHeartbeatRequestDTO request)
        {
            var nowUtc = DateTime.UtcNow;
            var connection = await GetOrCreateAsync(nowUtc);
            var wasConnected = connection.IsConnected;

            connection.DeviceId = request.DeviceId.Trim();
            connection.LastHeartbeatUtc = nowUtc;
            connection.UpdatedAtUtc = nowUtc;
            connection.IsConnected = true;

            _context.PhoneConnections.Update(connection);
            await _context.SaveChangesAsync();

            var status = MapStatus(connection, nowUtc);

            await _hubContext.Clients.All.SendAsync("PhoneStatus", new
            {
                isConnected = status.IsConnected,
                lastHeartbeatUtc = status.LastHeartbeatUtc,
                minutesSinceLastHeartbeat = status.MinutesSinceLastHeartbeat
            });

            if (!wasConnected)
            {
                await _auditLogService.LogEventAsync(
                    EventType.PhoneReconnected,
                    RiskLevel.Low,
                    "Telefono receptor reconectado.");
            }

            return status;
        }

        public async Task<PhoneConnectionStatusDTO> GetStatusAsync()
        {
            var connection = await _context.PhoneConnections
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return MapStatus(connection, DateTime.UtcNow);
        }

        public async Task CheckForTimeoutAsync(CancellationToken cancellationToken)
        {
            var connection = await _context.PhoneConnections
                .FirstOrDefaultAsync(cancellationToken);

            if (connection == null)
                return;

            var nowUtc = DateTime.UtcNow;
            var timeoutMinutes = _configuration.GetValue<int?>("PhoneConnection:TimeoutMinutes") ?? 15;

            if (timeoutMinutes <= 0)
                timeoutMinutes = 15;

            var timeout = TimeSpan.FromMinutes(timeoutMinutes);

            if (connection.IsConnected && nowUtc - connection.LastHeartbeatUtc > timeout)
            {
                connection.IsConnected = false;
                connection.UpdatedAtUtc = nowUtc;

                _context.PhoneConnections.Update(connection);
                await _context.SaveChangesAsync(cancellationToken);

                await _hubContext.Clients.All.SendAsync("PhoneStatus", new
                {
                    isConnected = false,
                    lastHeartbeatUtc = connection.LastHeartbeatUtc,
                    minutesSinceLastHeartbeat = Math.Round((nowUtc - connection.LastHeartbeatUtc).TotalMinutes, 2)
                });

                await _auditLogService.LogEventAsync(
                    EventType.PhoneDisconnected,
                    RiskLevel.Critical,
                    "Telefono receptor sin comunicacion.");
            }
        }

        private async Task<PhoneConnection> GetOrCreateAsync(DateTime nowUtc)
        {
            var connection = await _context.PhoneConnections.FirstOrDefaultAsync();

            if (connection != null)
                return connection;

            connection = new PhoneConnection
            {
                DeviceId = null,
                LastHeartbeatUtc = nowUtc,
                UpdatedAtUtc = nowUtc,
                IsConnected = true
            };

            _context.PhoneConnections.Add(connection);
            await _context.SaveChangesAsync();

            return connection;
        }

        private static PhoneConnectionStatusDTO MapStatus(PhoneConnection? connection, DateTime nowUtc)
        {
            if (connection == null)
            {
                return new PhoneConnectionStatusDTO
                {
                    IsConnected = false,
                    LastHeartbeatUtc = null,
                    MinutesSinceLastHeartbeat = null
                };
            }

            var minutes = (nowUtc - connection.LastHeartbeatUtc).TotalMinutes;

            return new PhoneConnectionStatusDTO
            {
                IsConnected = connection.IsConnected,
                LastHeartbeatUtc = connection.LastHeartbeatUtc,
                MinutesSinceLastHeartbeat = Math.Round(minutes, 2)
            };
        }
    }
}
