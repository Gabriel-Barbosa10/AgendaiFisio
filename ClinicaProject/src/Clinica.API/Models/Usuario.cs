using System;

namespace AgendaiFisio.Models
{
    public abstract class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cpf { get; set; }
        public string Senha { get; set; }
        public string TipoPerfil { get; set; } // PACIENTE ou TERAPEUTA
        public bool AceiteLgpd { get; set; }
    }
}
