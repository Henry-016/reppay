using Xunit;
using Microsoft.EntityFrameworkCore;
using RepPay.API.Models;
using RepPay.API.DTOs;
using RepPay.API.Services;
using System;
using System.Linq;

namespace RepPay.API.Tests.Services
{
    public class GrupoServiceTests
    {
        // ==========================================
        // CONFIGURAÇÕES BASE
        // ==========================================
        private AppDbContext CriarContextoEmMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // ==========================================
        // 1. TESTES DE CRIAÇÃO DE GRUPO
        // ==========================================

        [Fact]
        public void CriarGrupo_DeveRetornarCodigoAcesso_QuandoDadosForemValidos()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            var service = new GrupoService(context);
            var request = new GrupoRequestDTO { Nome = "República das Flores" };

            var resultado = service.CriarGrupo(admin.IdUsuario, request);

            Assert.NotNull(resultado.CodigoAcesso);
            Assert.Equal(8, resultado.CodigoAcesso.Length);
            Assert.True(context.Grupos.Any(g => g.Nome == "República das Flores" && g.IdAdmin == admin.IdUsuario));
            Assert.True(context.Pertences.Any(p => p.IdUsuario == admin.IdUsuario && p.IdGrupo == context.Grupos.First().IdGrupo));
        }

        // ==========================================
        // 2. TESTES DE ENTRADA NO GRUPO
        // ==========================================

        [Fact]
        public void EntrarNoGrupo_DeveDispararExcecao_QuandoUsuarioJaPertencerAoGrupo()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "João", Email = "joao@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            var grupo = new Grupo { Nome = "República Central", CodigoAcesso = "ABCDEFGH", IdAdmin = 99, Ativo = true };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdUsuario = usuario.IdUsuario, IdGrupo = grupo.IdGrupo });
            context.SaveChanges();

            var service = new GrupoService(context);
            var request = new EntrarGrupoRequestDTO { CodigoAcesso = "ABCDEFGH" };

            var excecao = Assert.Throws<Exception>(() => service.EntrarNoGrupo(usuario.IdUsuario, request));
            Assert.Equal("Você já faz parte desta república!", excecao.Message);
        }

        [Fact]
        public void EntrarNoGrupo_DeveVincularUsuario_QuandoCodigoForValido()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Novo Morador", Email = "novo@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            var grupo = new Grupo { Nome = "República TI", CodigoAcesso = "DEV12345", IdAdmin = 99, Ativo = true };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);
            var request = new EntrarGrupoRequestDTO { CodigoAcesso = "DEV12345" };

            var mensagem = service.EntrarNoGrupo(usuario.IdUsuario, request);

            Assert.Contains("Bem-vindo(a)", mensagem);
            Assert.True(context.Pertences.Any(p => p.IdUsuario == usuario.IdUsuario && p.IdGrupo == grupo.IdGrupo));
        }

        [Fact]
        public void EntrarNoGrupo_DeveDispararExcecao_QuandoCodigoForInvalido()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            var service = new GrupoService(context);
            var request = new EntrarGrupoRequestDTO { CodigoAcesso = "INVALIDO" };

            Assert.Throws<KeyNotFoundException>(() => service.EntrarNoGrupo(usuario.IdUsuario, request));
        }

        [Fact]
        public void EntrarNoGrupo_DeveDispararExcecao_QuandoGrupoEstiverInativo()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Teste", Email = "teste@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            var grupo = new Grupo { Nome = "República Encerrada", CodigoAcesso = "INATIVO1", IdAdmin = 99, Ativo = false };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);
            var request = new EntrarGrupoRequestDTO { CodigoAcesso = "INATIVO1" };

            Assert.Throws<KeyNotFoundException>(() => service.EntrarNoGrupo(usuario.IdUsuario, request));
        }

        // ==========================================
        // 3. TESTES DE SAÍDA E EXPULSÃO (REGRAS FINANCEIRAS)
        // ==========================================

        [Fact]
        public void SairDoGrupo_DeveDispararExcecao_QuandoUsuarioTiverDividas()
        {
            var context = CriarContextoEmMemoria();
            var grupo = new Grupo { Nome = "República", IdAdmin = 99, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            var morador = new Usuario { Nome = "Devedor", Email = "devedor@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(morador);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador.IdUsuario });
            var despesa = new Despesa { Nome = "Luz", IdGrupo = grupo.IdGrupo, Valor = 100, Vencimento = DateOnly.FromDateTime(DateTime.UtcNow), Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            context.Parcelas.Add(new Parcela { IdUsuario = morador.IdUsuario, IdDespesa = despesa.IdDespesa, Valor = 100, Status = StatusParcela.PENDENTE });
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<Exception>(() => service.SairDoGrupo(morador.IdUsuario, grupo.IdGrupo));
            Assert.Contains("Você possui dívidas pendentes ou em análise", excecao.Message);
        }

        [Fact]
        public void ExpulsarMorador_DeveDispararExcecao_QuandoQuemPedeNaoEAdmin()
        {
            var context = CriarContextoEmMemoria();

            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var moradorComum = new Usuario { Nome = "Comum", Email = "comum@ufal.com", Senha = "123", Ativo = true };
            var alvo = new Usuario { Nome = "Alvo", Email = "alvo@ufal.com", Senha = "123", Ativo = true };

            context.Usuarios.AddRange(admin, moradorComum, alvo);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = alvo.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<UnauthorizedAccessException>(() =>
                service.ExpulsarMorador(moradorComum.IdUsuario, grupo.IdGrupo, alvo.IdUsuario));

            Assert.Contains("Apenas o administrador pode expulsar", excecao.Message);
        }

        [Fact]
        public void ExpulsarMorador_DeveDispararExcecao_QuandoAdminTentaSeExpulsar()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<Exception>(() => service.ExpulsarMorador(admin.IdUsuario, grupo.IdGrupo, admin.IdUsuario));
            Assert.Contains("Você não pode expulsar a si mesmo", excecao.Message);
        }

        [Fact]
        public void SairDoGrupo_DeveDispararExcecao_QuandoAdminTentarSair()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = admin.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<Exception>(() => service.SairDoGrupo(admin.IdUsuario, grupo.IdGrupo));
            Assert.Contains("Você é o administrador do grupo", excecao.Message);
        }

        [Fact]
        public void SairDoGrupo_DeveDispararExcecao_QuandoUsuarioNaoPertencerAoGrupo()
        {
            var context = CriarContextoEmMemoria();
            var usuario = new Usuario { Nome = "Forasteiro", Email = "fora@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(usuario);
            var grupo = new Grupo { Nome = "República", IdAdmin = 99, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);

            Assert.Throws<KeyNotFoundException>(() => service.SairDoGrupo(usuario.IdUsuario, grupo.IdGrupo));
        }

        [Fact]
        public void SairDoGrupo_DeveRemoverVinculo_QuandoMoradorNaoPossuirDividas()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Morador", Email = "morador@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);
            var mensagem = service.SairDoGrupo(morador.IdUsuario, grupo.IdGrupo);

            Assert.Contains("saiu da república com sucesso", mensagem);
            Assert.False(context.Pertences.Any(p => p.IdUsuario == morador.IdUsuario && p.IdGrupo == grupo.IdGrupo));
        }

        [Fact]
        public void ExpulsarMorador_DeveDispararExcecao_QuandoMoradorTiverDividas()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Devedor", Email = "devedor@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador.IdUsuario });
            var despesa = new Despesa { Nome = "Água", IdGrupo = grupo.IdGrupo, Valor = 60, Vencimento = DateOnly.FromDateTime(DateTime.UtcNow), Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            context.Parcelas.Add(new Parcela { IdUsuario = morador.IdUsuario, IdDespesa = despesa.IdDespesa, Valor = 60, Status = StatusParcela.ATRASADO });
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<Exception>(() => service.ExpulsarMorador(admin.IdUsuario, grupo.IdGrupo, morador.IdUsuario));
            Assert.Contains("possui dívidas ativas", excecao.Message);
        }

        [Fact]
        public void ExpulsarMorador_DeveRemoverVinculo_QuandoMoradorNaoPossuirDividas()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Limpo", Email = "limpo@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);
            var mensagem = service.ExpulsarMorador(admin.IdUsuario, grupo.IdGrupo, morador.IdUsuario);

            Assert.Contains("removido da república com sucesso", mensagem);
            Assert.False(context.Pertences.Any(p => p.IdUsuario == morador.IdUsuario && p.IdGrupo == grupo.IdGrupo));
        }

        [Fact]
        public void ExpulsarMorador_DeveDispararExcecao_QuandoMoradorNaoPertencerAoGrupo()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var forasteiro = new Usuario { Nome = "Fora", Email = "fora@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, forasteiro);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);

            Assert.Throws<KeyNotFoundException>(() => service.ExpulsarMorador(admin.IdUsuario, grupo.IdGrupo, forasteiro.IdUsuario));
        }

        // ==========================================
        // 4. TESTES DE GESTÃO DO GRUPO
        // ==========================================

        [Fact]
        public void TransferirAdmin_DeveAtualizarLideranca_QuandoMoradorForValido()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var novoLider = new Usuario { Nome = "Novo Lider", Email = "lider@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, novoLider);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = novoLider.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);
            var mensagem = service.TransferirAdmin(admin.IdUsuario, grupo.IdGrupo, novoLider.IdUsuario);

            Assert.Contains("Liderança transferida com sucesso", mensagem);
            Assert.Equal(novoLider.IdUsuario, context.Grupos.First().IdAdmin);
        }

        [Fact]
        public void DeletarGrupo_DeveDispararExcecao_QuandoHouverOutrosMoradores()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var outroMorador = new Usuario { Nome = "Morador", Email = "morador@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, outroMorador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = admin.IdUsuario });
            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = outroMorador.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<Exception>(() => service.DeletarGrupo(admin.IdUsuario, grupo.IdGrupo));
            Assert.Contains("Não é possível encerrar a república enquanto houver outros moradores", excecao.Message);
        }

        [Fact]
        public void DeletarGrupo_DeveFazerSoftDelete_QuandoAdminForOUnicoMorador()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = admin.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);
            service.DeletarGrupo(admin.IdUsuario, grupo.IdGrupo);

            var grupoDeletado = context.Grupos.First();
            Assert.False(grupoDeletado.Ativo);
        }

        [Fact]
        public void TransferirAdmin_DeveDispararExcecao_QuandoAdminTentarTransferirParaSiMesmo()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<Exception>(() => service.TransferirAdmin(admin.IdUsuario, grupo.IdGrupo, admin.IdUsuario));
            Assert.Contains("Você já é o administrador", excecao.Message);
        }

        [Fact]
        public void TransferirAdmin_DeveDispararExcecao_QuandoDestinoTiverContaInativa()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var inativo = new Usuario { Nome = "Inativo", Email = "inativo@ufal.com", Senha = "123", Ativo = false };
            context.Usuarios.AddRange(admin, inativo);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = inativo.IdUsuario });
            context.SaveChanges();

            var service = new GrupoService(context);

            var excecao = Assert.Throws<Exception>(() => service.TransferirAdmin(admin.IdUsuario, grupo.IdGrupo, inativo.IdUsuario));
            Assert.Contains("conta desativada", excecao.Message);
        }

        [Fact]
        public void TransferirAdmin_DeveDispararExcecao_QuandoQuemPedeNaoForAdmin()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var moradorComum = new Usuario { Nome = "Comum", Email = "comum@ufal.com", Senha = "123", Ativo = true };
            var alvo = new Usuario { Nome = "Alvo", Email = "alvo@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, moradorComum, alvo);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.AddRange(
                new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = moradorComum.IdUsuario },
                new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = alvo.IdUsuario }
            );
            context.SaveChanges();

            var service = new GrupoService(context);

            Assert.Throws<UnauthorizedAccessException>(() =>
                service.TransferirAdmin(moradorComum.IdUsuario, grupo.IdGrupo, alvo.IdUsuario));
        }

        [Fact]
        public void TransferirAdmin_DeveDispararExcecao_QuandoDestinoNaoPertencerAoGrupo()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var forasteiro = new Usuario { Nome = "Fora", Email = "fora@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, forasteiro);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);

            Assert.Throws<KeyNotFoundException>(() =>
                service.TransferirAdmin(admin.IdUsuario, grupo.IdGrupo, forasteiro.IdUsuario));
        }

        [Fact]
        public void DeletarGrupo_DeveDispararExcecao_QuandoQuemPedeNaoForAdmin()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var moradorComum = new Usuario { Nome = "Comum", Email = "comum@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, moradorComum);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new GrupoService(context);

            Assert.Throws<UnauthorizedAccessException>(() => service.DeletarGrupo(moradorComum.IdUsuario, grupo.IdGrupo));
        }

        [Fact]
        public void DeletarGrupo_DeveDispararExcecao_QuandoGrupoNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new GrupoService(context);

            Assert.Throws<KeyNotFoundException>(() => service.DeletarGrupo(1, 9999));
        }
    }
}
