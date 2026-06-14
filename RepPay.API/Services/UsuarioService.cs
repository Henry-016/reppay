using RepPay.API.DTOs;
using RepPay.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace RepPay.API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UsuarioService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private string GerarTokenJWT(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var chaveSecreta = _config["Jwt:Key"];
            var key = Encoding.ASCII.GetBytes(chaveSecreta!);
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

        public void CriarUsuario(UsuarioRequestDTO novoUsuarioDTO)
        {
            if (_context.Usuarios.Any(u => u.Email.ToLower() == novoUsuarioDTO.Email.ToLower()))
            {
                throw new Exception("Este e-mail já está cadastrado no sistema!");
            }

            var usuarioParaSalvar = new Usuario
            {
                Nome = novoUsuarioDTO.Nome,
                Email = novoUsuarioDTO.Email,
                Senha = BCrypt.Net.BCrypt.HashPassword(novoUsuarioDTO.Senha)
            };

            _context.Usuarios.Add(usuarioParaSalvar);
            _context.SaveChanges();
        }

        public LoginResponseDTO Login(LoginRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null || !usuario.Ativo || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
            {
                throw new UnauthorizedAccessException("E-mail ou senha incorretos, ou conta desativada.");
            }

            var token = GerarTokenJWT(usuario);
            var tokenAleatorio = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var novoRefreshToken = new RefreshToken
            {
                TokenHash = tokenAleatorio,
                IdUsuario = usuario.IdUsuario,
                DataCriacao = DateTime.UtcNow,
                DataExpiracao = DateTime.UtcNow.AddDays(7),
                Revogado = false
            };

            _context.RefreshTokens.Add(novoRefreshToken);
            _context.SaveChanges();

            return new LoginResponseDTO
            {
                Mensagem = "Login realizado com sucesso!",
                Token = token,
                RefreshToken = tokenAleatorio,
                IdUsuario = usuario.IdUsuario,
                Nome = usuario.Nome
            };
        }

        public TokenResponseDTO RenovacaoToken(RefreshTokenRequestDTO request)
        {
            var tokenBanco = _context.RefreshTokens
                .Include(t => t.IdUsuarioNavigation)
                .FirstOrDefault(t => t.TokenHash == request.RefreshToken);

            if (tokenBanco == null)
            {
                throw new UnauthorizedAccessException("Refresh Token inválido ou inexistente.");
            }

            if (tokenBanco.Revogado)
            {
                throw new UnauthorizedAccessException("Este token já foi revogado por segurança.");
            }

            if (tokenBanco.DataExpiracao < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Sua sessão expirou. Por favor, faça login novamente.");
            }

            if (!tokenBanco.IdUsuarioNavigation.Ativo)
            {
                throw new UnauthorizedAccessException("A conta vinculada a esta sessão está inativa.");
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

            return new TokenResponseDTO { Token = novoJwt, RefreshToken = novoTokenAleatorio };
        }

        public void LogOut(RefreshTokenRequestDTO request)
        {
            var tokenBanco = _context.RefreshTokens.FirstOrDefault(t => t.TokenHash == request.RefreshToken);

            if (tokenBanco != null)
            {
                tokenBanco.Revogado = true;
                _context.SaveChanges();
            }
        }

        public UsuarioResponseDTO GetMeuPerfil(int idLogado)
        {
            var usuario = _context.Usuarios
                .Where(u => u.IdUsuario == idLogado)
                .Select(u => new UsuarioResponseDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nome = u.Nome,
                    Email = u.Email
                }).FirstOrDefault();

            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            return usuario;
        }

        public void AtualizarUsuario(int idLogado, UsuarioRequestDTO usuarioAtualizado)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idLogado);

            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            bool emailEmUso = _context.Usuarios.Any(u => u.Email.ToLower() == usuarioAtualizado.Email.ToLower() && u.IdUsuario != idLogado);

            if (emailEmUso)
            {
                throw new Exception("Este e-mail já está sendo utilizado por outra conta.");
            }

            usuario.Nome = usuarioAtualizado.Nome;
            usuario.Email = usuarioAtualizado.Email;

            if (!string.IsNullOrWhiteSpace(usuarioAtualizado.Senha))
            {
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuarioAtualizado.Senha);
            }

            _context.SaveChanges();
        }

        public void DeletarUsuario(int idLogado)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idLogado);

            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado!");
            }

            bool isAdminDeGrupoAtivo = _context.Grupos.Any(g => g.IdAdmin == idLogado && g.Ativo);

            if (isAdminDeGrupoAtivo)
            {
                throw new Exception("Não é possível excluir sua conta no momento. Você é o administrador de uma república ativa. Transfira a liderança.");
            }

            bool temDividasPendentes = _context.Parcelas.Any(p => p.IdUsuario == idLogado &&
                (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO || p.Status == StatusParcela.EM_ANALISE));

            if (temDividasPendentes)
            {
                throw new Exception("Você possui contas pendentes ou em análise. Quite todas as suas dívidas antes de excluir a conta.");
            }

            usuario.Ativo = false;
            _context.SaveChanges();
        }

        public void EsqueciSenha(EsqueciSenhaRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                return;
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
        }

        public void ValidarCodigo(ValidarCodigoRequestDTO request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                throw new Exception("Dados inválidos.");
            }

            var recuperacao = _context.CodigosRecuperacao
                .Where(c => c.IdUsuario == usuario.IdUsuario && !c.CodigoUsado)
                .OrderByDescending(c => c.DataExpiracao)
                .FirstOrDefault();

            if (recuperacao == null)
            {
                throw new Exception("Nenhum código ativo encontrado.");
            }

            if (recuperacao.Tentativas >= 3)
            {
                throw new Exception("Muitas tentativas falhas. Solicite um novo código.");
            }

            if (recuperacao.DataExpiracao < DateTime.UtcNow)
            {
                throw new Exception("Este código expirou.");
            }

            if (recuperacao.Codigo != request.Codigo)
            {
                recuperacao.Tentativas++;
                _context.SaveChanges();
                throw new Exception("Código incorreto.");
            }
        }

        public void ResetarSenha(ResetarSenhaRequestDTO request)
        {
            const int limiteDeTentativas = 3;

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

            if (usuario == null)
            {
                throw new Exception("Dados inválidos.");
            }

            var recuperacao = _context.CodigosRecuperacao
                .Where(c => c.IdUsuario == usuario.IdUsuario && !c.CodigoUsado)
                .OrderByDescending(c => c.DataExpiracao)
                .FirstOrDefault();

            if (recuperacao == null || recuperacao.DataExpiracao < DateTime.UtcNow || recuperacao.Tentativas >= limiteDeTentativas)
            {
                throw new Exception("Falha na validação final do código!");
            }

            if (recuperacao.Codigo != request.Codigo)
            {
                recuperacao.Tentativas++;
                _context.SaveChanges();
                throw new Exception("Código incorreto.");
            }

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
            recuperacao.CodigoUsado = true;
            _context.SaveChanges();
        }

        public ProximaContaResponseDTO? ObterProximaContaGeral(int idLogado)
        {
            return _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .ThenInclude(d => d.IdGrupoNavigation)
                .Where(p => p.IdUsuario == idLogado
                         && p.IdDespesaNavigation.Ativo == true
                         && p.IdDespesaNavigation.IdGrupoNavigation.Ativo == true
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
        }
    }
}