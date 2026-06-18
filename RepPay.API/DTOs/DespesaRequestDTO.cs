using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class DespesaRequestDTO
    {
        [Required(ErrorMessage = "O nome da despesa é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da despesa deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data de vencimento é obrigatória.")] 
        public DateOnly Vencimento { get; set; }

        public string? Icone { get; set; }

        [Required(ErrorMessage = "O ID do grupo é obrigatório.")] 
        public int IdGrupo { get; set; }

        [Required(ErrorMessage = "A lista de moradores é obrigatória.")]
        [MinLength(1, ErrorMessage = "É necessário selecionar pelo menos um morador para o rateio.")]
        public List<int> MoradoresIds { get; set; }
    }
}