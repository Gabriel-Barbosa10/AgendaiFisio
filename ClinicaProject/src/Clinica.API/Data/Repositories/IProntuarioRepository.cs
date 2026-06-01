using System;

namespace AgendaiFisio.Data.Repositories
{
    public interface IProntuarioRepository
    {
        int? ObterIdProntuario(int idPaciente, int idTerapeuta);
        int CriarProntuario(int idPaciente, int idTerapeuta, string descricao);
        void AdicionarNotaEvolucao(int idProntuario, int idTerapeuta, int idAgendamento, string textoEvolucao);
    }
}
