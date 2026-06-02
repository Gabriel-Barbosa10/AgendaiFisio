using BCrypt.Net;

namespace AgendaiFisio
{
    public static class Security
    {
        public static string CriptografarSenha(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
        }

        public static bool VerificarSenha(string senhaDigitada, string hashDoBanco)
        {
            return BCrypt.Net.BCrypt.Verify(senhaDigitada, hashDoBanco);
        }

        
    }
}