namespace RepPay.API.DTOs
{
    public class ResumoDividasResponseDTO
    {
        public decimal TotalDevido { get; set; }
        public List<MinhaDividaResponseDTO> ListaDividas { get; set; } = new();
    }

    public class ResumoInadimplentesDTO
    {
        public decimal TotalAReceber { get; set; }
        public List<InadimplenteResponseDTO> ListaInadimplentes { get; set; } = new();
    }
}