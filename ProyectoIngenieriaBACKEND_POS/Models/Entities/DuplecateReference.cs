using System;
using System.Collections.Generic;

namespace ProyectoIngenieriaBACKEND_POS.Models.Entities;

public partial class DuplecateReference
{
    public int Id { get; set; }

    public string? Cellphone { get; set; }

    public int? IdClient { get; set; }

    public virtual Client? IdClientNavigation { get; set; }
} 
