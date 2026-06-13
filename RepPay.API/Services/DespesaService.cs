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
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador pode lançar despesas.");

            if (request.MoradoresIds == null || request.MoradoresIds.Count == 0)
                throw new Exception("É necessário selecionar pelo menos um morador para dividir esta conta.");

            var moradoresValidos = _context.Pertences
                .Where(p => p.IdGrupo == request.IdGrupo && request.MoradoresIds.Contains(p.IdUsuario))
                .Select(p => p.IdUsuario)
                .ToList();

            if (moradoresValidos.Count != request.MoradoresIds.Count)
                throw new Exception("Um ou mais moradores informados não existem ou não pertencem a esta república.");

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

            decimal valorPorPessoa = Math.Round(request.Valor / request.MoradoresIds.Count, 2);

            foreach (var idMorador in request.MoradoresIds)
            {
                novaDespesa.Parcelas.Add(new Parcela
                {
                    IdUsuario = idMorador,
                    Valor = valorPorPessoa,
                    Status = StatusParcela.PENDENTE
                });
            }

            _context.Despesas.Add(novaDespesa);
            _context.SaveChanges();

            return "Despesa lançada e rateio gerado com sucesso!";
        }

        public ResumoDividasDTO GetMinhasDividas(int idLogado)
        {
            var dividas = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.IdDespesaNavigation.Ativo == true && p.IdDespesaNavigation.IdGrupoNavigation.Ativo == true
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

            return new ResumoDividasDTO
            {
                TotalDevido = dividas.Sum(d => d.Valor),
                ListaDividas = dividas
            };
        }

        public ResumoInadimplentesDTO GetInadimplentes(int idLogado, int idGrupo)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);
            if (grupo == null) throw new KeyNotFoundException("Grupo não encontrado.");
            if (grupo.IdAdmin != idLogado) throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador do grupo pode ver essa lista!");

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
            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);
            if (parcela == null) throw new KeyNotFoundException("Parcela não encontrada.");
            if (parcela.IdUsuario != idLogado) throw new UnauthorizedAccessException("Não tem permissão para alterar uma dívida que não lhe pertence!");
            if (parcela.Status == StatusParcela.PAGO) throw new Exception("Esta parcela já se encontra paga.");

            parcela.Status = StatusParcela.EM_ANALISE;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            return "Pagamento sinalizado! Aguardando validação do administrador.";
        }

        public string DesfazerPagamento(int idLogado, int idParcela)
        {
            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);
            if (parcela == null) throw new KeyNotFoundException("Parcela não encontrada.");
            if (parcela.IdUsuario != idLogado) throw new UnauthorizedAccessException("Não tem permissão para alterar uma dívida que não lhe pertence!");
            if (parcela.Status != StatusParcela.EM_ANALISE) throw new Exception("Só é possível desfazer pagamentos que ainda estão em análise.");

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
                .FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null) throw new KeyNotFoundException("Parcela não encontrada.");
            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador do grupo pode validar pagamentos.");
            if (parcela.Status != StatusParcela.EM_ANALISE)
                throw new Exception("Esta parcela não está aguardando validação.");

            if (request.Aprovado)
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
            return request.Aprovado ? "Pagamento aprovado com sucesso! A parcela foi quitada." : "Pagamento rejeitado. A dívida voltou para o morador.";
        }

        public List<HistoricoPagoResponseDTO> GetMeuHistoricoPago(int idLogado)
        {
            return _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.Status == StatusParcela.PAGO && p.IdDespesaNavigation.Ativo == true)
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
            if (grupo == null) throw new KeyNotFoundException("Grupo não encontrado.");
            if (grupo.IdAdmin != idLogado) throw new UnauthorizedAccessException("Apenas o administrador pode ver o histórico financeiro global.");

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

        public List<AnaliseMoradorDTO> GetMinhasAnalises(int idLogado)
        {
            return _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado && p.IdDespesaNavigation.Ativo == true && p.Status == StatusParcela.EM_ANALISE)
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
            if (grupo == null) throw new KeyNotFoundException("Grupo não encontrado.");
            if (grupo.IdAdmin != idLogado) throw new UnauthorizedAccessException("Apenas o administrador pode ver as validações pendentes.");

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
                .FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null) throw new KeyNotFoundException("Parcela não encontrada.");
            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador da república pode quitar dívidas administrativamente.");
            if (parcela.Status == StatusParcela.PAGO)
                throw new Exception("Esta parcela já está paga e não precisa de intervenção.");

            parcela.Status = StatusParcela.PAGO;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            return "Dívida quitada administrativamente com sucesso! O histórico do morador foi limpo para esta conta.";
        }

        public string EditarDespesa(int idLogado, int idDespesa, DespesaRequestDTO request)
        {
            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .FirstOrDefault(d => d.IdDespesa == idDespesa);

            if (despesa == null) throw new KeyNotFoundException("Despesa não encontrada.");
            if (despesa.IdGrupoNavigation.IdAdmin != idLogado) throw new UnauthorizedAccessException("Apenas o administrador pode editar despesas.");

            despesa.Nome = request.Nome;
            despesa.Valor = request.Valor;
            despesa.Vencimento = request.Vencimento;
            despesa.Icone = request.Icone;

            try
            {
                _context.SaveChanges();
                return "Despesa atualizada com sucesso!";
            }
            catch (Exception)
            {
                throw new Exception("Não é permitido alterar o valor ou o vencimento de uma despesa que já possui parcelas pagas ou em análise.");
            }
        }

        public string DeletarDespesa(int idLogado, int idDespesa)
        {
            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .FirstOrDefault(d => d.IdDespesa == idDespesa);

            if (despesa == null) throw new KeyNotFoundException("Despesa não encontrada.");
            if (despesa.IdGrupoNavigation.IdAdmin != idLogado) throw new UnauthorizedAccessException("Apenas o administrador pode apagar despesas.");

            despesa.Ativo = false;

            try
            {
                _context.SaveChanges();
                return "Despesa arquivada com sucesso!";
            }
            catch (Exception)
            {
                throw new Exception("Não é possível deletar uma despesa que ainda possui parcelas pagas!");
            }
        }
    }
}