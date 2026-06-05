namespace RepPay.API.DTOs
{
    public class HistoricoPagoResponseDTO
    {
        public int IdParcela { get; set; }
        public string NomeDespesa { get; set; }
        public string? Icone { get; set; }
        public decimal ValorPago { get; set; }
        public DateOnly? DataPagamento { get; set; }
        public DateOnly Vencimento { get; set; }
    }
}