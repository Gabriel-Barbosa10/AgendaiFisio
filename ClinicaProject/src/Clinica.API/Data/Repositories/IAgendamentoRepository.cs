using System;
using System.Collections.Generic;
using AgendaiFisio.Models;

namespace AgendaiFisio.Data.Repositories
{
    public interface IAgendamentoRepository
    {
        List<AgendamentoDto> ObterAgendamentosDoTerapeuta(int idTerapeuta);
        List<AgendamentoDto> ObterAgendamentosDoTerapeutaPorMes(int idTerapeuta, int mes, int ano);
        List<AgendamentoDto> ObterConsultasDoPaciente(int idPaciente);
        void CriarAgendamento(int? idPaciente, int idTerapeuta, DateTime dataAgenda, DateTime horaAgenda, string tipoRegistro, string status, string sintomas);
        Dictionary<int, string> ObterStatusHorariosDoBanco(DateTime data);
        List<int> ObterHorariosOcupadosDoTerapeuta(int idTerapeuta, DateTime data);
        AgendamentoInfo ObterInfoAgendamento(int idAgendamento, int idTerapeuta);
        void MarcarComoRealizado(int idAgendamento);
    }

    public class AgendamentoDto
    {
        public int IdAgendamento { get; set; }
        public DateTime HoraAgenda { get; set; }
        public string NomePessoa { get; set; } // Paciente ou Terapeuta
        public string Status { get; set; }
        public string DescricaoSintomas { get; set; }
        public int? IdPaciente { get; set; }
        public string TipoRegistro { get; set; }
    }

    public class AgendamentoInfo
    {
        public int? IdPaciente { get; set; }
        public string Status { get; set; }
        public string TipoRegistro { get; set; }
    }
}
