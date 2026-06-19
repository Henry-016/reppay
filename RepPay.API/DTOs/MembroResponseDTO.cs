namespace RepPay.API.DTOs
{
	public class MembroResponseDTO
	{
		public int IdUsuario { get; set; }
		public string Nome { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FotoPerfil { get; set; }
        public decimal TotalDevido { get; set; }
    }
}