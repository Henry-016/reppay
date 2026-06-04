using Microsoft.AspNetCore.Mvc;
using RepPay.API.Models;
using RepPay.API.DTOs;
using System.Reflection.Metadata;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace RepPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetTodosUsuarios()
        {
            var usuarios = _context.Usuarios.Select(u => new UsuarioResponseDTO
            {
                IdUsuario = u.IdUsuario,
                Nome = u.Nome,
                Email = u.Email
            })
                .ToList();

            return Ok(usuarios);
        }

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

        [HttpGet("{id}")]
        public IActionResult GetUsuarioPorId(int id)
        {
            var usuario = _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new UsuarioResponseDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nome = u.Nome,
                    Email = u.Email
                })
                .FirstOrDefault();

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado no sistema." });
            }

            return Ok(usuario);
        }

        [HttpPut("{id}")]
        public IActionResult AtualizarUsuario(int id, [FromBody] UsuarioRequestDTO usuarioAtualizado)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado para atualização!" });
            }

            usuario.Nome = usuarioAtualizado.Nome;
            usuario.Email = usuarioAtualizado.Email;

            usuario.Senha = usuarioAtualizado.Senha;

            _context.SaveChanges();

            return Ok(new { mensagem = "Dados do usuário atualizados com sucesso!" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeletarUsuario(int id)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado!" });
            }

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return Ok(new { mensagem = "Usuário deletado do sistema com sucesso!" });
        }

        [HttpPost("EsqueciSenha")]
        public IActionResult EsqueciSenha([FromBody] EsqueciSenhaRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                return Ok(new { mensagem = "Se o e-mail existir em nossa base, um código será enviado!"});
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
                return BadRequest(new { mensagem = "Nenhum código ativo encontrado." });

            if (recuperacao.Tentativas >= 3)
                return BadRequest(new { mensagem = "Muitas tentativas falhas. Solicite um novo código." });

            if (recuperacao.DataExpiracao < DateTime.UtcNow)
                return BadRequest(new { mensagem = "Este código expirou." });

            if (recuperacao.Codigo != request.Codigo)
            {
                recuperacao.Tentativas++;
                _context.SaveChanges();
                return BadRequest(new { mensagem = "Código incorreto." });
            }

            return Ok(new { mensagem = "Código validado com sucesso!" });
        }

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

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });
            }

            bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha);

            if (!senhaValida)
            {
                return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });
            }

            var token = GerarTokenJWT(usuario);

            return Ok(new { mensagem = "Login realizado com sucesso!",
                token = token,
                idUsuario = usuario.IdUsuario,
                nome = usuario.Nome
            });
        }

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

                Expires = DateTime.UtcNow.AddHours(8),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}