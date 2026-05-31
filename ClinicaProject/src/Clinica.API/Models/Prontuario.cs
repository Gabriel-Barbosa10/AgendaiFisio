using System;

namespace AgendaiFisio.Models
{
    public class Prontuario
    {
        public int IdProntuario { get; set; }
        public int IdPaciente { get; set; }
        public int IdTerapeuta { get; set; }
        public int Versao { get; set; }
        public string Descricao { get; set; }
    }
}
