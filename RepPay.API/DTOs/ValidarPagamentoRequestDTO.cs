using System.ComponentModel.DataAnnotations;

namespace RepPay.API.DTOs
{
    public class ValidarPagamentoRequestDTO
    {
        [Required(ErrorMessage = "É necessário informar se o pagamento foi aprovado ou rejeitado.")]
        public bool? Aprovado { get; set; }
    }
}