namespace RepPay.API.DTOs
{
    public class MinhaDividaResponseDTO
    {
        public int IdParcela { get; set; }
        public string NomeDespesa { get; set; }
        public string? Icone { get; set; }
        public decimal Valor { get; set; }
        public DateOnly Vencimento { get; set; }
        public string Status { get; set; }
    }
}