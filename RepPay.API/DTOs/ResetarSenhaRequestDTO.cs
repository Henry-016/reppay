namespace RepPay.API.DTOs
{
	public class ResetarSenhaRequestDTO
	{
		public string Email { get; set; }
		public string Codigo { get; set; }
		public string NovaSenha { get; set; }
	}
}