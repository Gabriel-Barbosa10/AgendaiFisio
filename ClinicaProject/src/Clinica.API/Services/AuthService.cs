using System;
using System.Text.RegularExpressions;
using System.Threading;
using AgendaiFisio.Models;
using AgendaiFisio.Data.Repositories;

namespace AgendaiFisio.Services
{
    public class AuthService
    {
        private static int falhasLogin = 0;
        private static DateTime? bloqueadoAte = null;
        private readonly IUsuarioRepository _usuarioRepository;

        public AuthService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public Usuario Login()
        {
            if (bloqueadoAte.HasValue && DateTime.Now < bloqueadoAte.Value)
            {
                AguardarDesbloqueio();
            }
            Console.Clear();
            Console.WriteLine("\n--- LOGIN ---");
            Console.Write("Email: ");
            string email = Console.ReadLine()?.Trim();
            Console.Write("Senha: ");
            string senha = LerSenha();

            Usuario usuario = _usuarioRepository.ObterPorEmail(email);
            
            if (usuario != null)
            {
                string hashDoBanco = usuario.Senha;

                if (Security.VerificarSenha(senha, hashDoBanco))
                {
                    falhasLogin = 0; 
                    return usuario;
                }
            }

            // 4. Se o e-mail não existir OU a senha estiver errada, cai aqui:
            falhasLogin++;
            Console.WriteLine("\n[ERRO] Email ou senha incorretos.");
            
            if (falhasLogin >= 6)
            {
                Console.WriteLine("[AVISO] Muitas tentativas falhas. Bloqueando por 3 minutos.");
                bloqueadoAte = DateTime.Now.AddMinutes(3);
                AguardarDesbloqueio();
            }
            else if (falhasLogin >= 3)
            {
                Console.Write("[AVISO] Você esqueceu a senha? (S/N): ");
                if (Console.ReadLine()?.ToUpper() == "S")
                {
                    Console.WriteLine("Por favor, entre em contato com o administrador do sistema para resetar sua senha.");
                }
            }
            
            return null;
        }

        private void AguardarDesbloqueio()
        {
            while (DateTime.Now < bloqueadoAte.Value)
            {
                TimeSpan restante = bloqueadoAte.Value - DateTime.Now;
                Console.Write($"\r[BLOQUEADO] Tente novamente em {restante.Minutes:D2}:{restante.Seconds:D2}   ");
                Thread.Sleep(1000);
            }
            Console.WriteLine("\nDesbloqueado! Você pode tentar novamente.");
            falhasLogin = 0;
            bloqueadoAte = null;
        }

        public void NovoUsuario()
        {
        string opcao;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n--- NOVO USUÁRIO ---");
            Console.WriteLine("(Pressione ENTER vazio a qualquer momento para cancelar e voltar)");
            Console.WriteLine("1. Sou Paciente");
            Console.WriteLine("2. Sou Terapeuta");
            Console.Write("Escolha seu perfil: ");

            opcao = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(opcao)) 
            {
                Console.WriteLine("\n[INFO] Cadastro cancelado. Voltando ao menu...");
                return; 
            }

            if (opcao == "1" || opcao == "2")
            {
                break;
            }

            Console.WriteLine("[ERRO] Opção inválida! Digite apenas 1 ou 2.\n");
        }

        string tipoPerfil = opcao == "2" ? "TERAPEUTA" : "PACIENTE";

            string nome;
            while (true)
            {
                Console.Write("Nome: ");
                nome = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    break; 
                }

                Console.WriteLine("[ERRO] O nome não pode ser vazio. Por favor, digite um nome válido.");
            }

            string email;
            while (true)
            {
                Console.Write("Email: ");
                email = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    Console.WriteLine("[ERRO] Formato de email inválido. Digite novamente.");
                    continue;
                }
                
                if (_usuarioRepository.ExisteEmail(email))
                {
                    Console.WriteLine("[ERRO] Email já cadastrado.");
                    continue;
                }
                
                break;
            }

            string cpf;
            while (true)
            {
                cpf = ConsoleInput.LerCpfComMascara();
                if (!ValidarCpf(cpf))
                {
                    Console.WriteLine("[ERRO] CPF inválido.");
                    continue;
                }

                if (_usuarioRepository.ExisteCpf(cpf))
                {
                    Console.WriteLine("[ERRO] CPF já cadastrado.");
                    continue;
                }
                break;
            }

            string senha;
            while (true)
            {
                Console.Write("Senha (mínimo 8 caracteres): ");
                senha = LerSenha();
                if (senha.Length >= 8) break;
                Console.WriteLine("[ERRO] A senha deve ter pelo menos 8 caracteres.");
            }

            string crefito = null;
            if (tipoPerfil == "TERAPEUTA")
            {
                while (true)
                {
                    Console.Write("CREFITO (ex: 123456-TO): ");
                    crefito = Console.ReadLine();
                    if (Regex.IsMatch(crefito, @"^\d{6}-[A-Za-z]{2}$")) break;
                    Console.WriteLine("[ERRO] Formato de CREFITO inválido. Use 6 números, hífen, e 2 letras.");
                }
            }

            Console.WriteLine("\n--- TERMO DE LGPD ---");
            Console.WriteLine("A AgendaiFisio coleta seus dados (Nome, CPF, Email, etc.) estritamente para");
            Console.WriteLine("fins de agendamento de consultas e prontuário médico. Seus dados são");
            Console.WriteLine("armazenados com segurança e não serão compartilhados com terceiros.");
            Console.Write("Você aceita o termo de uso dos seus dados? (S/N): ");
            bool aceite = Console.ReadLine().ToUpper() == "S";

            if (!aceite)
            {
                Console.WriteLine("Cadastro cancelado: O aceite do termo de LGPD é obrigatório.");
                return;
            }

            try
            {
                string senhaHash = Security.CriptografarSenha(senha);
                
                Usuario novoUsuario;
                if (tipoPerfil == "TERAPEUTA")
                {
                    novoUsuario = new Terapeuta
                    {
                        Nome = nome,
                        Email = email,
                        Cpf = cpf,
                        Senha = senhaHash,
                        Crefito = crefito,
                        AceiteLgpd = aceite
                    };
                }
                else
                {
                    novoUsuario = new Paciente
                    {
                        Nome = nome,
                        Email = email,
                        Cpf = cpf,
                        Senha = senhaHash,
                        AceiteLgpd = aceite
                    };
                }

                _usuarioRepository.Criar(novoUsuario);
                Console.WriteLine("\n[SUCESSO] Usuário cadastrado com sucesso!");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERRO DB] {ex.Message}");
            }
        }

        private bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            // Remove qualquer caractere que não seja número
            cpf = Regex.Replace(cpf, "[^0-9]", "");

            // CPF deve ter 11 dígitos e não pode ter todos os números iguais
            if (cpf.Length != 11 || new string(cpf[0], 11) == cpf) return false;

            // Validação do Primeiro Dígito Verificador
            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += (cpf[i] - '0') * multiplicadores1[i];
            }

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            if (cpf[9] - '0' != digito1) return false;

            // Validação do Segundo Dígito Verificador
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += (cpf[i] - '0') * multiplicadores2[i];
            }

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return cpf[10] - '0' == digito2;
        }

        private string LerSenha()
        {
            string senha = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    senha += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && senha.Length > 0)
                {
                    senha = senha.Substring(0, (senha.Length - 1));
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return senha;
        }
    }
}
