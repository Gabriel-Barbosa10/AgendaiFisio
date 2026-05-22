using System;

namespace AgendaiFisioConsole.Models
{
    public class NotaEvolucao
    {
        public int IdNota { get; set; }
        public int IdProntuario { get; set; }
        public int IdTerapeuta { get; set; }
        public int? IdAgendamento { get; set; }
        public string TextoEvolucao { get; set; }
        public DateTime DataRegistro { get; set; }
    }
}
