namespace RepPay.API.DTOs
{
    public class InadimplenteResponseDTO
    {
        public int IdParcela { get; set; }
        public string NomeMorador { get; set; }
        public string NomeDespesa { get; set; }
        public string? Icone { get; set; }
        public decimal Valor { get; set; }
        public DateOnly Vencimento { get; set; }
        public string Status { get; set; }
    }
}