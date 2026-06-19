using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class EsqueciSenhaRequestDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        [StringLength(254, ErrorMessage = "O e-mail informado é muito longo.")]
        public string Email { get; set; } = string.Empty;
    }
}