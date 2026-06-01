using System;
using System.IO;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AgendaiFisio.Models;
using AgendaiFisio.Data.Repositories;

namespace AgendaiFisio.Services
{
    public class TerapeutaService : IMenuService
    {
        private readonly Usuario _terapeuta;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IProntuarioRepository _prontuarioRepository;

        public TerapeutaService(Usuario terapeuta, IAgendamentoRepository agendamentoRepository, IProntuarioRepository prontuarioRepository)
        {
            _terapeuta = terapeuta;
            _agendamentoRepository = agendamentoRepository;
            _prontuarioRepository = prontuarioRepository;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void Menu()
        {
            while (true)
            {
                // Limpa o terminal ao exibir ou retornar para o Menu
                Console.Clear();

                Console.WriteLine($"\n--- BEM VINDO, {_terapeuta.Nome.ToUpper()} (TERAPEUTA) ---");
                Console.WriteLine("1. Ver Minhas Consultas");
                Console.WriteLine("2. Bloquear Horário");
                Console.WriteLine("3. Prontuário e Nota de Evolução");
                Console.WriteLine("4. Gerar Relatório Mensal (PDF)");
                Console.WriteLine("5. Sair / Logout");
                Console.Write("Escolha uma opção: ");
                string opcao = Console.ReadLine();

                if (opcao == "1") VerConsultas();
                else if (opcao == "2") BloquearHorario();
                else if (opcao == "3") ProntuarioEvolucao();
                else if (opcao == "4") GerarRelatorioPdf();
                else if (opcao == "5") break;
                else 
                {
                    Console.WriteLine("[ERRO] Opção inválida. Pressione qualquer tecla para tentar novamente...");
                    Console.ReadKey(true);
                }
            }
        }

        private void VerConsultas()
        {
            Console.Clear();
            Console.WriteLine("\n--- MINHAS CONSULTAS ---");
            
            List<AgendamentoDto> agendamentos = _agendamentoRepository.ObterAgendamentosDoTerapeuta(_terapeuta.IdUsuario);

            bool achou = false;
            foreach (var ag in agendamentos)
            {
                achou = true;

                if (ag.TipoRegistro == "BLOQUEIO" || ag.IdPaciente == _terapeuta.IdUsuario) 
                {
                    Console.WriteLine($"[BLOQUEIO DE AGENDA] #{ag.IdAgendamento} | Data/Hora: {ag.HoraAgenda:dd/MM/yyyy HH:mm}");
                }
                else
                {
                    Console.WriteLine($"Consulta #{ag.IdAgendamento} | Data/Hora: {ag.HoraAgenda:dd/MM/yyyy HH:mm} | Paciente: {ag.NomePessoa} | Status: {ag.Status} | Sintomas: {ag.DescricaoSintomas}");
                }
            }

            if (!achou)
            {
                Console.WriteLine("Você não possui nenhuma consulta agendada ou realizada.");
            }

            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey(true);
        }

        private void BloquearHorario()
        {
            Console.Clear();
            Console.WriteLine("\n--- BLOQUEAR HORÁRIO ---");
            Console.WriteLine("(Pressione ENTER vazio a qualquer momento para cancelar e voltar ao menu)\n");

            // 1. GERAR OS PRÓXIMOS 7 DIAS A PARTIR DE HOJE
            List<DateTime> proximosDias = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                proximosDias.Add(DateTime.Now.AddDays(i));
            }

            // 2. EXIBIR E SELECIONAR A DATA
            Console.WriteLine("Selecione uma data para o bloqueio:");
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

            DateTime dataBloqueio = proximosDias[opcaoData - 1];

            // 3. BUSCAR DETALHES DOS HORÁRIOS NO BANCO
            Dictionary<int, string> statusHorarios = _agendamentoRepository.ObterStatusHorariosDoBanco(dataBloqueio);

            Console.WriteLine($"\nHorários para o dia {dataBloqueio:dd/MM/yyyy}:");
            
            int horaInicial = 10;
            int horaFinal = 20;

            for (int hora = horaInicial; hora <= horaFinal; hora++)
            {
                if (statusHorarios.ContainsKey(hora))
                {
                    string info = statusHorarios[hora];

                    if (info.Equals("BLOQUEADO", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($" [B] {hora}:00 - JÁ BLOQUEADO");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" [X] {hora}:00 - OCUPADO (Paciente: {info})");
                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" [ ] {hora}:00 - Disponível");
                    Console.ResetColor();
                }
            }

            // 4. SELEÇÃO E VALIDAÇÃO DA HORA
            int horaBloqueio;
            while (true)
            {
                Console.Write($"\nDigite o número da hora que deseja bloquear ({horaInicial} a {horaFinal}): ");
                string inputHora = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inputHora)) return; // Volta para o menu limpando a tela

                if (!int.TryParse(inputHora, out horaBloqueio) || horaBloqueio < horaInicial || horaBloqueio > horaFinal)
                {
                    Console.WriteLine($"[ERRO] Por favor, digite uma hora válida entre {horaInicial} e {horaFinal}.");
                    continue;
                }

                if (statusHorarios.ContainsKey(horaBloqueio))
                {
                    Console.WriteLine("[ERRO] Este horário não está livre. Escolha um horário com '[ ] Disponível'.");
                    continue;
                }

                break;
            }

            // 5. SALVAR NO BANCO
            DateTime dataHoraCompleta = new DateTime(dataBloqueio.Year, dataBloqueio.Month, dataBloqueio.Day, horaBloqueio, 0, 0);
            _agendamentoRepository.CriarAgendamento(null, _terapeuta.IdUsuario, dataBloqueio, dataHoraCompleta, "BLOQUEIO", "CONFIRMADO", "Horário bloqueado pelo terapeuta.");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[SUCESSO] Horário das {horaBloqueio}:00 no dia {dataBloqueio:dd/MM/yyyy} bloqueado!");
            Console.ResetColor();

            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey(true);
        }

        private void ProntuarioEvolucao()
        {
            Console.Clear();
            Console.WriteLine("\n--- PRONTUÁRIO E EVOLUÇÃO ---");
            Console.WriteLine("(Pressione ENTER vazio a qualquer momento para cancelar e voltar ao menu)\n");

            Console.Write("Digite o ID da Consulta (Agendamento) que deseja atualizar: ");
            string inputAgendamento = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputAgendamento)) return;

            if (!int.TryParse(inputAgendamento, out int idAgendamento))
            {
                Console.WriteLine("[ERRO] ID Inválido. Pressione qualquer tecla para voltar...");
                Console.ReadKey(true);
                return;
            }

            AgendamentoInfo infoAgendamento = _agendamentoRepository.ObterInfoAgendamento(idAgendamento, _terapeuta.IdUsuario);
            
            if (infoAgendamento == null)
            {
                Console.WriteLine("[ERRO] Consulta não encontrada ou não pertence a você. Pressione qualquer tecla...");
                Console.ReadKey(true);
                return;
            }

            if (infoAgendamento.TipoRegistro == "BLOQUEIO" || !infoAgendamento.IdPaciente.HasValue)
            {
                Console.WriteLine("[ERRO] Esta ID de agendamento pertence a um bloqueio de agenda, não a uma consulta com paciente.");
                Console.WriteLine("Pressione qualquer tecla para voltar...");
                Console.ReadKey(true);
                return;
            }

            if (infoAgendamento.Status != "REALIZADO")
            {
                _agendamentoRepository.MarcarComoRealizado(idAgendamento);
                Console.WriteLine("[INFO] Consulta marcada como REALIZADA.");
            }

            int? idProntuarioExistente = _prontuarioRepository.ObterIdProntuario(infoAgendamento.IdPaciente.Value, _terapeuta.IdUsuario);
            int idProntuario = 0;

            if (!idProntuarioExistente.HasValue)
            {
                Console.WriteLine("[INFO] Nenhum prontuário anterior encontrado para este paciente. Criando um novo...");
                Console.Write("Digite a descrição base do prontuário: ");
                string desc = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(desc)) return;

                idProntuario = _prontuarioRepository.CriarProntuario(infoAgendamento.IdPaciente.Value, _terapeuta.IdUsuario, desc);
            }
            else
            {
                idProntuario = idProntuarioExistente.Value;
            }

            Console.WriteLine("\n--- NOVA NOTA DE EVOLUÇÃO ---");
            Console.Write("Digite o texto da evolução do paciente: ");
            string textoEvolucao = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(textoEvolucao)) return;

            _prontuarioRepository.AdicionarNotaEvolucao(idProntuario, _terapeuta.IdUsuario, idAgendamento, textoEvolucao);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCESSO] Prontuário e Nota de Evolução salvos com sucesso!");
            Console.ResetColor();

            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey(true);
        }

        private void GerarRelatorioPdf()
        {
            Console.Clear();
            Console.WriteLine("\n--- GERAR RELATÓRIO MENSAL ---");
            Console.WriteLine("(Pressione ENTER vazio para cancelar)\n");

            Console.Write("Digite o Mês (1-12): ");
            string inputMes = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputMes) || !int.TryParse(inputMes, out int mes) || mes < 1 || mes > 12) return;

            Console.Write("Digite o Ano (ex: 2026): ");
            string inputAno = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputAno) || !int.TryParse(inputAno, out int ano)) return;

            string filePath = Path.Combine(Environment.CurrentDirectory, $"Relatorio_{_terapeuta.Nome.Replace(" ", "_")}_{mes}_{ano}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text($"Relatório de Consultas - {_terapeuta.Nome} ({mes:D2}/{ano})")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Text("Abaixo está o detalhamento de todas as consultas agendadas e realizadas neste mês.");
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Data/Hora").Bold();
                                header.Cell().Text("Paciente").Bold();
                                header.Cell().Text("Status").Bold();
                            });

                            List<AgendamentoDto> agendamentos = _agendamentoRepository.ObterAgendamentosDoTerapeutaPorMes(_terapeuta.IdUsuario, mes, ano);

                            foreach (var ag in agendamentos)
                            {
                                string pac = ag.NomePessoa;
                                string stat = ag.Status;

                                if (ag.TipoRegistro == "BLOQUEIO" || ag.IdPaciente == _terapeuta.IdUsuario)
                                {
                                    pac = "BLOQUEIO DE AGENDA";
                                    stat = "-";
                                }

                                table.Cell().Text(ag.HoraAgenda.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Text(pac);
                                table.Cell().Text(stat);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }) 
            .GeneratePdf(filePath); 

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[SUCESSO] Relatório PDF gerado com sucesso em: {filePath}");
            Console.ResetColor();

            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey(true);
        }
    }
}