namespace ProyectoIngenieriaBACKEND_POS.Models.Dtos
{
    public class OrderStatusDTO
    {
        public int OrderId { get; set; }
        public string State { get; set; }
        public int? PaymentId { get; set; }
    }
}
