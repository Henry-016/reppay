namespace RepPay.API.DTOs
{
	public class UsuarioResponseDTO
	{
        public int IdUsuario { get; set; }
        public string FotoDePerfil { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
	
}