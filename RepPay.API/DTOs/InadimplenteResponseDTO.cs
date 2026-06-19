namespace RepPay.API.DTOs
{
    public class InadimplenteResponseDTO
    {
        public int IdParcela { get; set; }
        public string NomeMorador { get; set; } = string.Empty;
        public string NomeDespesa { get; set; } = string.Empty;
        public string? Icone { get; set; }
        public decimal Valor { get; set; }
        public DateOnly Vencimento { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}