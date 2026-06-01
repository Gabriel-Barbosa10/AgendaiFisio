using System;
using AgendaiFisio.Services;
using AgendaiFisio.Models;
using AgendaiFisio.Data;
using AgendaiFisio.Data.Repositories;

namespace AgendaiFisio
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("=======================================");
            Console.WriteLine("    BEM VINDO AO AGENDAI FISIO");
            Console.WriteLine("=======================================");

            // Dependências
            IUsuarioRepository usuarioRepository = new UsuarioRepository();
            IAgendamentoRepository agendamentoRepository = new AgendamentoRepository();
            IProntuarioRepository prontuarioRepository = new ProntuarioRepository();

            AuthService authService = new AuthService(usuarioRepository);

            while (true)
            {
                Console.WriteLine("\n--- MENU PRINCIPAL ---");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Novo Usuário");
                Console.WriteLine("3. Sair");
                Console.Write("Escolha uma opção: ");
                
                string opcao = Console.ReadLine();

                if (opcao == "1")
                {
                    Usuario usuarioLogado = authService.Login();
                    
                    if (usuarioLogado != null)
                    {
                        IMenuService menuService = CriarMenuService(usuarioLogado, usuarioRepository, agendamentoRepository, prontuarioRepository);
                        
                        if (menuService != null)
                        {
                            menuService.Menu();
                        }
                    }
                }
                else if (opcao == "2")
                {
                    authService.NovoUsuario();
                }
                else if (opcao == "3")
                {
                    Console.WriteLine("Saindo do sistema...");
                    break;
                }
                else
                {
                    Console.WriteLine("Opção inválida.");
                }
            }
        }

        private static IMenuService CriarMenuService(Usuario usuario, IUsuarioRepository usuarioRepo, IAgendamentoRepository agendamentoRepo, IProntuarioRepository prontuarioRepo)
        {
            if (usuario.TipoPerfil == "PACIENTE")
            {
                return new PacienteService(usuario, usuarioRepo, agendamentoRepo);
            }
            else if (usuario.TipoPerfil == "TERAPEUTA")
            {
                return new TerapeutaService(usuario, agendamentoRepo, prontuarioRepo);
            }
            return null;
        }
    }
}
