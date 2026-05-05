using System;
using System.Collections.Generic;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;

namespace ProyectoIngenieriaBACKEND_POS.Models.Entities;

public partial class Payment
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string Reference { get; set; } = null!;

    public DateTime ReceivedAt { get; set; }

    public string OriginalMessage { get; set; } = null!;

    public int ClientId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public virtual Client Client { get; set; } = null!;
}
