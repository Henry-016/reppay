namespace RepPay.API.DTOs
{
	public class MeuGrupoResponseDTO
	{
		public int IdGrupo { get; set; }
		public string Nome { get; set; }
		public string CodigoAcesso { get; set; }
		public string? ImagemBanner { get; set; }
		public bool IsAdmin { get; set; }
	}
}