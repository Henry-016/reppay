namespace RepPay.API.DTOs
{
    public class DespesaGerenciamentoResponseDTO
    {
        public int IdDespesa { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public DateOnly Vencimento { get; set; }
        public string? Icone { get; set; }
    }
}