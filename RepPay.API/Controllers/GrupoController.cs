using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepPay.API.DTOs;
using RepPay.API.Models;
using System.Security.Claims;

namespace RepPay.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class GrupoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GrupoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CriarGrupo([FromBody] GrupoRequestDTO request)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado!" });
            }

            int IdAdmin = int.Parse(usuarioIdClaim);

            string codigoAcesso = GerarCodigoAcesso();

            var novoGrupo = new Grupo
            {
                Nome = request.Nome,
                ImagemBanner = request.ImagemBanner,
                CodigoAcesso = codigoAcesso,
                IdAdmin = IdAdmin
            };

            _context.Grupos.Add(novoGrupo);
            _context.SaveChanges();

            return Created("", new
            {
                mensagem = "República criada com sucesso!",
                codigoAcesso = codigoAcesso
            });
        }

        private string GerarCodigoAcesso()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string codigo;
            bool codigoExiste;

            do
            {
                codigo = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                codigoExiste = _context.Grupos.Any(g => g.CodigoAcesso == codigo);
            }
            while (codigoExiste);

            return codigo;
        }

        [HttpPost("Entrar")]
        public IActionResult EntrarNoGrupo([FromBody] EntrarGrupoRequestDTO request)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado!" });
            }

            int idUsuario = int.Parse(usuarioIdClaim);

            var grupo = _context.Grupos.FirstOrDefault(g => g.CodigoAcesso.ToLower() == request.CodigoAcesso.ToLower());

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Código de acesso inválido ou república não encontrada." });
            }

            bool jaPertence = _context.Pertences.Any(p => p.IdGrupo == grupo.IdGrupo && p.IdUsuario == idUsuario);

            if (jaPertence)
            {
                return BadRequest(new { mensagem = "Você já faz parte desta república!" });
            }

            var novoVinculo = new Pertence
            {
                IdGrupo = grupo.IdGrupo,
                IdUsuario = idUsuario
            };

            _context.Pertences.Add(novoVinculo);
            _context.SaveChanges();

            return Ok(new { mensagem = $"Bem-vindo(a) à {grupo.Nome}!" });
        }
    }
}