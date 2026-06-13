using RepPay.API.DTOs;
using System.Collections.Generic;

namespace RepPay.API.Services
{
    public interface IDespesaService
    {
        string CadastrarDespesa(int idLogado, DespesaRequestDTO request);
        ResumoDividasDTO GetMinhasDividas(int idLogado);
        ResumoInadimplentesDTO GetInadimplentes(int idLogado, int idGrupo);
        string PagarParcela(int idLogado, int idParcela);
        string DesfazerPagamento(int idLogado, int idParcela);
        string ValidarPagamento(int idLogado, int idParcela, ValidarPagamentoRequestDTO request);
        List<HistoricoPagoResponseDTO> GetMeuHistoricoPago(int idLogado);
        List<HistoricoGrupoDTO> GetHistoricoPagoGrupo(int idLogado, int idGrupo);
        List<AnaliseMoradorDTO> GetMinhasAnalises(int idLogado);
        List<AnaliseAdminDTO> GetAnalisesPendentesGrupo(int idLogado, int idGrupo);
        string QuitarDividaAdmin(int idLogado, int idParcela);
        string EditarDespesa(int idLogado, int idDespesa, DespesaRequestDTO request);
        string DeletarDespesa(int idLogado, int idDespesa);
    }
}