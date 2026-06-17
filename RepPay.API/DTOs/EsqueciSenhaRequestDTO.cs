using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class EsqueciSenhaRequestDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        public string Email { get; set; }
    }
}