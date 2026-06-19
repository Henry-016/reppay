using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class UsuarioAtualizarRequestDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "O nome deve ter no mínimo 3 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public string FotoDePerfil { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        public string Email { get; set; } = string.Empty;

        [MinLength(8, ErrorMessage = "Se quiser trocar a senha, ela deve ter no mínimo 8 caracteres.")]
        public string? Senha { get; set; }
    }
}