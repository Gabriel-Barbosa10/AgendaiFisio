using System;

namespace AgendaiFisio.Models
{
    public class Terapeuta : Usuario
    {
        public string Crefito { get; set; }

        public Terapeuta()
        {
            TipoPerfil = "TERAPEUTA";
        }
    }
}
