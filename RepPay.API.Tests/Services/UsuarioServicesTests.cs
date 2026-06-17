using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RepPay.API.Models;
using RepPay.API.DTOs;
using RepPay.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RepPay.API.Tests.Services
{
    public class UsuarioServiceTests
    {
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private IConfiguration CriarConfiguracaoMock()
        {
            var inMemorySettings = new Dictionary<string, string?> {
                {"Jwt:Key", "ChaveSecretaDeTesteSuperCumpridaParaO_JWT_NaoReclamar2026!!"}
            };
            return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        }

        // ==========================================
        // 1. TESTES DE CRIAÇÃO DE USUÁRIO
        // ==========================================

        [Fact]
        public void CriarUsuario_DeveSalvarUsuario_QuandoDadosForemValidos()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new UsuarioRequestDTO { Nome = "João", Email = "joao@ufal.com", Senha = "123" };

            service.CriarUsuario(request);

            var usuarioSalvo = context.Usuarios.FirstOrDefault(u => u.Email == "joao@ufal.com");
            Assert.NotNull(usuarioSalvo);
            Assert.True(BCrypt.Net.BCrypt.Verify("123", usuarioSalvo.Senha));
        }

        [Fact]
        public void CriarUsuario_DeveDispararExcecao_QuandoEmailJaExistirIgnorandoCaixaAlta()
        {
            var context = CriarContextoEmMemoria();
            context.Usuarios.Add(new Usuario { Nome = "Maria", Email = "maria@ufal.com", Senha = "123", Ativo = true });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new UsuarioRequestDTO { Nome = "Maria Nova", Email = "MARIA@UFAL.COM", Senha = "654" };

            var excecao = Assert.Throws<Exception>(() => service.CriarUsuario(request));
            Assert.Equal("Este e-mail já está cadastrado no sistema!", excecao.Message);
        }

        [Fact]
        public void CriarUsuario_DeveDispararExcecao_QuandoSenhaForNula()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new UsuarioRequestDTO { Nome = "João", Email = "joao@ufal.com", Senha = null! };

            var excecao = Assert.Throws<Exception>(() => service.CriarUsuario(request));
            Assert.Equal("A senha é obrigatória e não pode estar vazia!", excecao.Message);
        }

        // ==========================================
        // 2. TESTES DE LOGIN E RENOVAÇÃO
        // ==========================================

        [Fact]
        public void Login_DeveDispararExcecao_QuandoContaEstiverDesativada()
        {
            var context = CriarContextoEmMemoria();
            context.Usuarios.Add(new Usuario { Nome = "Inativo", Email = "inativo@ufal.com", Senha = BCrypt.Net.BCrypt.HashPassword("123"), Ativo = false });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new LoginRequestDTO { Email = "inativo@ufal.com", Senha = "123" };

            Assert.Throws<UnauthorizedAccessException>(() => service.Login(request));
        }

        [Fact]
        public void RenovacaoToken_DeveDispararExcecao_QuandoTokenExpiradoOuRevogado()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.RefreshTokens.Add(new RefreshToken { TokenHash = "token_expirado", IdUsuario = usuario.IdUsuario, DataExpiracao = DateTime.UtcNow.AddDays(-1), Revogado = false });
            context.RefreshTokens.Add(new RefreshToken { TokenHash = "token_revogado", IdUsuario = usuario.IdUsuario, DataExpiracao = DateTime.UtcNow.AddDays(1), Revogado = true });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            Assert.Throws<UnauthorizedAccessException>(() => service.RenovacaoToken(new RefreshTokenRequestDTO { RefreshToken = "token_expirado" }));
            Assert.Throws<UnauthorizedAccessException>(() => service.RenovacaoToken(new RefreshTokenRequestDTO { RefreshToken = "token_revogado" }));
        }

        [Fact]
        public void Login_DeveDispararExcecao_QuandoEmailNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new LoginRequestDTO { Email = "fantasma@ufal.com", Senha = "qualquer" };

            Assert.Throws<UnauthorizedAccessException>(() => service.Login(request));
        }

        [Fact]
        public void Login_DeveDispararExcecao_QuandoSenhaEstiverErrada()
        {
            var context = CriarContextoEmMemoria();
            context.Usuarios.Add(new Usuario { Nome = "Carlos", Email = "carlos@ufal.com", Senha = BCrypt.Net.BCrypt.HashPassword("senhaCorreta"), Ativo = true });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new LoginRequestDTO { Email = "carlos@ufal.com", Senha = "senhaErrada" };

            Assert.Throws<UnauthorizedAccessException>(() => service.Login(request));
        }

        [Fact]
        public void Login_DeveRetornarTokens_QuandoCredenciaisForemValidas()
        {
            var context = CriarContextoEmMemoria();
            context.Usuarios.Add(new Usuario { Nome = "Carlos", Email = "carlos@ufal.com", Senha = BCrypt.Net.BCrypt.HashPassword("senhaCorreta"), Ativo = true });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new LoginRequestDTO { Email = "carlos@ufal.com", Senha = "senhaCorreta" };

            var resultado = service.Login(request);

            Assert.NotNull(resultado.Token);
            Assert.NotNull(resultado.RefreshToken);
            Assert.Equal("carlos@ufal.com", context.Usuarios.First().Email);
            Assert.True(context.RefreshTokens.Any());
        }

        [Fact]
        public void RenovacaoToken_DeveDispararExcecao_QuandoTokenNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<UnauthorizedAccessException>(() =>
                service.RenovacaoToken(new RefreshTokenRequestDTO { RefreshToken = "token_inexistente" }));

            Assert.Contains("inválido ou inexistente", excecao.Message);
        }

        [Fact]
        public void RenovacaoToken_DeveDispararExcecao_QuandoUsuarioVinculadoEstiverInativo()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Inativo", Email = "inativo@ufal.com", Senha = "123", Ativo = false };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = "token_valido_usuario_inativo",
                IdUsuario = usuario.IdUsuario,
                DataExpiracao = DateTime.UtcNow.AddDays(7),
                Revogado = false
            });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<UnauthorizedAccessException>(() =>
                service.RenovacaoToken(new RefreshTokenRequestDTO { RefreshToken = "token_valido_usuario_inativo" }));

            Assert.Contains("conta vinculada", excecao.Message);
        }

        [Fact]
        public void LogOut_DeveRevogarToken_QuandoTokenExistir()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = "token_ativo",
                IdUsuario = usuario.IdUsuario,
                DataExpiracao = DateTime.UtcNow.AddDays(7),
                Revogado = false
            });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            service.LogOut(new RefreshTokenRequestDTO { RefreshToken = "token_ativo" });

            Assert.True(context.RefreshTokens.First().Revogado);
        }

        [Fact]
        public void LogOut_NaoDeveDispararExcecao_QuandoTokenNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var exception = Record.Exception(() =>
                service.LogOut(new RefreshTokenRequestDTO { RefreshToken = "token_fantasma" }));

            Assert.Null(exception);
        }

        // ==========================================
        // 3. TESTES DE ATUALIZAÇÃO E PERFIL
        // ==========================================

        [Fact]
        public void AtualizarUsuario_DeveAtualizarApenasNomeEEmail_QuandoSenhaForVazia()
        {
            var context = CriarContextoEmMemoria();
            var senhaAntigaHash = BCrypt.Net.BCrypt.HashPassword("senhaVelha");
            var usuario = new Usuario { Nome = "Antigo", Email = "antigo@ufal.com", Senha = senhaAntigaHash, Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new UsuarioRequestDTO { Nome = "Novo Nome", Email = "novo@ufal.com", Senha = "" };

            service.AtualizarUsuario(usuario.IdUsuario, request);

            var usuarioAtualizado = context.Usuarios.First();
            Assert.Equal("Novo Nome", usuarioAtualizado.Nome);
            Assert.Equal("novo@ufal.com", usuarioAtualizado.Email);
            Assert.Equal(senhaAntigaHash, usuarioAtualizado.Senha);
        }

        [Fact]
        public void AtualizarUsuario_DeveDispararExcecao_QuandoTentarUsarEmailDeOutraPessoa()
        {
            var context = CriarContextoEmMemoria();
            context.Usuarios.Add(new Usuario { Nome = "User 1", Email = "user1@ufal.com", Senha = "123", Ativo = true });
            var usuarioLogado = new Usuario { Nome = "User 2", Email = "user2@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuarioLogado);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new UsuarioRequestDTO { Nome = "User 2", Email = "user1@ufal.com", Senha = "" };

            var excecao = Assert.Throws<Exception>(() => service.AtualizarUsuario(usuarioLogado.IdUsuario, request));
            Assert.Equal("Este e-mail já está sendo utilizado por outra conta.", excecao.Message);
        }

        [Fact]
        public void AtualizarUsuario_DeveDispararExcecao_QuandoUsuarioNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new UsuarioRequestDTO { Nome = "Fantasma", Email = "fantasma@ufal.com", Senha = "" };

            var excecao = Assert.Throws<Exception>(() => service.AtualizarUsuario(9999, request));
            Assert.Equal("Usuário não encontrado.", excecao.Message);
        }

        [Fact]
        public void AtualizarUsuario_DeveAtualizarSenha_QuandoNovaSenhaForFornecida()
        {
            var context = CriarContextoEmMemoria();
            var senhaAntigaHash = BCrypt.Net.BCrypt.HashPassword("senhaVelha");
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = senhaAntigaHash, Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new UsuarioRequestDTO { Nome = "Teste", Email = "teste@ufal.com", Senha = "senhaNova" };

            service.AtualizarUsuario(usuario.IdUsuario, request);

            var usuarioAtualizado = context.Usuarios.First();
            Assert.NotEqual(senhaAntigaHash, usuarioAtualizado.Senha);
            Assert.True(BCrypt.Net.BCrypt.Verify("senhaNova", usuarioAtualizado.Senha));
        }

        [Fact]
        public void GetMeuPerfil_DeveRetornarDadosCorretos_QuandoUsuarioExistir()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Ana", Email = "ana@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var perfil = service.GetMeuPerfil(usuario.IdUsuario);

            Assert.Equal("Ana", perfil.Nome);
            Assert.Equal("ana@ufal.com", perfil.Email);
        }

        [Fact]
        public void GetMeuPerfil_DeveDispararExcecao_QuandoUsuarioNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<Exception>(() => service.GetMeuPerfil(9999));
            Assert.Equal("Usuário não encontrado.", excecao.Message);
        }

        // ==========================================
        // 4. TESTES DE EXCLUSÃO (AS REGRAS DE NEGÓCIO MAIS PESADAS)
        // ==========================================

        [Fact]
        public void DeletarUsuario_DeveDispararExcecao_QuandoForAdminDeGrupoAtivo()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            context.Grupos.Add(new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<Exception>(() => service.DeletarUsuario(admin.IdUsuario));
            Assert.Contains("administrador de uma república ativa", excecao.Message);
        }

        [Fact]
        public void DeletarUsuario_DeveDispararExcecao_QuandoTiverDividasPendentes()
        {
            var context = CriarContextoEmMemoria();
            var caloteiro = new Usuario { Nome = "Caloteiro", Email = "caloteiro@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(caloteiro);
            context.SaveChanges();

            context.Parcelas.Add(new Parcela { IdUsuario = caloteiro.IdUsuario, Valor = 100, Status = StatusParcela.PENDENTE });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<Exception>(() => service.DeletarUsuario(caloteiro.IdUsuario));
            Assert.Contains("Quite todas as suas dívidas antes de excluir a conta", excecao.Message);
        }

        [Fact]
        public void DeletarUsuario_DeveFazerSoftDelete_QuandoRegrasForemRespeitadas()
        {
            var context = CriarContextoEmMemoria();
            var usuarioLimpo = new Usuario { Nome = "Limpo", Email = "limpo@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuarioLimpo);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            service.DeletarUsuario(usuarioLimpo.IdUsuario);

            var usuarioDeletado = context.Usuarios.First();
            Assert.False(usuarioDeletado.Ativo);
        }

        [Fact]
        public void DeletarUsuario_DeveDispararExcecao_QuandoTiverParcelaAtrasada()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Atrasado", Email = "atrasado@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.Parcelas.Add(new Parcela { IdUsuario = usuario.IdUsuario, Valor = 50, Status = StatusParcela.ATRASADO });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<Exception>(() => service.DeletarUsuario(usuario.IdUsuario));
            Assert.Contains("Quite todas as suas dívidas antes de excluir a conta", excecao.Message);
        }

        [Fact]
        public void DeletarUsuario_DeveDispararExcecao_QuandoTiverParcelaEmAnalise()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "EmAnalise", Email = "emanalise@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.Parcelas.Add(new Parcela { IdUsuario = usuario.IdUsuario, Valor = 75, Status = StatusParcela.EM_ANALISE });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<Exception>(() => service.DeletarUsuario(usuario.IdUsuario));
            Assert.Contains("Quite todas as suas dívidas antes de excluir a conta", excecao.Message);
        }

        [Fact]
        public void DeletarUsuario_NaoDeveBloquear_QuandoGrupoDoAdminEstiverInativo()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            context.Grupos.Add(new Grupo { Nome = "República Encerrada", IdAdmin = admin.IdUsuario, Ativo = false, CodigoAcesso = "INATIVO1" });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            service.DeletarUsuario(admin.IdUsuario);

            Assert.False(context.Usuarios.First().Ativo);
        }

        // ==========================================
        // 5. TESTES DE RECUPERAÇÃO DE SENHA
        // ==========================================

        [Fact]
        public void ValidarCodigo_DeveBloquear_QuandoExceder3Tentativas()
        {
            var context = CriarContextoEmMemoria();
            var esquecido = new Usuario { Nome = "Esquecido", Email = "esqueci@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(esquecido);
            context.SaveChanges();

            context.CodigosRecuperacao.Add(new CodigoRecuperacao
            {
                IdUsuario = esquecido.IdUsuario,
                Codigo = "123456",
                Tentativas = 3,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10),
                CodigoUsado = false
            });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new ValidarCodigoRequestDTO { Email = "esqueci@ufal.com", Codigo = "654321" };

            var excecao = Assert.Throws<Exception>(() => service.ValidarCodigo(request));
            Assert.Equal("Muitas tentativas falhas. Solicite um novo código.", excecao.Message);
        }

        [Fact]
        public void ValidarCodigo_DeveFalharEAumentarTentativa_QuandoCodigoForIncorreto()
        {
            var context = CriarContextoEmMemoria();
            var esquecido = new Usuario { Nome = "Esquecido", Email = "esqueci@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(esquecido);
            context.SaveChanges();

            var codigoReal = new CodigoRecuperacao
            {
                IdUsuario = esquecido.IdUsuario,
                Codigo = "111111",
                Tentativas = 0,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10),
                CodigoUsado = false
            };
            context.CodigosRecuperacao.Add(codigoReal);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new ValidarCodigoRequestDTO { Email = "esqueci@ufal.com", Codigo = "999999" };

            var excecao = Assert.Throws<Exception>(() => service.ValidarCodigo(request));
            Assert.Equal("Código incorreto.", excecao.Message);
            Assert.Equal(1, codigoReal.Tentativas);
        }

        [Fact]
        public void ValidarCodigo_DevePassar_QuandoCodigoEstiverCorreto()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.CodigosRecuperacao.Add(new CodigoRecuperacao
            {
                IdUsuario = usuario.IdUsuario,
                Codigo = "123456",
                Tentativas = 0,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10),
                CodigoUsado = false
            });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new ValidarCodigoRequestDTO { Email = "teste@ufal.com", Codigo = "123456" };

            var exception = Record.Exception(() => service.ValidarCodigo(request));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidarCodigo_DeveDispararExcecao_QuandoCodigoEstiverExpirado()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.CodigosRecuperacao.Add(new CodigoRecuperacao
            {
                IdUsuario = usuario.IdUsuario,
                Codigo = "123456",
                Tentativas = 0,
                DataExpiracao = DateTime.UtcNow.AddMinutes(-1),
                CodigoUsado = false
            });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new ValidarCodigoRequestDTO { Email = "teste@ufal.com", Codigo = "123456" };

            var excecao = Assert.Throws<Exception>(() => service.ValidarCodigo(request));
            Assert.Equal("Este código expirou.", excecao.Message);
        }

        [Fact]
        public void ValidarCodigo_DeveDispararExcecao_QuandoEmailNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());
            var request = new ValidarCodigoRequestDTO { Email = "naoexiste@ufal.com", Codigo = "123456" };

            var excecao = Assert.Throws<Exception>(() => service.ValidarCodigo(request));
            Assert.Equal("Dados inválidos.", excecao.Message);
        }

        [Fact]
        public void EsqueciSenha_NaoDeveDispararExcecao_QuandoEmailNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var exception = Record.Exception(() =>
                service.EsqueciSenha(new EsqueciSenhaRequestDTO { Email = "naoexiste@ufal.com" }));

            Assert.Null(exception);
        }

        [Fact]
        public void EsqueciSenha_DeveCriarCodigoNobanco_QuandoEmailExistir()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            service.EsqueciSenha(new EsqueciSenhaRequestDTO { Email = "teste@ufal.com" });

            Assert.True(context.CodigosRecuperacao.Any(c => c.IdUsuario == usuario.IdUsuario));
        }

        [Fact]
        public void ResetarSenha_DeveAlterarSenha_QuandoCodigoForValido()
        {
            var context = CriarContextoEmMemoria();
            var senhaAntiga = BCrypt.Net.BCrypt.HashPassword("senhaVelha");
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = senhaAntiga, Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.CodigosRecuperacao.Add(new CodigoRecuperacao
            {
                IdUsuario = usuario.IdUsuario,
                Codigo = "654321",
                Tentativas = 0,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10),
                CodigoUsado = false
            });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());
            service.ResetarSenha(new ResetarSenhaRequestDTO { Email = "teste@ufal.com", Codigo = "654321", NovaSenha = "senhaNova" });

            var usuarioAtualizado = context.Usuarios.First();
            Assert.True(BCrypt.Net.BCrypt.Verify("senhaNova", usuarioAtualizado.Senha));
            Assert.True(context.CodigosRecuperacao.First().CodigoUsado);
        }

        [Fact]
        public void ResetarSenha_DeveDispararExcecao_QuandoCodigoEstiverExpirado()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.CodigosRecuperacao.Add(new CodigoRecuperacao
            {
                IdUsuario = usuario.IdUsuario,
                Codigo = "111111",
                Tentativas = 0,
                DataExpiracao = DateTime.UtcNow.AddMinutes(-5),
                CodigoUsado = false
            });
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<Exception>(() =>
                service.ResetarSenha(new ResetarSenhaRequestDTO { Email = "teste@ufal.com", Codigo = "111111", NovaSenha = "nova" }));

            Assert.Contains("Falha na validação final", excecao.Message);
        }

        [Fact]
        public void ResetarSenha_DeveDispararExcecao_QuandoCodigoEstiverErrado()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            var codigo = new CodigoRecuperacao
            {
                IdUsuario = usuario.IdUsuario,
                Codigo = "111111",
                Tentativas = 0,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10),
                CodigoUsado = false
            };
            context.CodigosRecuperacao.Add(codigo);
            context.SaveChanges();

            var service = new UsuarioService(context, CriarConfiguracaoMock());

            var excecao = Assert.Throws<Exception>(() =>
                service.ResetarSenha(new ResetarSenhaRequestDTO { Email = "teste@ufal.com", Codigo = "999999", NovaSenha = "nova" }));

            Assert.Equal("Código incorreto.", excecao.Message);
            Assert.Equal(1, codigo.Tentativas);
        }
    }
}
