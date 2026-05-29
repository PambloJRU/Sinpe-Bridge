namespace ProyectoIngenieriaBACKEND_POS.Models.Dtos
{
    public class PendingReviewPaymentDTO
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string ClientPhone { get; set; }
        public string ClientName { get; set; }
        public int? OrderId { get; set; }
        public decimal? OrderAmount { get; set; }
        public decimal? Difference { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
