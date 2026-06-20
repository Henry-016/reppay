using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepPay.API.DTOs;
using RepPay.API.Services;
using System.Security.Claims;
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
        private readonly IDespesaService _despesaService;

        public DespesaController(IDespesaService despesaService)
        {
            _despesaService = despesaService;
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

            try
            {
                var mensagem = _despesaService.CadastrarDespesa(idLogado.Value, request);
                return Created("", new { mensagem });
            }

            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
        }

        [HttpGet("MinhasDividas/{idGrupo}")]
        public IActionResult GetMinhasDividas(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var resultado = _despesaService.GetMinhasDividas(idLogado.Value, idGrupo);

            if (!resultado.ListaDividas.Any())
            {
                return Ok(new { mensagem = "Você não tem dívidas pendentes! Tudo em paz.", dividas = resultado.ListaDividas });
            }

            return Ok(resultado);
        }

        [HttpGet("Inadimplentes/{idGrupo}")]
        public IActionResult GetInadimplentes(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var resultado = _despesaService.GetInadimplentes(idLogado.Value, idGrupo);

                if (!resultado.ListaInadimplentes.Any())
                {
                    return Ok(new { mensagem = "Nenhum morador tem dívidas neste grupo. Tudo perfeito!", listaInadimplentes = resultado.ListaInadimplentes });
                }

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
        }

        [HttpPut("SinalizarPagamento/{idParcela}")]
        public IActionResult PagarParcela(int idParcela)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var mensagem = _despesaService.PagarParcela(idLogado.Value, idParcela);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
        }

        [HttpPut("DesfazerSinalizacaoPagamento/{idParcela}")]
        public IActionResult DesfazerPagamento(int idParcela)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var mensagem = _despesaService.DesfazerPagamento(idLogado.Value, idParcela);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
        }

        [HttpPut("ValidarPagamento/{idParcela}")]
        public IActionResult ValidarPagamento(int idParcela, [FromBody] ValidarPagamentoRequestDTO request)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var mensagem = _despesaService.ValidarPagamento(idLogado.Value, idParcela, request);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
        }

        [HttpGet("HistoricoPago/{idGrupo}")]
        public IActionResult GetMeuHistoricoPago(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var historico = _despesaService.GetMeuHistoricoPago(idLogado.Value, idGrupo);

            if (!historico.Any())
            {
                return Ok(new { mensagem = "Você ainda não possui pagamentos registrados no histórico.", historicoPago = historico });
            }

            return Ok(historico);
        }

        [HttpGet("HistoricoGrupo/{idGrupo}")]
        public IActionResult GetHistoricoPagoGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var historico = _despesaService.GetHistoricoPagoGrupo(idLogado.Value, idGrupo);

                if (!historico.Any())
                {
                    return Ok(new { mensagem = "Nenhum histórico de pagamento registrado neste grupo.", listaHistorico = historico });
                }

                return Ok(new { listaHistorico = historico });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
        }

        [HttpGet("MinhasAnalises/{idGrupo}")]
        public IActionResult GetMinhasAnalises(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var analises = _despesaService.GetMinhasAnalises(idLogado.Value, idGrupo);

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

            try
            {
                var analises = _despesaService.GetAnalisesPendentesGrupo(idLogado.Value, idGrupo);

                if (!analises.Any())
                {
                    return Ok(new { mensagem = "Nenhuma validação pendente. Tudo atualizado!", listaAnalises = analises });
                }

                return Ok(new { listaAnalises = analises });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
        }

        [HttpPut("QuitarDividaAdministrativamente/{idParcela}")]
        public IActionResult QuitarDividaAdmin(int idParcela)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var mensagem = _despesaService.QuitarDividaAdmin(idLogado.Value, idParcela);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
        }

        [HttpPut("Editar/{idDespesa}")]
        public IActionResult EditarDespesa(int idDespesa, [FromBody] EditarDespesaRequestDTO request)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var mensagem = _despesaService.EditarDespesa(idLogado.Value, idDespesa, request);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
        }

        [HttpDelete("Deletar/{idDespesa}")]
        public IActionResult DeletarDespesa(int idDespesa)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            try
            {
                var mensagem = _despesaService.DeletarDespesa(idLogado.Value, idDespesa);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { mensagem = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensagem = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
        }

        [HttpGet("gerenciamento/grupo/{idGrupo}")]
        [Authorize] 
        public IActionResult GetDespesasParaGerenciamento(int idGrupo)
        {
            try
            {
                var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirst("id");

                if (claimId == null)
                {
                    return Unauthorized(new { erro = "Token inválido ou não contém a identificação do usuário." });
                }

                int idLogado = int.Parse(claimId.Value);

                var despesas = _despesaService.GetDespesasParaGerenciamento(idLogado, idGrupo);

                return Ok(despesas);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}