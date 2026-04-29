using System.ComponentModel.DataAnnotations;

namespace ProyectoIngenieriaBACKEND_POS.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public Client Client { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Reference { get; set; } = string.Empty;

        public DateTime ReceivedAt { get; set; }

        public string OriginalMessage { get; set; } = string.Empty;
    }
}