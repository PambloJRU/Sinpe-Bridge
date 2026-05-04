using System;
using System.Collections.Generic;

namespace ProyectoIngenieriaBACKEND_POS.Models.Entities;

public partial class Client
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual ICollection<DuplecateReference> DuplecateReferences { get; set; } = new List<DuplecateReference>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
