using System;
using System.Collections.Generic;

namespace ProyectoIngenieriaBACKEND_POS.Models.Entities;

public partial class Order
{
    public int Id { get; set; }

    public string Phone { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? State { get; set; }

    public int? PaymentId { get; set; }

    public virtual Payment? Payment { get; set; }
}
