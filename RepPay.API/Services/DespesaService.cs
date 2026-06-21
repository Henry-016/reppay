using RepPay.API.DTOs;
using RepPay.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RepPay.API.Services
{
    public class DespesaService : IDespesaService
    {
        private readonly AppDbContext _context;

        public DespesaService(AppDbContext context)
        {
            _context = context;
        }

        public string CadastrarDespesa(int idLogado, DespesaRequestDTO request)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == request.IdGrupo);

            if (grupo == null || grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador pode lançar despesas.");
            }

            if (!grupo.Ativo)
            {
                throw new Exception("Não é possível lançar despesas em uma república encerrada.");
            }

            if (request.Vencimento < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new Exception("A data de vencimento não pode ser no passado.");
            }

            if (request.MoradoresIds == null || request.MoradoresIds.Count == 0)
            {
                throw new Exception("É necessário selecionar pelo menos um morador para dividir esta conta.");
            }

            if (request.Valor <= 0)
            {
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            }

            var moradoresValidos = _context.Pertences
             .Include(p => p.IdUsuarioNavigation)
             .Where(p => p.IdGrupo == request.IdGrupo
                  && request.MoradoresIds.Contains(p.IdUsuario)
                  && p.IdUsuarioNavigation.Ativo == true)
             .Select(p => p.IdUsuario)
             .ToList();

            if (moradoresValidos.Count != request.MoradoresIds.Count)
            {
                throw new Exception("Um ou mais moradores informados não existem ou não pertencem a esta república.");
            }

            var novaDespesa = new Despesa
            {
                Nome = request.Nome,
                Valor = request.Valor,
                Vencimento = request.Vencimento,
                Icone = request.Icone,
                IdGrupo = request.IdGrupo,
                Status = StatusDespesa.ATIVA,
                Parcelas = new List<Parcela>()
            };

            int totalMoradores = request.MoradoresIds.Count;
            decimal valorBaseParcela = Math.Round(request.Valor / totalMoradores, 2);
            decimal diferencaCentavos = request.Valor - (valorBaseParcela * totalMoradores);

            for (int i = 0; i < totalMoradores; i++)
            {
                decimal valorParcela = (i == totalMoradores - 1)
                    ? valorBaseParcela + diferencaCentavos
                    : valorBaseParcela;

                novaDespesa.Parcelas.Add(new Parcela
                {
                    IdUsuario = request.MoradoresIds[i],
                    Valor = valorParcela,
                    Status = StatusParcela.PENDENTE
                });
            }

            _context.Despesas.Add(novaDespesa);
            _context.SaveChanges();

            return "Despesa lançada e rateio gerado com sucesso!";
        }

        public ResumoDividasResponseDTO GetMinhasDividas(int idLogado, int idGrupo)
        {
            var dividas = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .ThenInclude(d => d.IdGrupoNavigation)
                .Where(p => p.IdUsuario == idLogado && p.IdDespesaNavigation.IdGrupo == idGrupo
                         && p.IdDespesaNavigation.Ativo == true && p.IdDespesaNavigation.IdGrupoNavigation.Ativo == true
                         && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO))
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

            return new ResumoDividasResponseDTO
            {
                TotalDevido = dividas.Sum(d => d.Valor),
                ListaDividas = dividas
            };
        }

        public ResumoInadimplentesDTO GetInadimplentes(int idLogado, int idGrupo)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador do grupo pode ver essa lista!");
            }

            var inadimplentes = _context.Parcelas
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo && p.IdDespesaNavigation.Ativo == true
                         && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO))
                .Select(p => new InadimplenteResponseDTO
                {
                    IdParcela = p.IdParcela,
                    NomeMorador = p.IdUsuarioNavigation.Nome,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Icone = p.IdDespesaNavigation.Icone,
                    Valor = p.Valor,
                    Vencimento = p.IdDespesaNavigation.Vencimento,
                    Status = p.Status.ToString()
                })
                .OrderBy(p => p.Vencimento)
                .ThenBy(p => p.NomeMorador)
                .ToList();

            return new ResumoInadimplentesDTO
            {
                TotalAReceber = inadimplentes.Sum(i => i.Valor),
                ListaInadimplentes = inadimplentes
            };
        }

        public string PagarParcela(int idLogado, int idParcela)
        {
            var parcela = _context.Parcelas
            .Include(p => p.IdDespesaNavigation)
            .FirstOrDefault(p => p.IdParcela == idParcela && p.IdDespesaNavigation.Ativo == true);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela não encontrada.");
            }

            if (parcela.IdUsuario != idLogado)
            {
                throw new UnauthorizedAccessException("Não tem permissão para alterar uma dívida que não lhe pertence!");
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                throw new Exception("Esta parcela já se encontra paga.");
            }

            if (parcela.Status == StatusParcela.EM_ANALISE)
            {
                throw new Exception("Este pagamento já foi sinalizado e está aguardando validação do administrador.");
            }

            parcela.Status = StatusParcela.EM_ANALISE;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            return "Pagamento sinalizado! Aguardando validação do administrador.";
        }

        public string DesfazerPagamento(int idLogado, int idParcela)
        {
            var parcela = _context.Parcelas
            .Include(p => p.IdDespesaNavigation)
            .FirstOrDefault(p => p.IdParcela == idParcela && p.IdDespesaNavigation.Ativo == true);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela não encontrada.");
            }

            if (parcela.IdUsuario != idLogado)
            {
                throw new UnauthorizedAccessException("Não tem permissão para alterar uma dívida que não lhe pertence!");
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                throw new Exception("Só é possível desfazer pagamentos que ainda estão em análise.");
            }

            parcela.DataPagamento = null;
            parcela.Status = StatusParcela.PENDENTE;
            _context.SaveChanges();

            return "Sinalização de pagamento desfeita com sucesso.";
        }

        public string ValidarPagamento(int idLogado, int idParcela, ValidarPagamentoRequestDTO request)
        {
            var parcela = _context.Parcelas
            .Include(p => p.IdDespesaNavigation)
            .ThenInclude(d => d.IdGrupoNavigation)
            .FirstOrDefault(p => p.IdParcela == idParcela && p.IdDespesaNavigation.Ativo == true);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela não encontrada.");
            }

            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador do grupo pode validar pagamentos.");
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                throw new Exception("Esta parcela não está aguardando validação.");
            }

            if (request.Aprovado == true)
            {
                parcela.Status = StatusParcela.PAGO;
            }

            else
            {
                parcela.DataPagamento = null;
                if (DateOnly.FromDateTime(DateTime.UtcNow) > parcela.IdDespesaNavigation.Vencimento)
                {
                    parcela.Status = StatusParcela.ATRASADO;
                }
                else
                {
                    parcela.Status = StatusParcela.PENDENTE;
                }
            }

            _context.SaveChanges();
            return request.Aprovado == true ? "Pagamento aprovado com sucesso! A parcela foi quitada." : "Pagamento rejeitado. A dívida voltou para o morador.";
        }

        public List<HistoricoPagoResponseDTO> GetMeuHistoricoPago(int idLogado, int idGrupo)
        {
            return _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.IdDespesaNavigation.IdGrupo == idGrupo
                      && p.Status == StatusParcela.PAGO && p.IdDespesaNavigation.Ativo == true)
                .Select(p => new HistoricoPagoResponseDTO
                {
                    IdParcela = p.IdParcela,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Icone = p.IdDespesaNavigation.Icone,
                    ValorPago = p.Valor,
                    DataPagamento = p.DataPagamento,
                    Vencimento = p.IdDespesaNavigation.Vencimento
                })
                .OrderByDescending(p => p.DataPagamento)
                .ToList();
        }

        public List<HistoricoGrupoDTO> GetHistoricoPagoGrupo(int idLogado, int idGrupo)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode ver o histórico financeiro global.");
            }

            return _context.Parcelas
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo && p.IdDespesaNavigation.Ativo == true && p.Status == StatusParcela.PAGO)
                .Select(p => new HistoricoGrupoDTO
                {
                    IdParcela = p.IdParcela,
                    NomeMorador = p.IdUsuarioNavigation.Nome,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Icone = p.IdDespesaNavigation.Icone,
                    ValorPago = p.Valor,
                    DataPagamento = p.DataPagamento,
                    Vencimento = p.IdDespesaNavigation.Vencimento
                })
                .OrderByDescending(p => p.DataPagamento)
                .ToList();
        }

        public List<AnaliseMoradorDTO> GetMinhasAnalises(int idLogado, int idGrupo)
        {
            return _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.IdDespesaNavigation.IdGrupo == idGrupo
                      && p.IdDespesaNavigation.Ativo == true && p.Status == StatusParcela.EM_ANALISE)
                .Select(p => new AnaliseMoradorDTO
                {
                    IdParcela = p.IdParcela,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Icone = p.IdDespesaNavigation.Icone,
                    Valor = p.Valor,
                    Vencimento = p.IdDespesaNavigation.Vencimento,
                    DataSinalizacao = p.DataPagamento
                })
                .OrderByDescending(p => p.DataSinalizacao)
                .ToList();
        }

        public List<AnaliseAdminDTO> GetAnalisesPendentesGrupo(int idLogado, int idGrupo)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode ver as validações pendentes.");
            }

            return _context.Parcelas
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo && p.IdDespesaNavigation.Ativo == true && p.Status == StatusParcela.EM_ANALISE)
                .Select(p => new AnaliseAdminDTO
                {
                    IdParcela = p.IdParcela,
                    NomeMorador = p.IdUsuarioNavigation.Nome,
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    Icone = p.IdDespesaNavigation.Icone,
                    Valor = p.Valor,
                    DataSinalizacao = p.DataPagamento
                })
                .OrderBy(p => p.DataSinalizacao)
                .ToList();
        }

        public string QuitarDividaAdmin(int idLogado, int idParcela)
        {
            var parcela = _context.Parcelas
            .Include(p => p.IdDespesaNavigation)
            .ThenInclude(d => d.IdGrupoNavigation)
            .FirstOrDefault(p => p.IdParcela == idParcela && p.IdDespesaNavigation.Ativo == true);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela não encontrada.");
            }

            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador da república pode quitar dívidas administrativamente.");
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                throw new Exception("Esta parcela já está paga e não precisa de intervenção.");
            }

            parcela.Status = StatusParcela.PAGO;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            return "Dívida quitada administrativamente com sucesso! O histórico do morador foi limpo para esta conta.";
        }

        public string EditarDespesa(int idLogado, int idDespesa, EditarDespesaRequestDTO request)
        {
            if (request.Valor <= 0)
            {
                throw new ArgumentException("O valor da despesa deve ser maior que zero.");
            }

            if (request.Vencimento < DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException("O novo vencimento não pode ser uma data que já passou.");
            }

            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .Include(d => d.Parcelas)
                .FirstOrDefault(d => d.IdDespesa == idDespesa && d.Ativo == true);

            if (despesa == null)
            {
                throw new KeyNotFoundException("Despesa não encontrada.");
            }

            if (despesa.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode editar despesas.");
            }

            bool temParcelaBloqueante = despesa.Parcelas.Any(p =>
                p.Status == StatusParcela.PAGO || p.Status == StatusParcela.EM_ANALISE);

            if (temParcelaBloqueante)
            {
                throw new Exception("Não é permitido alterar o valor ou o vencimento de uma despesa que já possui parcelas pagas ou em análise.");
            }

            if (despesa.Valor != request.Valor)
            {
                var listaParcelas = despesa.Parcelas.ToList();
                int totalParcelas = listaParcelas.Count;

                if (totalParcelas > 0)
                {
                    decimal valorBaseParcela = Math.Round(request.Valor / totalParcelas, 2);
                    decimal diferencaCentavos = request.Valor - (valorBaseParcela * totalParcelas);

                    for (int i = 0; i < totalParcelas; i++)
                    {
                        listaParcelas[i].Valor = (i == totalParcelas - 1)
                            ? valorBaseParcela + diferencaCentavos
                            : valorBaseParcela;
                    }
                }
            }

            despesa.Nome = request.Nome;
            despesa.Valor = request.Valor;
            despesa.Vencimento = request.Vencimento;
            despesa.Icone = request.Icone;

            _context.SaveChanges();

            return "Despesa atualizada com sucesso!";
        }

        public string DeletarDespesa(int idLogado, int idDespesa)
        {
            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .Include(d => d.Parcelas)
                .FirstOrDefault(d => d.IdDespesa == idDespesa && d.Ativo == true);

            if (despesa == null)
            {
                throw new KeyNotFoundException("Despesa não encontrada.");
            }

            if (despesa.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode apagar despesas.");
            }

            bool temParcelaPagaOuEmAnalise = despesa.Parcelas.Any(p =>
                p.Status == StatusParcela.PAGO || p.Status == StatusParcela.EM_ANALISE);

            if (temParcelaPagaOuEmAnalise)
            {
                throw new Exception("Não é possível deletar uma despesa que ainda possui parcelas pagas.");
            }

            despesa.Ativo = false;
            _context.SaveChanges();

            return "Despesa arquivada com sucesso!";
        }

        public List<DespesaGerenciamentoResponseDTO> GetDespesasParaGerenciamento(int idLogado, int idGrupo)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode acessar a lista de gerenciamento de despesas.");
            }

            return _context.Despesas
                .Include(d => d.Parcelas)
                .Where(d => d.IdGrupo == idGrupo && d.Ativo == true)
                .Where(d => !d.Parcelas.Any(p => p.Status == StatusParcela.PAGO || p.Status == StatusParcela.EM_ANALISE))
                .Select(d => new DespesaGerenciamentoResponseDTO
                {
                    IdDespesa = d.IdDespesa,
                    Nome = d.Nome,
                    ValorTotal = d.Valor,
                    Vencimento = d.Vencimento,
                    Icone = d.Icone
                })
                .OrderBy(d => d.Vencimento)
                .ToList();
        }
    }
}