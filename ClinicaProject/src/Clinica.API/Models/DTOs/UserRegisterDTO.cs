namespace Clinica.API.Models.DTOs
{
    public class UserRegisterDTO
    {
        public string  Nome       { get; set; } = string.Empty;
        public string  Email      { get; set; } = string.Empty;
        public string  Password   { get; set; } = string.Empty;
        public string? Cpf        { get; set; }
        public bool    AceiteLgpd { get; set; } = false;
        public string? TipoPerfil { get; set; } = "PACIENTE";
    }
}