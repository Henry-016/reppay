using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepPay.API.DTOs;
using RepPay.API.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private int? ObterIdUsuarioLogado()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                return null;
            }
            return int.Parse(usuarioIdClaim);
        }

        [HttpPost("LancarDespesa")]
        public IActionResult CadastrarDespesa([FromBody] DespesaRequestDTO request)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == request.IdGrupo);

            if (grupo == null || grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador pode lançar despesas." });
            }

            if (request.MoradoresIds == null || request.MoradoresIds.Count == 0)
            {
                return BadRequest(new { mensagem = "É necessário selecionar pelo menos um morador para dividir esta conta." });
            }

            var moradoresValidos = _context.Pertences
                .Where(p => p.IdGrupo == request.IdGrupo && request.MoradoresIds.Contains(p.IdUsuario))
                .Select(p => p.IdUsuario)
                .ToList();

            if (moradoresValidos.Count != request.MoradoresIds.Count)
            {
                return BadRequest(new { mensagem = "Um ou mais moradores informados não existem ou não pertencem a esta república." });
            }

            var novaDespesa = new Despesa
            {
                Nome = request.Nome,
                Valor = request.Valor,
                Vencimento = request.Vencimento,
                Icone = request.Icone,
                IdGrupo = request.IdGrupo,
                Status = StatusDespesa.ATIVA,
                Parcelas = new List<Parcela>()
            };

            decimal valorPorPessoa = Math.Round(request.Valor / request.MoradoresIds.Count, 2);

            foreach (var idMorador in request.MoradoresIds)
            {
                novaDespesa.Parcelas.Add(new Parcela
                {
                    IdUsuario = idMorador,
                    Valor = valorPorPessoa,
                    Status = StatusParcela.PENDENTE
                });
            }

            _context.Despesas.Add(novaDespesa);
            _context.SaveChanges();

            return Created("", new { mensagem = "Despesa lançada e rateio gerado com sucesso!" });
        }

        [HttpGet("MinhasDividas")]
        public IActionResult GetMinhasDividas()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var dividas = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.IdDespesaNavigation.Ativo == true && p.IdDespesaNavigation.IdGrupoNavigation.Ativo == true 
                 && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO))
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

        [HttpGet("Inadimplentes/{idGrupo}")]
        public IActionResult GetInadimplentes(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null) return NotFound(new { mensagem = "Grupo não encontrado." });

            if (grupo.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador do grupo pode ver essa lista!" });
            }

            var inadimplentes = _context.Parcelas
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo && p.IdDespesaNavigation.Ativo == true
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
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null) return NotFound(new { mensagem = "Parcela não encontrada." });

            if (parcela.IdUsuario != idLogado)
            {
                return StatusCode(403, new { mensagem = "Não tem permissão para alterar uma dívida que não lhe pertence!" });
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                return BadRequest(new { mensagem = "Esta parcela já se encontra paga." });
            }

            parcela.Status = StatusParcela.EM_ANALISE;

            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);

            _context.SaveChanges();

            return Ok(new { mensagem = "Pagamento sinalizado! Aguardando validação do administrador." });
        }

        [HttpPut("DesfazerSinalizacaoPagamento/{idParcela}")]
        public IActionResult DesfazerPagamento(int idParcela)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null) return NotFound(new { mensagem = "Parcela não encontrada." });

            if (parcela.IdUsuario != idLogado)
            {
                return StatusCode(403, new { mensagem = "Não tem permissão para alterar uma dívida que não lhe pertence!" });
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                return BadRequest(new { mensagem = "Só é possível desfazer pagamentos que ainda estão em análise." });
            }

            parcela.DataPagamento = null;
            parcela.Status = StatusParcela.PENDENTE;

            _context.SaveChanges();

            return Ok(new { mensagem = "Sinalização de pagamento desfeita com sucesso." });
        }

        [HttpPut("ValidarPagamento/{idParcela}")]
        public IActionResult ValidarPagamento(int idParcela, [FromBody] ValidarPagamentoRequestDTO request)
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
                return StatusCode(403, new { mensagem = "Acesso negado. Apenas o administrador do grupo pode validar pagamentos." });
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                return BadRequest(new { mensagem = "Esta parcela não está aguardando validação." });
            }

            if (request.Aprovado)
            {
                parcela.Status = StatusParcela.PAGO;
            }
            else
            {
                parcela.DataPagamento = null;

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

        [HttpGet("HistoricoPago")]
        public IActionResult GetMeuHistoricoPago()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var historicoPago = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.Status == StatusParcela.PAGO && p.IdDespesaNavigation.Ativo == true)
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

        [HttpGet("HistoricoGrupo/{idGrupo}")]
        public IActionResult GetHistoricoPagoGrupo(int idGrupo)
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
                return StatusCode(403, new { mensagem = "Apenas o administrador pode ver o histórico financeiro global." });
            }

            var historico = _context.Parcelas
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo
                         && p.IdDespesaNavigation.Ativo == true
                         && p.Status == StatusParcela.PAGO)
                .Select(p => new
                {
                    idParcela = p.IdParcela,
                    nomeMorador = p.IdUsuarioNavigation.Nome,
                    nomeDespesa = p.IdDespesaNavigation.Nome,
                    valorPago = p.Valor,
                    dataPagamento = p.DataPagamento,
                    vencimento = p.IdDespesaNavigation.Vencimento
                })
                .OrderByDescending(p => p.dataPagamento)
                .ToList();

            if (!historico.Any())
            {
                return Ok(new { mensagem = "Nenhum histórico de pagamento registrado neste grupo.", listaHistorico = historico });
            }  

            return Ok(new { listaHistorico = historico });
        }

        [HttpGet("MinhasAnalises")]
        public IActionResult GetMinhasAnalises()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var analises = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado
                         && p.IdDespesaNavigation.Ativo == true
                         && p.Status == StatusParcela.EM_ANALISE)
                .Select(p => new
                {
                    idParcela = p.IdParcela,
                    nomeDespesa = p.IdDespesaNavigation.Nome,
                    icone = p.IdDespesaNavigation.Icone,
                    valor = p.Valor,
                    vencimento = p.IdDespesaNavigation.Vencimento,
                    dataSinalizacao = p.DataPagamento
                })
                .OrderByDescending(p => p.dataSinalizacao)
                .ToList();

            if (!analises.Any())
            {
                return Ok(new { mensagem = "Nenhum pagamento em análise no momento.", listaAnalises = analises });
            }

            return Ok(new { listaAnalises = analises });
        }

        [HttpGet("AnalisesPendentes/{idGrupo}")]
        public IActionResult GetAnalisesPendentesGrupo(int idGrupo)
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
                return StatusCode(403, new { mensagem = "Apenas o administrador pode ver as validações pendentes." });
            }

            var analisesPendentes = _context.Parcelas
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo
                         && p.IdDespesaNavigation.Ativo == true
                         && p.Status == StatusParcela.EM_ANALISE)
                .Select(p => new
                {
                    idParcela = p.IdParcela,
                    nomeMorador = p.IdUsuarioNavigation.Nome,
                    nomeDespesa = p.IdDespesaNavigation.Nome,
                    valor = p.Valor,
                    dataSinalizacao = p.DataPagamento
                })
                .OrderBy(p => p.dataSinalizacao)
                .ToList();

            if (!analisesPendentes.Any())
            {
                return Ok(new { mensagem = "Nenhuma validação pendente. Tudo atualizado!", listaAnalises = analisesPendentes });
            }

            return Ok(new { listaAnalises = analisesPendentes });
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

        [HttpPut("Editar/{idDespesa}")]
        public IActionResult EditarDespesa(int idDespesa, [FromBody] DespesaRequestDTO request)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .FirstOrDefault(d => d.IdDespesa == idDespesa);

            if (despesa == null)
            {
                return NotFound(new { mensagem = "Despesa não encontrada." });
            }

            if (despesa.IdGrupoNavigation.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Apenas o administrador pode editar despesas." });
            }

            despesa.Nome = request.Nome;
            despesa.Valor = request.Valor;
            despesa.Vencimento = request.Vencimento;
            despesa.Icone = request.Icone;

            try
            {
                _context.SaveChanges();
                return Ok(new { mensagem = "Despesa atualizada com sucesso!" });
            }
            catch (Exception)
            {
                return BadRequest(new { mensagem = "Não é permitido alterar o valor ou o vencimento de uma despesa que já possui parcelas pagas ou em análise." });
            }
        }

        [HttpDelete("Deletar/{idDespesa}")]
        public IActionResult DeletarDespesa(int idDespesa)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .FirstOrDefault(d => d.IdDespesa == idDespesa);

            if (despesa == null)
            {
                return NotFound(new { mensagem = "Despesa não encontrada." });
            }

            if (despesa.IdGrupoNavigation.IdAdmin != idLogado)
            {
                return StatusCode(403, new { mensagem = "Apenas o administrador pode apagar despesas." });
            }
                
            despesa.Ativo = false;

            try
            {
                _context.SaveChanges();
                return Ok(new { mensagem = "Despesa arquivada com sucesso!" });
            }
            catch (Exception)
            {
                return BadRequest(new { mensagem = "Não é possível deletar uma despesa que ainda possui parcelas pagas!" });
            }
        }
    }
}