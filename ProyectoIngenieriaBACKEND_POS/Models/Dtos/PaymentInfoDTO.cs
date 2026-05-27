namespace ProyectoIngenieriaBACKEND_POS.Models.Dtos
{
    public class PaymentInfoDTO
    {

        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string OriginalMessage { get; set; }
        public string ClientName { get; set; } = null!;

    }
}
