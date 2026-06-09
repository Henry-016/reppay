using Microsoft.AspNetCore.Mvc;
using RepPay.API.Models;
using RepPay.API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace RepPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
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

        // Devo mover esta chave para o appsettings.json no futuro!!!
        private string GerarTokenJWT(Models.Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("MinhaSuperChaveSecretaDoRepPay2026!!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Name, usuario.Nome)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CriarUsuario([FromBody] UsuarioRequestDTO novoUsuarioDTO)
        {
            if (_context.Usuarios.Any(u => u.Email.ToLower() == novoUsuarioDTO.Email.ToLower()))
            {
                return BadRequest(new { mensagem = "Este e-mail já está cadastrado no sistema!" });
            }

            var usuarioParaSalvar = new Usuario
            {
                Nome = novoUsuarioDTO.Nome,
                Email = novoUsuarioDTO.Email,
                Senha = BCrypt.Net.BCrypt.HashPassword(novoUsuarioDTO.Senha)
            };

            _context.Usuarios.Add(usuarioParaSalvar);
            _context.SaveChanges();

            return Created("", new { mensagem = "Usuário cadastrado com total segurança!" });
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });
            }

            if (!usuario.Ativo)
            {
                return Unauthorized(new { mensagem = "Esta conta foi desativada e não possui mais acesso ao sistema." });
            }

            bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha);

            if (!senhaValida)
            {
                return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });
            }

            var token = GerarTokenJWT(usuario);

            var tokenAleatorio = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var novoRefreshToken = new RefreshToken
            {
                TokenHash = tokenAleatorio,
                IdUsuario = usuario.IdUsuario,
                DataExpiracao = DateTime.UtcNow.AddDays(7),
                Revogado = false
            };

            _context.RefreshTokens.Add(novoRefreshToken);
            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Login realizado com sucesso!",
                token = token,
                refreshToken = tokenAleatorio,
                idUsuario = usuario.IdUsuario,
                nome = usuario.Nome
            });
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public IActionResult RenovacaoToken([FromBody] RefreshTokenRequestDTO request)
        {
            var tokenBanco = _context.RefreshTokens
                .Include(t => t.IdUsuarioNavigation)
                .FirstOrDefault(t => t.TokenHash == request.RefreshToken);

            if (tokenBanco == null)
            {
                return Unauthorized(new { mensagem = "Refresh Token inválido ou inexistente." });
            }

            if (tokenBanco.Revogado)
            {
                return Unauthorized(new { mensagem = "Este token já foi revogado por segurança." });

            }

            if (tokenBanco.DataExpiracao < DateTime.UtcNow)
            {
                return Unauthorized(new { mensagem = "Sua sessão expirou. Por favor, faça login novamente." });
            }

            if (!tokenBanco.IdUsuarioNavigation.Ativo)
            {
                return Unauthorized(new { mensagem = "A conta vinculada a esta sessão está inativa." });
            }

            tokenBanco.Revogado = true;

            var novoJwt = GerarTokenJWT(tokenBanco.IdUsuarioNavigation);
            var novoTokenAleatorio = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var novoRefreshToken = new RefreshToken
            {
                TokenHash = novoTokenAleatorio,
                IdUsuario = tokenBanco.IdUsuario,
                DataCriacao = DateTime.UtcNow,
                DataExpiracao = DateTime.UtcNow.AddDays(7),
                Revogado = false
            };

            _context.RefreshTokens.Add(novoRefreshToken);
            _context.SaveChanges();

            return Ok(new
            {
                token = novoJwt,
                refreshToken = novoTokenAleatorio
            });
        }

        [HttpPost("LogOut")]
        public IActionResult LogOut([FromBody] RefreshTokenRequestDTO request)
        {
            var tokenBanco = _context.RefreshTokens.FirstOrDefault(t => t.TokenHash == request.RefreshToken);

            if (tokenBanco != null)
            {
                tokenBanco.Revogado = true;
                _context.SaveChanges();
            }

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

            var usuario = _context.Usuarios
                .Where(u => u.IdUsuario == idLogado)
                .Select(u => new UsuarioResponseDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nome = u.Nome,
                    Email = u.Email
                })
                .FirstOrDefault();

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado." });
            }

            return Ok(usuario);
        }

        [HttpPut("Atualizar")]
        public IActionResult AtualizarUsuario([FromBody] UsuarioRequestDTO usuarioAtualizado)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idLogado);

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado." });
            }

            usuario.Nome = usuarioAtualizado.Nome;
            usuario.Email = usuarioAtualizado.Email;
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuarioAtualizado.Senha);

            _context.SaveChanges();

            return Ok(new { mensagem = "Dados do usuário atualizados com sucesso!" });
        }

        [HttpDelete("Deletar")]
        public IActionResult DeletarUsuario()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idLogado);

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado!" });
            }

            bool isAdminDeGrupoAtivo = _context.Grupos.Any(g => g.IdAdmin == idLogado && g.Ativo);

            if (isAdminDeGrupoAtivo)
            {
                return BadRequest(new { mensagem = "Não é possível excluir sua conta no momento. Você é o administrador de uma república ativa. Transfira a liderança."});
            }

            bool temDividasPendentes = _context.Parcelas.Any(p => p.IdUsuario == idLogado &&
            (p.Status == StatusParcela.PENDENTE ||
             p.Status == StatusParcela.ATRASADO ||
             p.Status == StatusParcela.EM_ANALISE));

            if (temDividasPendentes)
            {
                return BadRequest(new { mensagem = "Você possui contas pendentes ou em análise. Quite todas as suas dívidas antes de excluir a conta."});
            }

            usuario.Ativo = false;

            _context.SaveChanges();

            return Ok(new { mensagem = "Sua conta foi excluída com sucesso!" });
        }

        [AllowAnonymous]
        [HttpPost("EsqueciSenha")]
        public IActionResult EsqueciSenha([FromBody] EsqueciSenhaRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                return Ok(new { mensagem = "Se o e-mail existir em nossa base, um código será enviado!" });
            }

            Random random = new Random();
            string codigo = random.Next(100000, 999999).ToString();

            var novoCodigo = new CodigoRecuperacao
            {
                Codigo = codigo,
                DataExpiracao = DateTime.UtcNow.AddMinutes(15),
                CodigoUsado = false,
                Tentativas = 0,
                IdUsuario = usuario.IdUsuario
            };

            _context.CodigosRecuperacao.Add(novoCodigo);
            _context.SaveChanges();

            Console.WriteLine("\n========================================================");
            Console.WriteLine($"📧 MOCK EMAIL -> Para: {usuario.Email} | Código: {codigo}");
            Console.WriteLine("========================================================\n");

            return Ok(new { mensagem = "Se o e-mail existir em nossa base, um código será enviado!" });
        }

        [AllowAnonymous]
        [HttpPost("ValidarCodigo")]
        public IActionResult ValidarCodigo([FromBody] ValidarCodigoRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                return BadRequest(new { mensagem = "Dados inválidos." });
            }

            var recuperacao = _context.CodigosRecuperacao
                .Where(c => c.IdUsuario == usuario.IdUsuario && !c.CodigoUsado)
                .OrderByDescending(c => c.DataExpiracao)
                .FirstOrDefault();

            if (recuperacao == null)
            {
                return BadRequest(new { mensagem = "Nenhum código ativo encontrado." });
            }

            if (recuperacao.Tentativas >= 3)
            {
                return BadRequest(new { mensagem = "Muitas tentativas falhas. Solicite um novo código." });
            }

            if (recuperacao.DataExpiracao < DateTime.UtcNow)
            {
                return BadRequest(new { mensagem = "Este código expirou." });
            }

            if (recuperacao.Codigo != request.Codigo)
            {
                recuperacao.Tentativas++;
                _context.SaveChanges();
                return BadRequest(new { mensagem = "Código incorreto." });
            }

            return Ok(new { mensagem = "Código validado com sucesso!" });
        }

        [AllowAnonymous]
        [HttpPost("ResetarSenha")]
        public IActionResult ResetarSenha([FromBody] ResetarSenhaRequestDTO request)
        {
            const int limiteDeTentativas = 3;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                return BadRequest(new { mensagem = "Dados inválidos." });
            }

            var recuperacao = _context.CodigosRecuperacao
                .Where(c => c.IdUsuario == usuario.IdUsuario && !c.CodigoUsado)
                .OrderByDescending(c => c.DataExpiracao)
                .FirstOrDefault();

            if (recuperacao == null || recuperacao.DataExpiracao < DateTime.UtcNow || recuperacao.Tentativas >= limiteDeTentativas)
            {
                return BadRequest(new { mensagem = "Falha na validação final do código!" });
            }

            if (recuperacao.Codigo != request.Codigo)
            {
                recuperacao.Tentativas++;
                _context.SaveChanges();
                return BadRequest(new { mensagem = "Código incorreto." });
            }

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
            recuperacao.CodigoUsado = true;
            _context.SaveChanges();

            return Ok(new { mensagem = "Sua senha foi redefinida com sucesso!" });
        }

        [HttpGet("Home/ProximaConta")]
        public IActionResult ObterProximaContaGeral()
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            var proximaConta = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .ThenInclude(d => d.IdGrupoNavigation)
                .Where(p => p.IdUsuario == idLogado
                         && p.IdDespesaNavigation.Ativo == true
                         && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO))
                .OrderBy(p => p.IdDespesaNavigation.Vencimento)
                .Select(p => new ProximaContaResponseDTO
                {
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    NomeGrupo = p.IdDespesaNavigation.IdGrupoNavigation.Nome,
                    Vencimento = p.IdDespesaNavigation.Vencimento,
                    Valor = p.Valor
                })
                .FirstOrDefault();

            return Ok(proximaConta);
        }

        [HttpGet("{idGrupo}/ProximaConta")]
        public IActionResult ObterProximaContaGrupo(int idGrupo)
        {
            int? idLogado = ObterIdUsuarioLogado();

            if (idLogado == null)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

            if (!_context.Pertences.Any(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado))
            {
                return StatusCode(403, new { mensagem = "Você não pertence a esta república." });
            }

            var proximaConta = _context.Despesas
                .Where(d => d.IdGrupo == idGrupo
                         && d.Ativo == true
                         && d.Status == StatusDespesa.ATIVA)
                .OrderBy(d => d.Vencimento)
                .Select(d => new ProximaContaResponseDTO
                {
                    NomeDespesa = d.Nome,
                    NomeGrupo = null,
                    Vencimento = d.Vencimento,
                    Valor = d.Valor
                })
                .FirstOrDefault();

            return Ok(proximaConta);
        }
    }
}