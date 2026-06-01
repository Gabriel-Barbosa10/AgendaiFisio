using System;
using System.Collections.Generic;
using AgendaiFisio.Models;
using AgendaiFisio.Data.Repositories;

namespace AgendaiFisio.Services
{
    public class PacienteService : IMenuService
    {
        private readonly Usuario _paciente;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAgendamentoRepository _agendamentoRepository;

        public PacienteService(Usuario paciente, IUsuarioRepository usuarioRepository, IAgendamentoRepository agendamentoRepository)
        {
            _paciente = paciente;
            _usuarioRepository = usuarioRepository;
            _agendamentoRepository = agendamentoRepository;
        }

        public void Menu()
        {
            while (true)
            {
                // Limpa o terminal toda vez que o menu volta a ser exibido
                Console.Clear();

                Console.WriteLine($"\n--- BEM VINDO, {_paciente.Nome.ToUpper()} (PACIENTE) ---");
                Console.WriteLine("1. Agendar Consulta");
                Console.WriteLine("2. Minhas Consultas");
                Console.WriteLine("3. Sair / Logout");
                Console.Write("Escolha uma opção: ");
                string opcao = Console.ReadLine();

                if (opcao == "1") AgendarConsulta();
                else if (opcao == "2") MinhasConsultas();
                else if (opcao == "3") break;
                else
                {
                    Console.WriteLine("[ERRO] Opção inválida. Pressione qualquer tecla para tentar novamente...");
                    Console.ReadKey(true);
                }
            }
        }

        private void AgendarConsulta()
        {
            Console.Clear();
            Console.WriteLine("\n--- AGENDAR CONSULTA ---");
            Console.WriteLine("(Pressione ENTER vazio a qualquer momento para cancelar e voltar ao menu)\n");
            
            // 1. LISTAR TERAPEUTAS DISPONÍVEIS
            List<Terapeuta> terapeutas = _usuarioRepository.ListarTerapeutas();
            
            Console.WriteLine("Terapeutas Disponíveis:");
            var terapeutasValidos = new List<int>();

            foreach (var terapeuta in terapeutas)
            {
                terapeutasValidos.Add(terapeuta.IdUsuario);
                Console.WriteLine($"ID: {terapeuta.IdUsuario} | Nome: {terapeuta.Nome} | CREFITO: {terapeuta.Crefito}");
            }

            if (terapeutas.Count == 0)
            {
                Console.WriteLine("Nenhum terapeuta disponível no sistema. Pressione qualquer tecla para voltar...");
                Console.ReadKey(true);
                return;
            }

            int idTerapeuta;
            while (true)
            {
                Console.Write("\nDigite o ID do terapeuta desejado: ");
                string inputTerapeuta = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(inputTerapeuta)) return; // Volta para o menu limpando a tela

                if (int.TryParse(inputTerapeuta, out idTerapeuta) && terapeutasValidos.Contains(idTerapeuta))
                {
                    break;
                }
                Console.WriteLine("[ERRO] ID inválido. Escolha um ID da lista acima ou pressione ENTER para sair.");
            }

            // 2. GERAR OS PRÓXIMOS 7 DIAS A PARTIR DE HOJE
            List<DateTime> proximosDias = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                proximosDias.Add(DateTime.Now.AddDays(i));
            }

            // 3. EXIBIR E SELECIONAR A DATA POR NÚMERO
            Console.WriteLine("\nSelecione uma data para o agendamento:");
            for (int i = 0; i < proximosDias.Count; i++)
            {
                string labelHoje = (i == 0) ? " [Hoje]" : "";
                string diaSemana = proximosDias[i].ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
                diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);

                Console.WriteLine($"[{i + 1}] - {diaSemana} - {proximosDias[i]:dd/MM/yyyy}{labelHoje}");
            }

            int opcaoData;
            while (true)
            {
                Console.Write("Digite o número da data desejada: ");
                string inputData = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inputData)) return; // Volta para o menu limpando a tela

                if (int.TryParse(inputData, out opcaoData) && opcaoData >= 1 && opcaoData <= 7)
                {
                    break;
                }
                Console.WriteLine("[ERRO] Opção inválida. Digite um número de 1 a 7 ou pressione ENTER para sair.");
            }

            DateTime dataConsulta = proximosDias[opcaoData - 1];

            // 4. BUSCAR COMPROMISSOS DO TERAPEUTA SELECIONADO NAQUELA DATA
            List<int> horasOcupadas = _agendamentoRepository.ObterHorariosOcupadosDoTerapeuta(idTerapeuta, dataConsulta);

            // 5. EXIBIR VITRINE DE HORÁRIOS PARA O PACIENTE
            Console.WriteLine($"\nHorários para o dia {dataConsulta:dd/MM/yyyy}:");
            int horaInicial = 10;
            int horaFinal = 20;

            for (int hora = horaInicial; hora <= horaFinal; hora++)
            {
                if (horasOcupadas.Contains(hora))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" [X] {hora}:00 - Indisponível");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" [ ] {hora}:00 - Disponível");
                    Console.ResetColor();
                }
            }

            // 6. SELEÇÃO E VALIDAÇÃO DA HORA
            int horaConsulta;
            while (true)
            {
                Console.Write($"\nDigite a hora que deseja agendar ({horaInicial} a {horaFinal}): ");
                string inputHora = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inputHora)) return; // Volta para o menu limpando a tela

                if (!int.TryParse(inputHora, out horaConsulta) || horaConsulta < horaInicial || horaConsulta > horaFinal)
                {
                    Console.WriteLine($"[ERRO] Por favor, digite uma hora válida entre {horaInicial} e {horaFinal}.");
                    continue;
                }

                if (horasOcupadas.Contains(horaConsulta))
                {
                    Console.WriteLine("[ERRO] Este horário não está livre. Escolha uma opção marcada como Disponível.");
                    continue;
                }

                break;
            }

            DateTime dataHoraCompleta = new DateTime(dataConsulta.Year, dataConsulta.Month, dataConsulta.Day, horaConsulta, 0, 0);

            Console.Write("\nDescreva brevemente os seus sintomas para o terapeuta: ");
            string sintomas = Console.ReadLine()?.Trim();

            // 7. SALVAR AGENDAMENTO
            _agendamentoRepository.CriarAgendamento(_paciente.IdUsuario, idTerapeuta, dataConsulta, dataHoraCompleta, "CONSULTA", "PENDENTE", sintomas);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCESSO] Consulta agendada com sucesso!");
            Console.ResetColor();
            
            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey(true);
        }

        private void MinhasConsultas()
        {
            Console.Clear();
            Console.WriteLine("\n--- MINHAS CONSULTAS ---");

            List<AgendamentoDto> agendamentos = _agendamentoRepository.ObterConsultasDoPaciente(_paciente.IdUsuario);

            bool achou = false;
            foreach (var ag in agendamentos)
            {
                achou = true;
                Console.WriteLine($"Consulta #{ag.IdAgendamento} | Data/Hora: {ag.HoraAgenda:dd/MM/yyyy HH:mm} | Terapeuta: {ag.NomePessoa} | Status: {ag.Status}");
            }

            if (!achou)
            {
                Console.WriteLine("Você não possui nenhuma consulta agendada.");
            }

            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey(true);
        }
    }
}