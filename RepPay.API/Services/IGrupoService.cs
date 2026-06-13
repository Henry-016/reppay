using RepPay.API.DTOs;
using System.Collections.Generic;

namespace RepPay.API.Services
{
    public interface IGrupoService
    {
        GrupoCriadoResponseDTO CriarGrupo(int idAdmin, GrupoRequestDTO request);
        string EntrarNoGrupo(int idUsuario, EntrarGrupoRequestDTO request);
        List<MeuGrupoResponseDTO> GetMeusGrupos(int idLogado);
        MeuGrupoResponseDTO GetGrupoPorId(int idLogado, int idGrupo);
        List<MembroResponseDTO> GetMembrosDoGrupo(int idLogado, int idGrupo);
        string SairDoGrupo(int idLogado, int idGrupo);
        string ExpulsarMorador(int idLogado, int idGrupo, int idMorador);
        string TransferirAdmin(int idLogado, int idGrupo, int idNovoAdmin);
        ProximaContaResponseDTO? ObterProximaContaGrupo(int idLogado, int idGrupo);
        string DeletarGrupo(int idLogado, int idGrupo);
    }
}