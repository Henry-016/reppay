using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepPay.API.DTOs;
using RepPay.API.Services;
using System.Security.Claims;
using System;
using System.Collections.Generic;

namespace RepPay.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class GrupoController : ControllerBase
    {
        private readonly IGrupoService _grupoService;

        public GrupoController(IGrupoService grupoService)
        {
            _grupoService = grupoService;
        }

        private int? ObterIdUsuarioLogado()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim)) return null;
            return int.Parse(usuarioIdClaim);
        }

        [HttpPost]
        public IActionResult CriarGrupo([FromBody] GrupoRequestDTO request)
        {
            int? idAdmin = ObterIdUsuarioLogado();
            if (idAdmin == null) return Unauthorized(new { mensagem = "Usuário não autenticado!" });

            try
            {
                var response = _grupoService.CriarGrupo(idAdmin.Value, request);
                return Created("", response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost("Entrar")]
        public IActionResult EntrarNoGrupo([FromBody] EntrarGrupoRequestDTO request)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado!" });

            try
            {
                var mensagem = _grupoService.EntrarNoGrupo(idLogado.Value, request);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("Meus")]
        public IActionResult GetMeusGrupos()
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            var meusGrupos = _grupoService.GetMeusGrupos(idLogado.Value);
            return Ok(meusGrupos);
        }

        [HttpGet("{idGrupo}")]
        public IActionResult GetGrupoPorId(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            try
            {
                var response = _grupoService.GetGrupoPorId(idLogado.Value, idGrupo);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
            }
        }

        [HttpGet("{idGrupo}/Membros")]
        public IActionResult GetMembrosDoGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            try
            {
                var membros = _grupoService.GetMembrosDoGrupo(idLogado.Value, idGrupo);
                return Ok(membros);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{idGrupo}/Sair")]
        public IActionResult SairDoGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            try
            {
                var mensagem = _grupoService.SairDoGrupo(idLogado.Value, idGrupo);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{idGrupo}/Expulsar/{idMorador}")]
        public IActionResult ExpulsarMorador(int idGrupo, int idMorador)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            try
            {
                var mensagem = _grupoService.ExpulsarMorador(idLogado.Value, idGrupo, idMorador);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{idGrupo}/TransferirAdmin/{idNovoAdmin}")]
        public IActionResult TransferirAdmin(int idGrupo, int idNovoAdmin)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            try
            {
                var mensagem = _grupoService.TransferirAdmin(idLogado.Value, idGrupo, idNovoAdmin);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("{idGrupo}/ProximaConta")]
        public IActionResult ObterProximaContaGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            try
            {
                var proximaConta = _grupoService.ObterProximaContaGrupo(idLogado.Value, idGrupo);
                return Ok(proximaConta);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
            }
        }

        [HttpDelete("Deletar/{idGrupo}")]
        public IActionResult DeletarGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();
            if (idLogado == null) return Unauthorized(new { mensagem = "Usuário não autenticado." });

            try
            {
                var mensagem = _grupoService.DeletarGrupo(idLogado.Value, idGrupo);
                return Ok(new { mensagem });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}