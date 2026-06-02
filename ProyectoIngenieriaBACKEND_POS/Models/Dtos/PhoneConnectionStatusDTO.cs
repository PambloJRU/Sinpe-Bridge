namespace ProyectoIngenieriaBACKEND_POS.Models.Dtos
{
    public class PhoneConnectionStatusDTO
    {
        public bool IsConnected { get; set; }

        public DateTime? LastHeartbeatUtc { get; set; }

        public double? MinutesSinceLastHeartbeat { get; set; }
    }
}
