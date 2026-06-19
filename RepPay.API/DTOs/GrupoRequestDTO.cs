using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class GrupoRequestDTO
    {
        [Required(ErrorMessage = "O nome da república é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da república deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public string? ImagemBanner { get; set; }
    }
}