using RepPay.API.DTOs;
using RepPay.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RepPay.API.Services
{
    public class GrupoService : IGrupoService
    {
        private readonly AppDbContext _context;

        public GrupoService(AppDbContext context)
        {
            _context = context;
        }

        private string GerarCodigoAcesso()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string codigo;
            bool codigoExiste;

            do
            {
                codigo = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                codigoExiste = _context.Grupos.Any(g => g.CodigoAcesso == codigo);
            }
            while (codigoExiste);

            return codigo;
        }

        public GrupoCriadoResponseDTO CriarGrupo(int idAdmin, GrupoRequestDTO request)
        {
            string codigoAcesso = GerarCodigoAcesso();

            var novoGrupo = new Grupo
            {
                Nome = request.Nome,
                ImagemBanner = request.ImagemBanner,
                CodigoAcesso = codigoAcesso,
                IdAdmin = idAdmin
            };

            _context.Grupos.Add(novoGrupo);
            _context.SaveChanges();

            bool jaEMembro = _context.Pertences
                .Any(p => p.IdUsuario == idAdmin && p.IdGrupo == novoGrupo.IdGrupo);

            if (!jaEMembro)
            {
                _context.Pertences.Add(new Pertence
                {
                    IdUsuario = idAdmin,
                    IdGrupo = novoGrupo.IdGrupo
                });
                _context.SaveChanges();
            }

            return new GrupoCriadoResponseDTO
            {
                Mensagem = "República criada com sucesso!",
                CodigoAcesso = codigoAcesso
            };
        }

        public string EntrarNoGrupo(int idUsuario, EntrarGrupoRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.CodigoAcesso))
            {
                throw new Exception("O código de acesso não pode estar vazio.");
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.CodigoAcesso.ToLower() == request.CodigoAcesso.ToLower() && g.Ativo == true);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Código de acesso inválido ou república não encontrada.");
            }

            bool jaPertence = _context.Pertences.Any(p => p.IdGrupo == grupo.IdGrupo && p.IdUsuario == idUsuario);

            if (jaPertence)
            {
                throw new Exception("Você já faz parte desta república!");
            }

            var novoVinculo = new Pertence
            {
                IdGrupo = grupo.IdGrupo,
                IdUsuario = idUsuario
            };

            _context.Pertences.Add(novoVinculo);
            _context.SaveChanges();

            return $"Bem-vindo(a) à {grupo.Nome}!";
        }

        public List<MeuGrupoResponseDTO> GetMeusGrupos(int idLogado)
        {
            return _context.Pertences
                .Include(p => p.IdGrupoNavigation)
                .Where(p => p.IdUsuario == idLogado && p.IdGrupoNavigation.Ativo == true)
                .Select(p => new MeuGrupoResponseDTO
                {
                    IdGrupo = p.IdGrupoNavigation.IdGrupo,
                    Nome = p.IdGrupoNavigation.Nome,
                    CodigoAcesso = p.IdGrupoNavigation.CodigoAcesso,
                    ImagemBanner = p.IdGrupoNavigation.ImagemBanner,
                    IsAdmin = p.IdGrupoNavigation.IdAdmin == idLogado
                }).ToList();
        }

        public MeuGrupoResponseDTO GetGrupoPorId(int idLogado, int idGrupo)
        {
            var relacaoPertence = _context.Pertences
                .Include(p => p.IdGrupoNavigation)
                .FirstOrDefault(p => p.IdUsuario == idLogado && p.IdGrupo == idGrupo && p.IdGrupoNavigation.Ativo == true);

            if (relacaoPertence == null)
            {
                throw new UnauthorizedAccessException("Acesso negado. Você não pertence a este grupo ou ele não existe.");
            }

            var grupo = relacaoPertence.IdGrupoNavigation;

            return new MeuGrupoResponseDTO
            {
                IdGrupo = grupo.IdGrupo,
                Nome = grupo.Nome,
                CodigoAcesso = grupo.CodigoAcesso,
                ImagemBanner = grupo.ImagemBanner,
                IsAdmin = grupo.IdAdmin == idLogado
            };
        }

        public List<MembroResponseDTO> GetMembrosDoGrupo(int idLogado, int idGrupo)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo && g.Ativo == true);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            bool usuarioPertence = _context.Pertences.Any(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado);

            if (!usuarioPertence)
            {
                throw new UnauthorizedAccessException("Acesso negado. Você não pertence a este grupo.");
            }

            return _context.Pertences
                .Include(p => p.IdUsuarioNavigation)
                .Where(p => p.IdGrupo == idGrupo)
                .Select(p => new MembroResponseDTO
                {
                    IdUsuario = p.IdUsuario,
                    Nome = p.IdUsuarioNavigation.Nome,
                    Email = p.IdUsuarioNavigation.Email,
                    FotoPerfil = p.IdUsuarioNavigation.FotoPerfil,
                    IsAdmin = p.IdUsuario == grupo.IdAdmin,

                    TotalDevido = _context.Parcelas
                        .Where(parcela => parcela.IdUsuario == p.IdUsuario
                                       && parcela.IdDespesaNavigation.IdGrupo == idGrupo
                                       && parcela.IdDespesaNavigation.Ativo == true
                                       && (parcela.Status == StatusParcela.PENDENTE || parcela.Status == StatusParcela.ATRASADO))
                        .Sum(parcela => parcela.Valor)
                })
                .OrderByDescending(m => m.IsAdmin)
                .ThenBy(m => m.Nome)
                .ToList();
        }

        public string SairDoGrupo(int idLogado, int idGrupo)
        {
            var vinculo = _context.Pertences.FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado);

            if (vinculo == null)
            {
                throw new KeyNotFoundException("Você não pertence a esta república.");
            }

            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo != null && grupo.IdAdmin == idLogado)
            {
                throw new Exception("Você é o administrador do grupo. Transfira a liderança para outro morador antes de sair.");
            }     

            bool temDividas = _context.Parcelas
                .Any(p => p.IdUsuario == idLogado && p.IdDespesaNavigation.IdGrupo == idGrupo
                       && p.IdDespesaNavigation.Ativo == true
                       && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO || p.Status == StatusParcela.EM_ANALISE));

            if (temDividas)
            {
                throw new Exception("Você possui dívidas pendentes ou em análise nesta república. Quite todas as contas antes de sair!");
            }

            _context.Pertences.Remove(vinculo);
            _context.SaveChanges();

            return "Você saiu da república com sucesso. Sentiremos sua falta!";
        }

        public string ExpulsarMorador(int idLogado, int idGrupo, int idMorador)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador pode expulsar moradores.");
            }

            if (idLogado == idMorador)
            {
                throw new Exception("Você não pode expulsar a si mesmo. Caso queira sair, utilize a opção de saída voluntária ou exclua o grupo.");
            }

            var vinculo = _context.Pertences.FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idMorador);

            if (vinculo == null)
            {
                throw new KeyNotFoundException("Este usuário não é um morador da sua república.");
            }

            bool moradorTemDividas = _context.Parcelas
                .Any(p => p.IdUsuario == idMorador
                       && p.IdDespesaNavigation.IdGrupo == idGrupo
                       && p.IdDespesaNavigation.Ativo == true
                       && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO || p.Status == StatusParcela.EM_ANALISE));

            if (moradorTemDividas)
            {
                throw new Exception("Não é possível expulsar este morador pois ele possui dívidas ativas. Quite as pendências financeiras dele antes de removê-lo.");
            }

            _context.Pertences.Remove(vinculo);
            _context.SaveChanges();

            return "Morador removido da república com sucesso.";
        }

        public string TransferirAdmin(int idLogado, int idGrupo, int idNovoAdmin)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Acesso negado. Apenas o administrador atual pode transferir a liderança da casa.");
            }

            if (idLogado == idNovoAdmin)
            {
                throw new Exception("Você já é o administrador desta república.");
            }

            var moradorDestino = _context.Pertences
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefault(p => p.IdGrupo == idGrupo && p.IdUsuario == idNovoAdmin);

            if (moradorDestino == null)
            {
                throw new KeyNotFoundException("O usuário escolhido não é um morador desta república.");
            }

            if (!moradorDestino.IdUsuarioNavigation.Ativo)
            {
                throw new Exception("Não é possível transferir a liderança para uma conta desativada.");
            }

            grupo.IdAdmin = idNovoAdmin;
            _context.SaveChanges();

            return $"Liderança transferida com sucesso para {moradorDestino.IdUsuarioNavigation.Nome}! Você agora é um morador comum.";
        }

        public ProximaContaResponseDTO? ObterProximaContaGrupo(int idLogado, int idGrupo)
        {
            if (!_context.Pertences.Any(p => p.IdGrupo == idGrupo && p.IdUsuario == idLogado))
            {
                throw new UnauthorizedAccessException("Você não pertence a esta república.");
            }

            return _context.Parcelas
                .Include(p => p.IdDespesaNavigation)
                .Where(p => p.IdUsuario == idLogado
                         && p.IdDespesaNavigation.IdGrupo == idGrupo
                         && p.IdDespesaNavigation.Ativo == true
                         && (p.Status == StatusParcela.PENDENTE || p.Status == StatusParcela.ATRASADO))
                .OrderBy(p => p.IdDespesaNavigation.Vencimento)
                .Select(p => new ProximaContaResponseDTO
                {
                    NomeDespesa = p.IdDespesaNavigation.Nome,
                    NomeGrupo = null,
                    Vencimento = p.IdDespesaNavigation.Vencimento,
                    Valor = p.Valor
                })
                .FirstOrDefault();
        }

        public string DeletarGrupo(int idLogado, int idGrupo)
        {
            var grupo = _context.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new KeyNotFoundException("Grupo não encontrado.");
            }

            if (grupo.IdAdmin != idLogado)
            {
                throw new UnauthorizedAccessException("Apenas o administrador pode encerrar a república.");
            }

            var moradoresExtras = _context.Pertences
                .Where(p => p.IdGrupo == idGrupo && p.IdUsuario != idLogado)
                .ToList();

            if (moradoresExtras.Any())
            {
                var parcelasDosMoradores = _context.Parcelas
                .Where(p => p.IdDespesaNavigation.IdGrupo == idGrupo
                         && p.IdDespesaNavigation.Ativo == true
                         && p.IdUsuario != idLogado);

                bool temDividaOuAnalise = parcelasDosMoradores.Any(p =>
                    p.Status == StatusParcela.PENDENTE ||
                    p.Status == StatusParcela.ATRASADO ||
                    p.Status == StatusParcela.EM_ANALISE);

                if (temDividaOuAnalise)
                {
                    throw new InvalidOperationException("Não é possível encerrar a república. Existem moradores com parcelas pendentes, atrasadas ou em análise. Quite todas as contas primeiro.");
                }

                bool temParcelaPaga = parcelasDosMoradores.Any(p => p.Status == StatusParcela.PAGO);

                if (temParcelaPaga)
                {
                    throw new InvalidOperationException("Não é possível encerrar a república com moradores que possuem histórico financeiro pago. Você deve remover esses moradores do grupo primeiro.");
                }

                _context.Pertences.RemoveRange(moradoresExtras);
            }

            grupo.Ativo = false;

            var despesasDoGrupo = _context.Despesas.Where(d => d.IdGrupo == idGrupo && d.Ativo == true).ToList();

            foreach (var despesa in despesasDoGrupo)
            {
                despesa.Ativo = false;
            }

            _context.SaveChanges();

            return "República encerrada com sucesso! Todas as despesas atreladas foram arquivadas.";
        }
    }
}