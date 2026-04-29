namespace ProyectoIngenieriaBACKEND_POS.Models.Entities
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}