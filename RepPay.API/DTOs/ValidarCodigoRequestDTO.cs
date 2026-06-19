using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class ValidarCodigoRequestDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código é obrigatório.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "O código deve ter exatamente 6 dígitos.")]
        public string Codigo { get; set; } = string.Empty;
    }
}