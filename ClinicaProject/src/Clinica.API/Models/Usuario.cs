using System;

namespace AgendaiFisioConsole.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cpf { get; set; }
        public string Senha { get; set; }
        public string Crefito { get; set; }
        public string TipoPerfil { get; set; } // PACIENTE ou TERAPEUTA
        public bool AceiteLgpd { get; set; }
    }
    
}
