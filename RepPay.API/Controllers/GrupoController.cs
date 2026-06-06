using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepPay.API.DTOs;
using RepPay.API.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

        private int? ObterIdUsuarioLogado()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                return null;
            }
            return int.Parse(usuarioIdClaim);
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

        [HttpPost]
        public IActionResult CriarGrupo([FromBody] GrupoRequestDTO request)
        {
            int? idAdmin = ObterIdUsuarioLogado();

            if (idAdmin == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado!" });
            }

            string codigoAcesso = GerarCodigoAcesso();

            var novoGrupo = new Grupo
            {
                Nome = request.Nome,
                ImagemBanner = request.ImagemBanner,
                CodigoAcesso = codigoAcesso,
                IdAdmin = idAdmin.Value
            };

            _context.Grupos.Add(novoGrupo);
            _context.SaveChanges();

            return Created("", new
            {
                mensagem = "República criada com sucesso!",
                codigoAcesso = codigoAcesso
            });
        }

        [HttpPost("Entrar")]
        public IActionResult EntrarNoGrupo([FromBody] EntrarGrupoRequestDTO request)
        {
            int? idUsuario = ObterIdUsuarioLogado();

            if (idUsuario == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado!" });
            }

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
                IdUsuario = idUsuario.Value
            };

            _context.Pertences.Add(novoVinculo);
            _context.SaveChanges();

            return Ok(new { mensagem = $"Bem-vindo(a) à {grupo.Nome}!" });
        }

        [HttpGet("Meus")]
        public IActionResult GetMeusGrupos()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var meusGrupos = _context.Pertences
                .Include(p => p.IdGrupoNavigation)
                .Where(p => p.IdUsuario == idLogado)
                .Select(p => new MeuGrupoResponseDTO
                {
                    IdGrupo = p.IdGrupoNavigation.IdGrupo,
                    Nome = p.IdGrupoNavigation.Nome,
                    CodigoAcesso = p.IdGrupoNavigation.CodigoAcesso,
                    ImagemBanner = p.IdGrupoNavigation.ImagemBanner,
                    IsAdmin = p.IdGrupoNavigation.IdAdmin == idLogado
                }).ToList();

            return Ok(meusGrupos);
        }

        [HttpGet("{idGrupo}")]
        public IActionResult GetGrupoPorId(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var relacaoPertence = _context.Pertences
                .Include(p => p.IdGrupoNavigation)
                .FirstOrDefault(p => p.IdUsuario == idLogado && p.IdGrupo == idGrupo);

            if (relacaoPertence == null)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Você não pertence a este grupo ou ele não existe." });
            }

            var grupo = relacaoPertence.IdGrupoNavigation;

            var response = new MeuGrupoResponseDTO
            {
                IdGrupo = grupo.IdGrupo,
                Nome = grupo.Nome,
                CodigoAcesso = grupo.CodigoAcesso,
                ImagemBanner = grupo.ImagemBanner,
                IsAdmin = grupo.IdAdmin == idLogado
            };

            return Ok(response);
        }

        [HttpGet("{idGrupo}/Membros")]
        public IActionResult GetMembrosDoGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo não encontrado." });
            }

            bool usuarioPertence = _context.Pertences.Any(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado);

            if (!usuarioPertence)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Você não pertence a este grupo." });
            }

            var membros = _context.Pertences
                .Include(p => p.IdUsuarioNavigation)
                .Where(p => p.IdGrupo == idGrupo)
                .Select(p => new MembroResponseDTO
                {
                    IdUsuario = p.IdUsuario,
                    Nome = p.IdUsuarioNavigation.Nome,
                    IsAdmin = p.IdUsuario == grupo.IdAdmin
                })
                .OrderByDescending(m => m.IsAdmin)
                .ThenBy(m => m.Nome)
                .ToList();

            return Ok(membros);
        }

        [HttpDelete("{idGrupo}/Sair")]
        public IActionResult SairDoGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var vinculo = _context.Pertences.FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado);

            if (vinculo == null)
            {
                return NotFound(new { mensagem = "Você não pertence a esta república." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo != null && grupo.IdAdmin == idLogado)
            {
                return BadRequest(new { mensagem = "Você é o administrador do grupo. Transfira a liderança para outro morador antes de sair." });
            }

            bool temDividas = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Any(p => p.IdUsuario == idLogado
                       && p.IdDespesaNavigation.IdGrupo == idGrupo
                       && (p.Status == StatusParcela.PENDENTE ||
                           p.Status == StatusParcela.ATRASADO ||
                           p.Status == StatusParcela.EM_ANALISE));

            if (temDividas)
            {
                return BadRequest(new
                {
                    mensagem = "Você possui dívidas pendentes ou em análise nesta república. Quite todas as contas antes de sair!"
                });
            }

            _context.Pertences.Remove(vinculo);
            _context.SaveChanges();

            return Ok(new { mensagem = "Você saiu da república com sucesso. Sentiremos sua falta!" });
        }

        [HttpDelete("{idGrupo}/Expulsar/{idMorador}")]
        public IActionResult ExpulsarMorador(int idGrupo, int idMorador)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo não encontrado." });
            }

            if (grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador pode expulsar moradores." });
            }

            if (idLogado == idMorador)
            {
                return BadRequest(new { mensagem = "Você não pode expulsar a si mesmo. Caso queira sair, utilize a opção de saída voluntária ou exclua o grupo." });
            }

            var vinculo = _context.Pertences.FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idMorador);

            if (vinculo == null)
            {
                return NotFound(new { mensagem = "Este usuário não é um morador da sua república." });
            }

            bool moradorTemDividas = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Any(p => p.IdUsuario == idMorador
                       && p.IdDespesaNavigation.IdGrupo == idGrupo
                       && (p.Status == StatusParcela.PENDENTE ||
                           p.Status == StatusParcela.ATRASADO ||
                           p.Status == StatusParcela.EM_ANALISE));

            if (moradorTemDividas)
            {
                return BadRequest(new { mensagem = "Não é possível expulsar este morador pois ele possui dívidas ativas. Quite as pendências financeiras dele antes de removê-lo."});
            }

            _context.Pertences.Remove(vinculo);
            _context.SaveChanges();

            return Ok(new { mensagem = "Morador removido da república com sucesso." });
        }

        [HttpPut("{idGrupo}/TransferirAdmin/{idNovoAdmin}")]
        public IActionResult TransferirAdmin(int idGrupo, int idNovoAdmin)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo não encontrado." });
            }

            if (grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador atual pode transferir a liderança da casa." });
            }

            if (idLogado == idNovoAdmin)
            {
                return BadRequest(new { mensagem = "Você já é o administrador desta república." });
            }

            var moradorDestino = _context.Pertences
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idNovoAdmin);

            if (moradorDestino == null)
            {
                return NotFound(new { mensagem = "O usuário escolhido não é um morador desta república." });
            }

            if (!moradorDestino.IdUsuarioNavigation.Ativo)
            {
                return BadRequest(new { mensagem = "Não é possível transferir a liderança para uma conta desativada." });
            }

            grupo.IdAdmin = idNovoAdmin;

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = $"Liderança transferida com sucesso para {moradorDestino.IdUsuarioNavigation.Nome}! Você agora é um morador comum."
            });
        }

        [HttpPut("QuitarDividaAdministrativamente/{idParcela}")]
        public IActionResult QuitarDividaAdmin(int idParcela)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var parcela = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .ThenInclude(d => d.IdGrupoNavigation)
                .FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                return NotFound(new { mensagem = "Parcela não encontrada." });
            }

            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador da república pode quitar dívidas administrativamente." });
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                return BadRequest(new { mensagem = "Esta parcela já está paga e não precisa de intervenção." });
            }

            parcela.Status = StatusParcela.PAGO;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Dívida quitada administrativamente com sucesso! O histórico do morador foi limpo para esta conta."
            });
        }
    }
}