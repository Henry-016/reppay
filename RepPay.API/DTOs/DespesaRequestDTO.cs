namespace RepPay.API.DTOs
{
    public class DespesaRequestDTO
    {
        public string Nome {  get; set; }
        public decimal Valor { get; set; }
        public DateOnly Vencimento { get; set; }
        public string? Icone { get; set; }
        public int IdGrupo { get; set; }
        public List<int> MoradoresIds { get; set; }
    }
}