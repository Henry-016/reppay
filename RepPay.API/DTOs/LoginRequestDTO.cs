using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        [StringLength(254, ErrorMessage = "O e-mail informado é muito longo.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }
}