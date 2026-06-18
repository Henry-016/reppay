using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class GrupoRequestDTO
    {
        [Required(ErrorMessage = "O nome da república é obrigatório.")]
        public string Nome { get; set; }

        public string? ImagemBanner { get; set; }
    }
}