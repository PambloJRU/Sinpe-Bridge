using ProyectoIngenieriaBACKEND_POS.Models.Dtos;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    public interface IPhoneConnectionService
    {
        Task<PhoneConnectionStatusDTO> RegisterHeartbeatAsync(PhoneHeartbeatRequestDTO request);
        Task<PhoneConnectionStatusDTO> GetStatusAsync();
        Task CheckForTimeoutAsync(CancellationToken cancellationToken);
    }
}
