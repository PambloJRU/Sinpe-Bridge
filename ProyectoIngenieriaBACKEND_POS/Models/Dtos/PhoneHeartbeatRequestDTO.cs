namespace ProyectoIngenieriaBACKEND_POS.Models.Dtos
{
    public class PhoneHeartbeatRequestDTO
    {
        public string DeviceId { get; set; } = string.Empty;

        public DateTime SentAtUtc { get; set; }
    }
}
