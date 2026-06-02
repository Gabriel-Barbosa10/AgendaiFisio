using System;

namespace AgendaiFisioAPI.Models
{
    public class Agendamento
    {
        public int IdAgendamento { get; set; }
        public int IdPaciente { get; set; }
        public int IdTerapeuta { get; set; }
        public DateTime DataAgenda { get; set; }
        public DateTime HoraAgenda { get; set; }
        public string TipoRegistro { get; set; }
        public string DescricaoSintomas { get; set; }
        public string Status { get; set; } // PENDENTE, CONFIRMADO, CANCELADO, REALIZADO, NO_SHOW
    }
}
