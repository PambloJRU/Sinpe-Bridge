namespace ProyectoIngenieriaBACKEND_POS.Models.Dtos
{
    public class ParsedSmsResult
    {
        public decimal Amount { get; set; }

        public string PayerName { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;

        public DateTime PaymentDateTime { get; set; }
    }
}
