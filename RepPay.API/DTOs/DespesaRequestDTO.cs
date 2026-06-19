using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class DespesaRequestDTO
    {
        [Required(ErrorMessage = "O nome da despesa é obrigatório.")]
        [StringLength(255, ErrorMessage = "O nome da despesa deve ter no máximo 255 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da despesa deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Range(typeof(DateOnly), "1900-01-01", "2999-12-31", ErrorMessage = "A data de vencimento informada é inválida.")] 
        public DateOnly Vencimento { get; set; }

        public string? Icone { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O ID do grupo é obrigatório e deve ser válido.")] 
        public int IdGrupo { get; set; }

        [Required(ErrorMessage = "A lista de moradores é obrigatória.")]
        [MinLength(1, ErrorMessage = "É necessário selecionar pelo menos um morador para o rateio.")]
        public List<int> MoradoresIds { get; set; } = new();
    }
}