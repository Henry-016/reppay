namespace RepPay.API.DTOs
{
    public class EditarDespesaRequestDTO
    {
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateOnly Vencimento { get; set; }
        public string? Icone { get; set; }
    }
}