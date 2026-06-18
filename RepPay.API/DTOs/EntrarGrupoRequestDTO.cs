using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class EntrarGrupoRequestDTO
    {
        [Required(ErrorMessage = "O código de acesso é obrigatório.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "O código de acesso deve ter exatamente 8 caracteres.")]
        public string CodigoAcesso { get; set; }
    }
}