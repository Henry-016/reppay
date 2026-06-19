using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class ResetarSenhaRequestDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código de recuperação é obrigatório.")]
        [StringLength(8, MinimumLength = 6, ErrorMessage = "O código deve ter exatamente 6 dígitos.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(8, ErrorMessage = "A nova senha deve ter no mínimo 8 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;
    }
}