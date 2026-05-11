using System.ComponentModel.DataAnnotations;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    public class OrderCreateDTO
    {

        public string Phone { get; set; } = null!;

        public decimal Amount { get; set; }
    }
}
