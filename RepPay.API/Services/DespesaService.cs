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
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador pode lan�ar despesas.");
            }

            if (request.MoradoresIds == null || request.MoradoresIds.Count == 0)
            {
                throw new Exception("� necess�rio selecionar pelo menos um morador para dividir esta conta.");
            }

            var moradoresValidos = _context.Pertences
                .Where(p => p.IdGrupo == request.IdGrupo && request.MoradoresIds.Contains(p.IdUsuario))
                .Select(p => p.IdUsuario)
                .ToList();

            if (moradoresValidos.Count != request.MoradoresIds.Count)
            {
                throw new Exception("Um ou mais moradores informados n�o existem ou n�o pertencem a esta rep�blica.");
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

            return "Despesa lan�ada e rateio gerado com sucesso!";
        }

        public ResumoDividasDTO GetMinhasDividas(int idLogado, int idGrupo)
        {
            var dividas = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
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

            return new ResumoDividasDTO
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
                throw new KeyNotFoundException("Grupo n�o encontrado.");
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
            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela n�o encontrada.");
            }

            if (parcela.IdUsuario != idLogado)
            {
                throw new UnauthorizedAccessException("N�o tem permiss�o para alterar uma d�vida que n�o lhe pertence!");
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                throw new Exception("Esta parcela j� se encontra paga.");
            }

            parcela.Status = StatusParcela.EM_ANALISE;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            return "Pagamento sinalizado! Aguardando valida��o do administrador.";
        }

        public string DesfazerPagamento(int idLogado, int idParcela)
        {
            var parcela = _context.Parcelas.FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela n�o encontrada.");
            }

            if (parcela.IdUsuario != idLogado)
            {
                throw new UnauthorizedAccessException("N�o tem permiss�o para alterar uma d�vida que n�o lhe pertence!");
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                throw new Exception("S� � poss�vel desfazer pagamentos que ainda est�o em an�lise.");
            }

            parcela.DataPagamento = null;
            parcela.Status = StatusParcela.PENDENTE;
            _context.SaveChanges();

            return "Sinaliza��o de pagamento desfeita com sucesso.";
        }

        public string ValidarPagamento(int idLogado, int idParcela, ValidarPagamentoRequestDTO request)
        {
            var parcela = _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .ThenInclude(d => d.IdGrupoNavigation)
                .FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela n�o encontrada.");
            }

            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador do grupo pode validar pagamentos.");
            }

            if (parcela.Status != StatusParcela.EM_ANALISE)
            {
                throw new Exception("Esta parcela n�o est� aguardando valida��o.");
            }

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
            return request.Aprovado ? "Pagamento aprovado com sucesso! A parcela foi quitada." : "Pagamento rejeitado. A d�vida voltou para o morador.";
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
                throw new KeyNotFoundException("Grupo n�o encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode ver o hist�rico financeiro global.");
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
                throw new KeyNotFoundException("Grupo n�o encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode ver as valida��es pendentes.");
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
                .FirstOrDefault(p => p.IdParcela == idParcela);

            if (parcela == null)
            {
                throw new KeyNotFoundException("Parcela n�o encontrada.");
            }

            if (parcela.IdDespesaNavigation.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador da rep�blica pode quitar d�vidas administrativamente.");
            }

            if (parcela.Status == StatusParcela.PAGO)
            {
                throw new Exception("Esta parcela j� est� paga e n�o precisa de interven��o.");
            }

            parcela.Status = StatusParcela.PAGO;
            parcela.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            return "D�vida quitada administrativamente com sucesso! O hist�rico do morador foi limpo para esta conta.";
        }

        public string EditarDespesa(int idLogado, int idDespesa, DespesaRequestDTO request)
        {
            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .FirstOrDefault(d => d.IdDespesa == idDespesa);

            if (despesa == null)
            {
                throw new KeyNotFoundException("Despesa n�o encontrada.");
            }

            if (despesa.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode editar despesas.");
            }

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
                throw new Exception("N�o � permitido alterar o valor ou o vencimento de uma despesa que j� possui parcelas pagas ou em an�lise.");
            }
        }

        public string DeletarDespesa(int idLogado, int idDespesa)
        {
            var despesa = _context.Despesas
                .Include(d => d.IdGrupoNavigation)
                .FirstOrDefault(d => d.IdDespesa == idDespesa);

            if (despesa == null)
            {
                throw new KeyNotFoundException("Despesa n�o encontrada.");
            }

            if (despesa.IdGrupoNavigation.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode apagar despesas.");
            }

            despesa.Ativo = false;

            try
            {
                _context.SaveChanges();
                return "Despesa arquivada com sucesso!";
            }
            catch (Exception)
            {
                throw new Exception("N�o � poss�vel deletar uma despesa que ainda possui parcelas pagas!");
            }
        }
    }
}