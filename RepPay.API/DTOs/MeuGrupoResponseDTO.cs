namespace RepPay.API.DTOs
{
	public class MeuGrupoResponseDTO
	{
		public int IdGrupo { get; set; }
		public string Nome { get; set; } = string.Empty;
        public string CodigoAcesso { get; set; } = string.Empty;
        public string? ImagemBanner { get; set; }
		public bool IsAdmin { get; set; }
	}
}