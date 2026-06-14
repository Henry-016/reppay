namespace RepPay.API.DTOs
{
    public class ProximaContaResponseDTO
    {
        public string NomeDespesa { get; set; } = string.Empty;
        public string? NomeGrupo { get; set; }
        public DateOnly Vencimento { get; set; }
        public decimal Valor { get; set; }
    }
}