using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "O Refresh Token é obrigatório.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}