using Microsoft.AspNetCore.Mvc;
using RepPay.API.DTOs;
using RepPay.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RepPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        private int? ObterIdUsuarioLogado()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                return null;
            }

            return int.Parse(usuarioIdClaim);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CriarUsuario([FromBody] UsuarioRequestDTO novoUsuarioDTO)
        {
            try
            {
                _usuarioService.CriarUsuario(novoUsuarioDTO);
                return Created("", new { mensagem = "Usuário cadastrado com total segurança!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var response = _usuarioService.Login(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public IActionResult RenovacaoToken([FromBody] RefreshTokenRequestDTO request)
        {
            try
            {
                var response = _usuarioService.RenovacaoToken(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpPost("LogOut")]
        public IActionResult LogOut([FromBody] RefreshTokenRequestDTO request)
        {
            _usuarioService.LogOut(request);
            return Ok(new { mensagem = "Logout efetuado com sucesso. Sessão encerrada no servidor." });
        }

        [HttpGet("MeuPerfil")]
        public IActionResult GetMeuPerfil()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var usuario = _usuarioService.GetMeuPerfil(idLogado.Value);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpPut("Atualizar")]
        public IActionResult AtualizarUsuario([FromBody] UsuarioAtualizarRequestDTO usuarioAtualizado)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                _usuarioService.AtualizarUsuario(idLogado.Value, usuarioAtualizado);
                return Ok(new { mensagem = "Dados do usuário atualizados com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("Deletar")]
        public IActionResult DeletarUsuario()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                _usuarioService.DeletarUsuario(idLogado.Value);
                return Ok(new { mensagem = "Sua conta foi excluída com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("EsqueciSenha")]
        public IActionResult EsqueciSenha([FromBody] EsqueciSenhaRequestDTO request)
        {
            _usuarioService.EsqueciSenha(request);

            return Ok(new { mensagem = "Se o e-mail existir em nossa base, um código será enviado!" });
        }

        [AllowAnonymous]
        [HttpPost("ValidarCodigo")]
        public IActionResult ValidarCodigo([FromBody] ValidarCodigoRequestDTO request)
        {
            try
            {
                _usuarioService.ValidarCodigo(request);
                return Ok(new { mensagem = "Código validado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("ResetarSenha")]
        public IActionResult ResetarSenha([FromBody] ResetarSenhaRequestDTO request)
        {
            try
            {
                _usuarioService.ResetarSenha(request);
                return Ok(new { mensagem = "Sua senha foi redefinida com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("Home/ProximaConta")]
        public IActionResult ObterProximaContaGeral()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var proximaConta = _usuarioService.ObterProximaContaGeral(idLogado.Value);

            return Ok(proximaConta);
        }
    }
}