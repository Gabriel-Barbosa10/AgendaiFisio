using System.Collections.Generic;
using AgendaiFisio.Models;

namespace AgendaiFisio.Data.Repositories
{
    public interface IUsuarioRepository
    {
        Usuario ObterPorEmail(string email);
        bool ExisteEmail(string email);
        bool ExisteCpf(string cpf);
        void Criar(Usuario usuario);
        List<Terapeuta> ListarTerapeutas();
    }
}
