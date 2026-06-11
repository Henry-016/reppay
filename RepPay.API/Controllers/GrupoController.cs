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
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado!" });
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
                mensagem = "Rep�blica criada com sucesso!",
                codigoAcesso = codigoAcesso
            });
        }

        [HttpPost("Entrar")]
        public IActionResult EntrarNoGrupo([FromBody] EntrarGrupoRequestDTO request)
        {
            int? idUsuario = ObterIdUsuarioLogado();

            if (idUsuario == null)
            {
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado!" });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.CodigoAcesso.ToLower() == request.CodigoAcesso.ToLower());

            if (grupo == null)
            {
                return NotFound(new { mensagem = "C�digo de acesso inv�lido ou rep�blica n�o encontrada." });
            }

            bool jaPertence = _context.Pertences.Any(p => p.IdGrupo == grupo.IdGrupo && p.IdUsuario == idUsuario);

            if (jaPertence)
            {
                return BadRequest(new { mensagem = "Voc� j� faz parte desta rep�blica!" });
            }

            var novoVinculo = new Pertence
            {
                IdGrupo = grupo.IdGrupo,
                IdUsuario = idUsuario.Value
            };

            _context.Pertences.Add(novoVinculo);
            _context.SaveChanges();

            return Ok(new { mensagem = $"Bem-vindo(a) � {grupo.Nome}!" });
        }

        [HttpGet("Meus")]
        public IActionResult GetMeusGrupos()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
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
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
            }

            var relacaoPertence = _context.Pertences
                .Include(p => p.IdGrupoNavigation)
                .FirstOrDefault(p => p.IdUsuario == idLogado && p.IdGrupo == idGrupo);

            if (relacaoPertence == null)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Voc� n�o pertence a este grupo ou ele n�o existe." });
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
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo n�o encontrado." });
            }

            bool usuarioPertence = _context.Pertences.Any(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado);

            if (!usuarioPertence)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Voc� n�o pertence a este grupo." });
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
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
            }

            var vinculo = _context.Pertences.FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado);

            if (vinculo == null)
            {
                return NotFound(new { mensagem = "Voc� n�o pertence a esta rep�blica." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo != null && grupo.IdAdmin == idLogado)
            {
                return BadRequest(new { mensagem = "Voc� � o administrador do grupo. Transfira a lideran�a para outro morador antes de sair." });
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
                    mensagem = "Voc� possui d�vidas pendentes ou em an�lise nesta rep�blica. Quite todas as contas antes de sair!"
                });
            }

            _context.Pertences.Remove(vinculo);
            _context.SaveChanges();

            return Ok(new { mensagem = "Voc� saiu da rep�blica com sucesso. Sentiremos sua falta!" });
        }

        [HttpDelete("{idGrupo}/Expulsar/{idMorador}")]
        public IActionResult ExpulsarMorador(int idGrupo, int idMorador)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo n�o encontrado." });
            }

            if (grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador pode expulsar moradores." });
            }

            if (idLogado == idMorador)
            {
                return BadRequest(new { mensagem = "Voc� n�o pode expulsar a si mesmo. Caso queira sair, utilize a op��o de sa�da volunt�ria ou exclua o grupo." });
            }

            var vinculo = _context.Pertences.FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idMorador);

            if (vinculo == null)
            {
                return NotFound(new { mensagem = "Este usu�rio n�o � um morador da sua rep�blica." });
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
                return BadRequest(new { mensagem = "N�o � poss�vel expulsar este morador pois ele possui d�vidas ativas. Quite as pend�ncias financeiras dele antes de remov�-lo."});
            }

            _context.Pertences.Remove(vinculo);
            _context.SaveChanges();

            return Ok(new { mensagem = "Morador removido da rep�blica com sucesso." });
        }

        [HttpPut("{idGrupo}/TransferirAdmin/{idNovoAdmin}")]
        public IActionResult TransferirAdmin(int idGrupo, int idNovoAdmin)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo n�o encontrado." });
            }

            if (grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador atual pode transferir a lideran�a da casa." });
            }

            if (idLogado == idNovoAdmin)
            {
                return BadRequest(new { mensagem = "Voc� j� � o administrador desta rep�blica." });
            }

            var moradorDestino = _context.Pertences
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idNovoAdmin);

            if (moradorDestino == null)
            {
                return NotFound(new { mensagem = "O usu�rio escolhido n�o � um morador desta rep�blica." });
            }

            if (!moradorDestino.IdUsuarioNavigation.Ativo)
            {
                return BadRequest(new { mensagem = "N�o � poss�vel transferir a lideran�a para uma conta desativada." });
            }

            grupo.IdAdmin = idNovoAdmin;

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = $"Lideran�a transferida com sucesso para {moradorDestino.IdUsuarioNavigation.Nome}! Voc� agora � um morador comum."
            });
        }

        [HttpPut("QuitarDividaAdministrativamente/{idParcela}")]
        public IActionResult QuitarDividaAdmin(int idParcela)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
            }

            var parcela = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .ThenInclude(d => d.IdGrupoNavigation)
                .FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                return NotFound(new { mensagem = "Parcela n�o encontrada." });
            }

            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador da rep�blica pode quitar d�vidas administrativamente." });
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                return BadRequest(new { mensagem = "Esta parcela j� est� paga e n�o precisa de interven��o." });
            }

            parcela.Status = StatusParcela.PAGO;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "D�vida quitada administrativamente com sucesso! O hist�rico do morador foi limpo para esta conta."
            });
        }

        [HttpDelete("Deletar/{idGrupo}")]
        public IActionResult DeletarGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usu�rio n�o autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo n�o encontrado." });
            }

            if (grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Apenas o administrador pode encerrar a rep�blica." });
            }
                
            int quantidadeMoradores = _context.Pertences.Count(p => p.IdGrupo == idGrupo);

            if (quantidadeMoradores > 1)
            {
                return BadRequest(new
                {
                    mensagem = "N�o � poss�vel encerrar a rep�blica enquanto houver outros moradores nela. Pe�a para que saiam voluntariamente ou remova-os primeiro."
                });
            }

            grupo.Ativo = false;

            try
            {
                _context.SaveChanges();
                return Ok(new { mensagem = "Rep�blica encerrada com sucesso! Todas as despesas atreladas foram arquivadas." });
            }
            catch (Exception)
            {
                return BadRequest(new { mensagem = "N�o � poss�vel encerrar a rep�blica no momento. Existem despesas com parcelas pendentes ou em an�lise. Quite todas as contas primeiro." });
            }
        }
    } 
}