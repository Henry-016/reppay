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
    public class DespesaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DespesaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("LançarDespesa")]
        public IActionResult CadastrarDespesa([FromBody] DespesaRequestDTO request)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int idLogado = int.Parse(usuarioIdClaim);

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == request.IdGrupo);

            if (grupo == null || grupo.IdAdmin != idLogado)
            {
                return Forbid();
            }

            var novaDespesa = new Despesa
            {
                Nome = request.Nome,
                Valor = request.Valor,
                Vencimento = DateOnly.Parse(request.Vencimento),
                Icone = request.Icone,
                IdGrupo = request.IdGrupo,
                Status = StatusDespesa.ATIVA
            };

            _context.Despesas.Add(novaDespesa);
            _context.SaveChanges();

            decimal valorPorPessoa = request.Valor / request.MoradoresIds.Count;

            foreach (var idMorador in request.MoradoresIds)
            {
                var parcela = new Parcela
                {
                    IdDespesa = novaDespesa.IdDespesa,
                    IdUsuario = idMorador,
                    Valor = valorPorPessoa,
                    Status = StatusParcela.PENDENTE
                };

                _context.Parcelas.Add(parcela);
            }

            _context.SaveChanges();
            return Created("", new { mensagem = "Despesa lançada e rateio gerado com sucesso!" });
        }

        [HttpGet("ObterMinhasDividas, Morador")]
        public IActionResult GetMinhasDividas()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            int idLogado = int.Parse(usuarioIdClaim);

            var dividas = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO))
                .Select(p => new MinhaDividaResponseDTO
                {
                    IdParcela = p.IdParcela,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Icone = p.IdDespesaNavigation.Icone,
                    Valor = p.Valor,
                    Vencimento = p.IdDespesaNavigation.Vencimento,
                    Status = p.Status.ToString()
                })
                .OrderBy(p => p.Vencimento)
                .ToList();

            if (!dividas.Any())
            {
                return Ok(new { mensagem = "Você não tem dívidas pendentes! Tudo em paz.", dividas = dividas });
            }

            decimal valorTotalDevido = dividas.Sum(d => d.Valor);

            return Ok(new
            {
                TotalDevido = valorTotalDevido,
                ListaDividas = dividas
            });
        }

        [HttpGet("ObterTodasAsDividas, Admin/{idGrupo}")]
        public IActionResult GetInadimplentes(int idGrupo)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Utilizador não autenticado." });
            }

            int idLogado = int.Parse(usuarioIdClaim);

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                return NotFound(new { mensagem = "Grupo não encontrado." });
            }

            if (grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador do grupo pode ver essa lista!" });
            }

            var inadimplentes = _context.Parcelas
                .Include(p => p.IdUsuarioNavigation) 
                .Include(p => p.IdDespesaNavigation) 
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo
                         && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO))
                .Select(p => new InadimplenteResponseDTO
                {
                    IdParcela = p.IdParcela,
                    NomeMorador = p.IdUsuarioNavigation.Nome,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Valor = p.Valor,
                    Vencimento = p.IdDespesaNavigation.Vencimento,
                    Status = p.Status.ToString()
                })
                .OrderBy(p => p.Vencimento)
                .ThenBy(p => p.NomeMorador)
                .ToList();

            if (!inadimplentes.Any())
            {
                return Ok(new { mensagem = "Nenhum morador tem dívidas neste grupo. Tudo perfeito!", listaInadimplentes = inadimplentes });
            }

            decimal totalAReceber = inadimplentes.Sum(i => i.Valor);

            return Ok(new
            {
                TotalAReceber = totalAReceber,
                ListaInadimplentes = inadimplentes
            });
        }

        [HttpPut("SinalizarPagamento/{idParcela}")]
        public IActionResult PagarParcela(int idParcela)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Utilizador não autenticado." });
            }

            int idLogado = int.Parse(usuarioIdClaim);

            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                return NotFound(new { mensagem = "Parcela não encontrada." });
            }

            if (parcela.IdUsuario != idLogado)
            {
                return StatusCode(403, new { mensagem = "Não tem permissão para alterar uma dívida que não lhe pertence!" });
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                return BadRequest(new { mensagem = "Esta parcela já se encontra paga." });
            }

            parcela.Status = StatusParcela.EM_ANALISE;

            _context.SaveChanges();

            return Ok(new { mensagem = "Pagamento sinalizado! Aguardando validação do administrador." });
        }

        [HttpPut("DesfazerSinalizaçãoPagamento/{idParcela}")]
        public IActionResult DesfazerPagamento(int idParcela)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Utilizador não autenticado." });
            }

            int idLogado = int.Parse(usuarioIdClaim);
            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                return NotFound(new { mensagem = "Parcela não encontrada." });
            }

            if (parcela.IdUsuario != idLogado)
            {
                return StatusCode(403, new { mensagem = "Não tem permissão para alterar uma dívida que não lhe pertence!" });
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                return BadRequest(new { mensagem = "Só é possível desfazer pagamentos que ainda estão em análise." });
            }

            parcela.Status = StatusParcela.PENDENTE;

            _context.SaveChanges();

            return Ok(new { mensagem = "Sinalização de pagamento desfeita com sucesso." });
        }

        [HttpPut("ValidarPagamento/{idParcela}")]
        public IActionResult ValidarPagamento(int idParcela, [FromBody] ValidarPagamentoRequestDTO request)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            int idLogado = int.Parse(usuarioIdClaim);

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
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador do grupo pode validar pagamentos." });
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                return BadRequest(new { mensagem = "Esta parcela não está aguardando validação." });
            }

            if (request.Aprovado)
            {
                parcela.Status = StatusParcela.PAGO;
                parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            }
            else
            {

                if (DateOnly.FromDateTime(DateTime.UtcNow) > parcela.IdDespesaNavigation.Vencimento)
                {
                    parcela.Status = StatusParcela.ATRASADO;
                }
                else
                {
                    parcela.Status = StatusParcela.PENDENTE;
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = request.Aprovado
                    ? "Pagamento aprovado com sucesso! A parcela foi quitada."
                    : "Pagamento rejeitado. A dívida voltou para o morador."
            });
        }

        [HttpGet("ObterHistóricoDespesasPagas")]
        public IActionResult GetMeuHistoricoPago()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            int idLogado = int.Parse(usuarioIdClaim);

            var historicoPago = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.Status == StatusParcela.PAGO)
                .Select(p => new HistoricoPagoResponseDTO
                {
                    IdParcela = p.IdParcela,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Icone = p.IdDespesaNavigation.Icone,
                    ValorPago = p.Valor,
                    DataPagamento = p.DataPagamento,
                    Vencimento = p.IdDespesaNavigation.Vencimento
                })
                .OrderByDescending(p => p.DataPagamento)
                .ToList();

            if (!historicoPago.Any())
            {
                return Ok(new { mensagem = "Você ainda não possui pagamentos registrados no histórico.", historicoPago = historicoPago });
            }

            return Ok(historicoPago);
        }
    }
}