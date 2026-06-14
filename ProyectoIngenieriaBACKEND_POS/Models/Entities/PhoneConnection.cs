namespace ProyectoIngenieriaBACKEND_POS.Models.Entities;

public partial class PhoneConnection
{
    public int Id { get; set; }

    public string? DeviceId { get; set; }

    public DateTime LastHeartbeatUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public bool IsConnected { get; set; }
}
