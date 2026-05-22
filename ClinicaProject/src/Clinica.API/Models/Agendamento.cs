using System;

namespace AgendaiFisioConsole.Models
{
    public class Agendamento
    {
        public int IdAgendamento { get; set; }
        public int IdPaciente { get; set; }
        public int IdTerapeuta { get; set; }
        public DateTime DataHora { get; set; }
        public string DescricaoSintomas { get; set; }
        public string Status { get; set; } // PENDENTE, CONFIRMADO, CANCELADO, REALIZADO, NO_SHOW
    }
}
