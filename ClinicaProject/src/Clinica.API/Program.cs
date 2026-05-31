using System;
using AgendaiFisio.Services;
using AgendaiFisio.Models;
using AgendaiFisio.Data;

namespace AgendaiFisio
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine("    BEM VINDO AO AGENDAI FISIO");
            Console.WriteLine("=======================================");

            // Testar a conexão com o banco ao iniciar
            //DatabaseConnection.TestConnection();
            
            // Inserir dados de teste caso o banco esteja vazio
            // DatabaseConnection.SeedDatabase();

            AuthService authService = new AuthService();

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
                        if (usuarioLogado.TipoPerfil == "PACIENTE")
                        {
                            var pacienteService = new PacienteService(usuarioLogado);
                            pacienteService.Menu();
                        }
                        else if (usuarioLogado.TipoPerfil == "TERAPEUTA")
                        {
                            var terapeutaService = new TerapeutaService(usuarioLogado);
                            terapeutaService.Menu();
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
    }
}
