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

        [HttpPost]
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

        [HttpGet("MinhasDividas")]
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

        [HttpGet("Inadimplentes/{idGrupo}")]
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
    }
}