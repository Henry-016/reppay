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
    }
}