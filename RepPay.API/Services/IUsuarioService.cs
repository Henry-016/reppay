using RepPay.API.DTOs;

namespace RepPay.API.Services
{
    public interface IUsuarioService
    {
        void CriarUsuario(UsuarioRequestDTO novoUsuarioDTO);

        LoginResponseDTO Login(LoginRequestDTO request);
        void LogOut(RefreshTokenRequestDTO request);
        TokenResponseDTO RenovacaoToken(RefreshTokenRequestDTO request);

        UsuarioResponseDTO GetMeuPerfil(int idLogado);
        void AtualizarUsuario(int idLogado, UsuarioRequestDTO usuarioAtualizado);
        void DeletarUsuario(int idLogado);

        void EsqueciSenha(EsqueciSenhaRequestDTO request);
        void ValidarCodigo(ValidarCodigoRequestDTO request);
        void ResetarSenha(ResetarSenhaRequestDTO request);

        ProximaContaResponseDTO? ObterProximaContaGeral(int idLogado);
    }
}
