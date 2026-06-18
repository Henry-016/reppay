using Xunit;
using Microsoft.EntityFrameworkCore;
using RepPay.API.Models;
using RepPay.API.DTOs;
using RepPay.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RepPay.API.Tests.Services
{
    public class DespesaServiceTests
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

        private (AppDbContext context, Usuario admin, Usuario morador, Grupo grupo, Despesa despesa) CriarCenarioCompleto(
            StatusParcela statusParcela = StatusParcela.PENDENTE)
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Morador", Email = "morador@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.AddRange(
                new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = admin.IdUsuario },
                new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador.IdUsuario }
            );

            var despesa = new Despesa { Nome = "Conta", IdGrupo = grupo.IdGrupo, Valor = 100, Vencimento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            context.Parcelas.Add(new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = morador.IdUsuario, Valor = 100, Status = statusParcela });
            context.SaveChanges();

            return (context, admin, morador, grupo, despesa);
        }

        // ==========================================
        // 1. TESTES DE LANÇAMENTO E RATEIO (CÁLCULO MATEMÁTICO)
        // ==========================================

        [Fact]
        public void CadastrarDespesa_DeveGerarRateioCorreto_QuandoDadosForemValidos()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador1 = new Usuario { Nome = "Morador 1", Email = "m1@ufal.com", Senha = "123", Ativo = true };
            var morador2 = new Usuario { Nome = "Morador 2", Email = "m2@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador1, morador2);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = admin.IdUsuario });
            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador1.IdUsuario });
            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador2.IdUsuario });
            context.SaveChanges();

            var service = new DespesaService(context);
            var request = new DespesaRequestDTO
            {
                Nome = "Conta de Luz",
                Valor = 300.00m,
                Vencimento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                IdGrupo = grupo.IdGrupo,
                MoradoresIds = new List<int> { admin.IdUsuario, morador1.IdUsuario, morador2.IdUsuario }
            };

            var mensagem = service.CadastrarDespesa(admin.IdUsuario, request);

            Assert.Contains("rateio gerado com sucesso", mensagem);

            var despesaSalva = context.Despesas.Include(d => d.Parcelas).First();
            Assert.Equal(3, despesaSalva.Parcelas.Count);

            Assert.All(despesaSalva.Parcelas, parcela => Assert.Equal(100.00m, parcela.Valor));
            Assert.All(despesaSalva.Parcelas, parcela => Assert.Equal(StatusParcela.PENDENTE, parcela.Status));
        }

        [Fact]
        public void CadastrarDespesa_DeveDispararExcecao_QuandoNaoAdminTentarLancar()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Comum", Email = "comum@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            context.Pertences.Add(new Pertence { IdGrupo = grupo.IdGrupo, IdUsuario = morador.IdUsuario });
            context.SaveChanges();

            var service = new DespesaService(context);
            var request = new DespesaRequestDTO { Nome = "Conta Falsa", Valor = 50, IdGrupo = grupo.IdGrupo, MoradoresIds = new List<int> { morador.IdUsuario } };

            var excecao = Assert.Throws<UnauthorizedAccessException>(() => service.CadastrarDespesa(morador.IdUsuario, request));
            Assert.Contains("Apenas o administrador pode lançar despesas", excecao.Message);
        }



        [Fact]
        public void CadastrarDespesa_DeveDispararExcecao_QuandoListaDeMoradoresForVazia()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new DespesaService(context);
            var request = new DespesaRequestDTO
            {
                Nome = "Conta",
                Valor = 100,
                IdGrupo = grupo.IdGrupo,
                MoradoresIds = new List<int>()
            };

            var excecao = Assert.Throws<Exception>(() => service.CadastrarDespesa(admin.IdUsuario, request));
            Assert.Contains("pelo menos um morador", excecao.Message);
        }

        [Fact]
        public void CadastrarDespesa_DeveDispararExcecao_QuandoMoradorNaoPertencerAoGrupo()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var forasteiro = new Usuario { Nome = "Forasteiro", Email = "fora@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, forasteiro);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new DespesaService(context);
            var request = new DespesaRequestDTO
            {
                Nome = "Conta",
                Valor = 100,
                IdGrupo = grupo.IdGrupo,
                MoradoresIds = new List<int> { forasteiro.IdUsuario }
            };

            var excecao = Assert.Throws<Exception>(() => service.CadastrarDespesa(admin.IdUsuario, request));
            Assert.Contains("não pertencem a esta república", excecao.Message);
        }

        [Fact]
        public void CadastrarDespesa_DeveDispararExcecao_QuandoGrupoEstiverInativo()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(admin);
            context.SaveChanges();

            var grupo = new Grupo
            {
                Nome = "República Encerrada",
                IdAdmin = admin.IdUsuario,
                Ativo = false,
                CodigoAcesso = "12345678"
            };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var service = new DespesaService(context);
            var request = new DespesaRequestDTO
            {
                Nome = "Conta",
                Valor = 100,
                Vencimento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                IdGrupo = grupo.IdGrupo,
                MoradoresIds = new List<int> { admin.IdUsuario }
            };

            var excecao = Assert.Throws<Exception>(() => service.CadastrarDespesa(admin.IdUsuario, request));
            Assert.Contains("república encerrada", excecao.Message);
        }

        // ==========================================
        // 2. TESTES DE SEGURANÇA EM PAGAMENTOS
        // ==========================================

        [Fact]
        public void PagarParcela_DeveColocarEmAnalise_QuandoDonoDaDividaAcionar()
        {
            var context = CriarContextoEmMemoria();
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(morador);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Internet", Valor = 100, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = morador.IdUsuario, Valor = 100, Status = StatusParcela.PENDENTE };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);
            var mensagem = service.PagarParcela(morador.IdUsuario, parcela.IdParcela);

            Assert.Contains("Pagamento sinalizado", mensagem);
            Assert.Equal(StatusParcela.EM_ANALISE, parcela.Status);
            Assert.NotNull(parcela.DataPagamento);
        }

        [Fact]
        public void PagarParcela_DeveDispararExcecao_QuandoTentarSinalizarDividaDeOutro()
        {
            var context = CriarContextoEmMemoria();
            var alvo = new Usuario { Nome = "Alvo", Email = "alvo@ufal.com", Senha = "123", Ativo = true };
            var invasor = new Usuario { Nome = "Invasor", Email = "invasor@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(alvo, invasor);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Internet", Valor = 100, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = alvo.IdUsuario, Valor = 100, Status = StatusParcela.PENDENTE };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);

            var excecao = Assert.Throws<UnauthorizedAccessException>(() => service.PagarParcela(invasor.IdUsuario, parcela.IdParcela));
            Assert.Contains("Não tem permissão para alterar uma dívida que não lhe pertence", excecao.Message);
        }

        [Fact]
        public void PagarParcela_DeveDispararExcecao_QuandoParcelaJaEstiverPaga()
        {
            var context = CriarContextoEmMemoria();
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(morador);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Gás", Valor = 50, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = morador.IdUsuario, Valor = 50, Status = StatusParcela.PAGO };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);

            var excecao = Assert.Throws<Exception>(() => service.PagarParcela(morador.IdUsuario, parcela.IdParcela));
            Assert.Contains("já se encontra paga", excecao.Message);
        }

        [Fact]
        public void PagarParcela_DeveDispararExcecao_QuandoParcelaNaoExistir()
        {
            var context = CriarContextoEmMemoria();
            var service = new DespesaService(context);

            Assert.Throws<KeyNotFoundException>(() => service.PagarParcela(1, 9999));
        }

        [Fact]
        public void PagarParcela_DeveDispararExcecao_QuandoParcelaJaEstiverEmAnalise()
        {
            var context = CriarContextoEmMemoria();
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(morador);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Internet", Valor = 100, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela
            {
                IdDespesa = despesa.IdDespesa,
                IdUsuario = morador.IdUsuario,
                Valor = 100,
                Status = StatusParcela.EM_ANALISE,
                DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
            };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var dataOriginal = parcela.DataPagamento;
            var service = new DespesaService(context);

            var excecao = Assert.Throws<Exception>(() => service.PagarParcela(morador.IdUsuario, parcela.IdParcela));
            Assert.Contains("já foi sinalizado", excecao.Message);
            Assert.Equal(dataOriginal, parcela.DataPagamento);
        }

        // ==========================================
        // 3. TESTES DE DESFAZER PAGAMENTO
        // ==========================================

        [Fact]
        public void DesfazerPagamento_DeveVoltarParaPendente_QuandoParcelaEstiverEmAnalise()
        {
            var context = CriarContextoEmMemoria();
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(morador);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Internet", Valor = 100, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela
            {
                IdDespesa = despesa.IdDespesa,
                IdUsuario = morador.IdUsuario,
                Valor = 100,
                Status = StatusParcela.EM_ANALISE,
                DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);
            var mensagem = service.DesfazerPagamento(morador.IdUsuario, parcela.IdParcela);

            Assert.Contains("desfeita com sucesso", mensagem);
            Assert.Equal(StatusParcela.PENDENTE, parcela.Status);
            Assert.Null(parcela.DataPagamento);
        }

        [Fact]
        public void DesfazerPagamento_DeveDispararExcecao_QuandoParcelaNaoEstiverEmAnalise()
        {
            var context = CriarContextoEmMemoria();
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.Add(morador);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Luz", Valor = 80, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = morador.IdUsuario, Valor = 80, Status = StatusParcela.PENDENTE };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);

            var excecao = Assert.Throws<Exception>(() => service.DesfazerPagamento(morador.IdUsuario, parcela.IdParcela));
            Assert.Contains("Só é possível desfazer pagamentos que ainda estão em análise", excecao.Message);
        }

        [Fact]
        public void DesfazerPagamento_DeveDispararExcecao_QuandoTentarDesfazerDividaDeOutro()
        {
            var context = CriarContextoEmMemoria();
            var dono = new Usuario { Nome = "Dono", Email = "dono@ufal.com", Senha = "123", Ativo = true };
            var invasor = new Usuario { Nome = "Invasor", Email = "invasor@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(dono, invasor);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Água", Valor = 40, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = dono.IdUsuario, Valor = 40, Status = StatusParcela.EM_ANALISE };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);

            Assert.Throws<UnauthorizedAccessException>(() => service.DesfazerPagamento(invasor.IdUsuario, parcela.IdParcela));
        }

        // ==========================================
        // 4. TESTES DE VALIDAÇÃO DO ADMIN
        // ==========================================

        [Fact]
        public void ValidarPagamento_DeveQuitarDivida_QuandoAdminAprovar()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Água", IdGrupo = grupo.IdGrupo, Valor = 50, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = morador.IdUsuario, Valor = 50, Status = StatusParcela.EM_ANALISE };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);
            var request = new ValidarPagamentoRequestDTO { Aprovado = true };

            var mensagem = service.ValidarPagamento(admin.IdUsuario, parcela.IdParcela, request);

            Assert.Contains("Pagamento aprovado com sucesso", mensagem);
            Assert.Equal(StatusParcela.PAGO, parcela.Status);
        }

        [Fact]
        public void ValidarPagamento_DeveRejeitarEVoltarStatus_QuandoAdminReprovar()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Internet", IdGrupo = grupo.IdGrupo, Valor = 100, Vencimento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            var parcela = new Parcela
            {
                IdDespesa = despesa.IdDespesa,
                IdUsuario = morador.IdUsuario,
                Valor = 100,
                Status = StatusParcela.EM_ANALISE,
                DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            context.Parcelas.Add(parcela);
            context.SaveChanges();

            var service = new DespesaService(context);
            var request = new ValidarPagamentoRequestDTO { Aprovado = false };

            var mensagem = service.ValidarPagamento(admin.IdUsuario, parcela.IdParcela, request);

            Assert.Contains("Pagamento rejeitado", mensagem);
            Assert.Equal(StatusParcela.PENDENTE, parcela.Status);
            Assert.Null(parcela.DataPagamento);
        }

        [Fact]
        public void ValidarPagamento_DeveDispararExcecao_QuandoNaoAdminTentarValidar()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.EM_ANALISE);
            var parcela = context.Parcelas.First();
            var service = new DespesaService(context);

            Assert.Throws<UnauthorizedAccessException>(() =>
                service.ValidarPagamento(morador.IdUsuario, parcela.IdParcela, new ValidarPagamentoRequestDTO { Aprovado = true }));
        }

        [Fact]
        public void ValidarPagamento_DeveDispararExcecao_QuandoParcelaNaoEstiverEmAnalise()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PENDENTE);
            var parcela = context.Parcelas.First();
            var service = new DespesaService(context);

            var excecao = Assert.Throws<Exception>(() =>
                service.ValidarPagamento(admin.IdUsuario, parcela.IdParcela, new ValidarPagamentoRequestDTO { Aprovado = true }));

            Assert.Contains("não está aguardando validação", excecao.Message);
        }

        // ==========================================
        // 5. TESTES DE QUITAR DÍVIDA (ADMIN)
        // ==========================================

        [Fact]
        public void QuitarDividaAdmin_DeveMarcarComoPago_QuandoAdminAcionar()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.ATRASADO);
            var parcela = context.Parcelas.First();
            var service = new DespesaService(context);

            var mensagem = service.QuitarDividaAdmin(admin.IdUsuario, parcela.IdParcela);

            Assert.Contains("quitada administrativamente", mensagem);
            Assert.Equal(StatusParcela.PAGO, parcela.Status);
            Assert.NotNull(parcela.DataPagamento);
        }

        [Fact]
        public void QuitarDividaAdmin_DeveDispararExcecao_QuandoNaoAdminTentarQuitar()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PENDENTE);
            var parcela = context.Parcelas.First();
            var service = new DespesaService(context);

            Assert.Throws<UnauthorizedAccessException>(() => service.QuitarDividaAdmin(morador.IdUsuario, parcela.IdParcela));
        }

        [Fact]
        public void QuitarDividaAdmin_DeveDispararExcecao_QuandoParcelaJaEstiverPaga()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PAGO);
            var parcela = context.Parcelas.First();
            var service = new DespesaService(context);

            var excecao = Assert.Throws<Exception>(() => service.QuitarDividaAdmin(admin.IdUsuario, parcela.IdParcela));
            Assert.Contains("já está paga", excecao.Message);
        }

        // ==========================================
        // 6. TESTES DE EDIÇÃO DE DESPESA
        // ==========================================

        [Fact]
        public void EditarDespesa_DeveAtualizarDados_QuandoAdminEDadosForemValidos()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PENDENTE);
            var service = new DespesaService(context);
            var request = new DespesaRequestDTO
            {
                Nome = "Conta Atualizada",
                Valor = 200,
                Vencimento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                IdGrupo = grupo.IdGrupo,
                MoradoresIds = new List<int> { morador.IdUsuario }
            };

            var mensagem = service.EditarDespesa(admin.IdUsuario, despesa.IdDespesa, request);

            Assert.Contains("atualizada com sucesso", mensagem);
            Assert.Equal("Conta Atualizada", context.Despesas.First().Nome);
        }

        [Fact]
        public void EditarDespesa_DeveDispararExcecao_QuandoNaoAdminTentarEditar()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PENDENTE);
            var service = new DespesaService(context);
            var request = new DespesaRequestDTO { Nome = "Hack", Valor = 1, IdGrupo = grupo.IdGrupo, MoradoresIds = new List<int>() };

            Assert.Throws<UnauthorizedAccessException>(() => service.EditarDespesa(morador.IdUsuario, despesa.IdDespesa, request));
        }

        [Fact]
        public void EditarDespesa_DeveDispararExcecao_QuandoDespesaNaoExistir()
        {
            var (context, admin, _, grupo, _) = CriarCenarioCompleto();
            var service = new DespesaService(context);
            var request = new DespesaRequestDTO { Nome = "X", Valor = 1, IdGrupo = grupo.IdGrupo, MoradoresIds = new List<int>() };

            Assert.Throws<KeyNotFoundException>(() => service.EditarDespesa(admin.IdUsuario, 9999, request));
        }

        public void EditarDespesa_DeveDispararExcecao_QuandoPossuirParcelaPaga()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PAGO);
            var service = new DespesaService(context);
            var request = new DespesaRequestDTO
            {
                Nome = "Tentativa",
                Valor = 999,
                Vencimento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                IdGrupo = grupo.IdGrupo,
                MoradoresIds = new List<int> { morador.IdUsuario }
            };

            var excecao = Assert.Throws<Exception>(() => service.EditarDespesa(admin.IdUsuario, despesa.IdDespesa, request));
            Assert.Contains("parcelas pagas ou em análise", excecao.Message);
        }

        // ==========================================
        // 7. TESTES DE HISTÓRICO E EXCLUSÃO
        // ==========================================

        [Fact]
        public void DeletarDespesa_DeveDispararExcecao_QuandoPossuirParcelasPagas()
        {
            var context = CriarContextoEmMemoria();
            var admin = new Usuario { Nome = "Admin", Email = "admin@ufal.com", Senha = "123", Ativo = true };
            var morador = new Usuario { Nome = "Morador", Email = "m@ufal.com", Senha = "123", Ativo = true };
            context.Usuarios.AddRange(admin, morador);
            context.SaveChanges();

            var grupo = new Grupo { Nome = "República", IdAdmin = admin.IdUsuario, Ativo = true, CodigoAcesso = "12345678" };
            context.Grupos.Add(grupo);
            context.SaveChanges();

            var despesa = new Despesa { Nome = "Gás", IdGrupo = grupo.IdGrupo, Valor = 100, Status = StatusDespesa.ATIVA, Ativo = true };
            context.Despesas.Add(despesa);
            context.SaveChanges();

            context.Parcelas.Add(new Parcela { IdDespesa = despesa.IdDespesa, IdUsuario = morador.IdUsuario, Valor = 100, Status = StatusParcela.PAGO });
            context.SaveChanges();

            var service = new DespesaService(context);

            var excecao = Assert.Throws<Exception>(() => service.DeletarDespesa(admin.IdUsuario, despesa.IdDespesa));
            Assert.Contains("Não é possível deletar uma despesa que ainda possui parcelas pagas", excecao.Message);
        }

        [Fact]
        public void DeletarDespesa_DeveDispararExcecao_QuandoPossuirParcelasEmAnalise()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.EM_ANALISE);
            var service = new DespesaService(context);

            var excecao = Assert.Throws<Exception>(() => service.DeletarDespesa(admin.IdUsuario, despesa.IdDespesa));
            Assert.Contains("Não é possível deletar uma despesa que ainda possui parcelas pagas", excecao.Message);
        }

        [Fact]
        public void DeletarDespesa_DeveFazerSoftDelete_QuandoParcelasEstiveremPendentes()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PENDENTE);
            var service = new DespesaService(context);

            var mensagem = service.DeletarDespesa(admin.IdUsuario, despesa.IdDespesa);

            Assert.Contains("arquivada com sucesso", mensagem);
            Assert.False(context.Despesas.First().Ativo);
        }

        [Fact]
        public void DeletarDespesa_DeveDispararExcecao_QuandoNaoAdminTentarDeletar()
        {
            var (context, admin, morador, grupo, despesa) = CriarCenarioCompleto(StatusParcela.PENDENTE);
            var service = new DespesaService(context);

            Assert.Throws<UnauthorizedAccessException>(() => service.DeletarDespesa(morador.IdUsuario, despesa.IdDespesa));
        }

        [Fact]
        public void DeletarDespesa_DeveDispararExcecao_QuandoDespesaNaoExistir()
        {
            var (context, admin, _, _, _) = CriarCenarioCompleto();
            var service = new DespesaService(context);

            Assert.Throws<KeyNotFoundException>(() => service.DeletarDespesa(admin.IdUsuario, 9999));
        }
    }
}
