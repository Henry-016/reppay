namespace RepPay.API.DTOs
{
	public class LoginResponseDTO
	{
		public string Mensagem { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
		public int IdUsuario { get; set; }
		public string Nome { get; set; } = string.Empty;
	}
}