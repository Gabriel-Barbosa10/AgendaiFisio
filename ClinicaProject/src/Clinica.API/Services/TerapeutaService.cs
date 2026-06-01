using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AgendaiFisio.Models;
using AgendaiFisio.Data;

namespace AgendaiFisio.Services
{
    public class TerapeutaService
    {
        private readonly Usuario _terapeuta;

        public TerapeutaService(Usuario terapeuta)
        {
            _terapeuta = terapeuta;
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
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                SELECT a.id_agendamento, a.hora_agenda, p.nome, a.status, a.descricao_sintomas, a.id_paciente, a.tipo_registro
                FROM agendamento a
                LEFT JOIN usuario p ON a.id_paciente = p.id_usuario
                WHERE a.id_terapeuta = @terapeuta
                ORDER BY a.hora_agenda ASC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
            using var reader = cmd.ExecuteReader();

            bool achou = false;
            while (reader.Read())
            {
                achou = true;
                int id = reader.GetInt32(0);
                DateTime horaAgenda = reader.GetDateTime(1);
                string paciente = reader.IsDBNull(2) ? "Nenhum" : reader.GetString(2);
                string status = reader.GetString(3);
                string sintomas = reader.IsDBNull(4) ? "Nenhum" : reader.GetString(4);
                int? idPaciente = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                string tipoRegistro = reader.GetString(6);

                if (tipoRegistro == "BLOQUEIO" || idPaciente == _terapeuta.IdUsuario) 
                {
                    Console.WriteLine($"[BLOQUEIO DE AGENDA] #{id} | Data/Hora: {horaAgenda:dd/MM/yyyy HH:mm}");
                }
                else
                {
                    Console.WriteLine($"Consulta #{id} | Data/Hora: {horaAgenda:dd/MM/yyyy HH:mm} | Paciente: {paciente} | Status: {status} | Sintomas: {sintomas}");
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
            Dictionary<int, string> statusHorarios = ObterStatusHorariosDoBanco(dataBloqueio);

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
            SalvarBloqueioNoBanco(dataBloqueio, horaBloqueio);
            
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

            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            
            string qCheck = "SELECT id_paciente, status, tipo_registro FROM agendamento WHERE id_agendamento = @id_agendamento AND id_terapeuta = @id_terapeuta";
            using var cmdCheck = new SqlCommand(qCheck, conn);
            cmdCheck.Parameters.AddWithValue("@id_agendamento", idAgendamento);
            cmdCheck.Parameters.AddWithValue("@id_terapeuta", _terapeuta.IdUsuario);
            
            int? idPaciente = null;
            string status = "";
            string tipoRegistro = "";

            using (var reader = cmdCheck.ExecuteReader())
            {
                if (reader.Read())
                {
                    idPaciente = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                    status = reader.GetString(1);
                    tipoRegistro = reader.GetString(2);
                }
                else
                {
                    Console.WriteLine("[ERRO] Consulta não encontrada ou não pertence a você. Pressione qualquer tecla...");
                    Console.ReadKey(true);
                    return;
                }
            }

            if (tipoRegistro == "BLOQUEIO" || !idPaciente.HasValue)
            {
                Console.WriteLine("[ERRO] Esta ID de agendamento pertence a um bloqueio de agenda, não a uma consulta com paciente.");
                Console.WriteLine("Pressione qualquer tecla para voltar...");
                Console.ReadKey(true);
                return;
            }

            if (status != "REALIZADO")
            {
                string qUpd = "UPDATE agendamento SET status = 'REALIZADO' WHERE id_agendamento = @id";
                using var cmdUpd = new SqlCommand(qUpd, conn);
                cmdUpd.Parameters.AddWithValue("@id", idAgendamento);
                cmdUpd.ExecuteNonQuery();
                Console.WriteLine("[INFO] Consulta marcada como REALIZADA.");
            }

            string qPront = "SELECT id_prontuario FROM prontuario WHERE id_paciente = @paciente AND id_terapeuta = @terapeuta";
            using var cmdPront = new SqlCommand(qPront, conn);
            cmdPront.Parameters.AddWithValue("@paciente", idPaciente.Value); 
            cmdPront.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
            
            object objPront = cmdPront.ExecuteScalar();
            int idProntuario = 0;

            if (objPront == null)
            {
                Console.WriteLine("[INFO] Nenhum prontuário anterior encontrado para este paciente. Criando um novo...");
                Console.Write("Digite a descrição base do prontuário: ");
                string desc = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(desc)) return;

                string qInsPront = "INSERT INTO prontuario (id_paciente, id_terapeuta, versao, descricao) OUTPUT INSERTED.id_prontuario VALUES (@paciente, @terapeuta, 1, @desc)";
                using var cmdIns = new SqlCommand(qInsPront, conn);
                cmdIns.Parameters.AddWithValue("@paciente", idPaciente.Value);
                cmdIns.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
                cmdIns.Parameters.AddWithValue("@desc", desc);
                idProntuario = (int)cmdIns.ExecuteScalar();
            }
            else
            {
                idProntuario = (int)objPront;
            }

            Console.WriteLine("\n--- NOVA NOTA DE EVOLUÇÃO ---");
            Console.Write("Digite o texto da evolução do paciente: ");
            string textoEvolucao = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(textoEvolucao)) return;

            string qNota = "INSERT INTO nota_evolucao (id_prontuario, id_terapeuta, id_agendamento, texto_evolucao) VALUES (@pront, @terapeuta, @agen, @texto)";
            using var cmdNota = new SqlCommand(qNota, conn);
            cmdNota.Parameters.AddWithValue("@pront", idProntuario);
            cmdNota.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
            cmdNota.Parameters.AddWithValue("@agen", idAgendamento);
            cmdNota.Parameters.AddWithValue("@texto", textoEvolucao);
            cmdNota.ExecuteNonQuery();

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

                    page.Page(p => p.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
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

                            using var conn = DatabaseConnection.GetConnection();
                            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                            
                            // QUERY CORRIGIDA PARA USAR 'hora_agenda' e 'data_agenda'
                            string query = @"
                                SELECT a.hora_agenda, p.nome, a.status, a.id_paciente, a.tipo_registro
                                FROM agendamento a
                                LEFT JOIN usuario p ON a.id_paciente = p.id_usuario
                                WHERE a.id_terapeuta = @terapeuta AND MONTH(a.data_agenda) = @mes AND YEAR(a.data_agenda) = @ano
                                ORDER BY a.hora_agenda ASC";

                            using var cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
                            cmd.Parameters.AddWithValue("@mes", mes);
                            cmd.Parameters.AddWithValue("@ano", ano);
                            using var reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                DateTime data = reader.GetDateTime(0);
                                string pac = reader.IsDBNull(1) ? "Nenhum" : reader.GetString(1);
                                string stat = reader.GetString(2);
                                int? idPac = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                                string tipoReg = reader.GetString(4);

                                if (tipoReg == "BLOQUEIO" || idPac == _terapeuta.IdUsuario)
                                {
                                    pac = "BLOQUEIO DE AGENDA";
                                    stat = "-";
                                }

                                table.Cell().Text(data.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Text(pac);
                                table.Cell().Text(stat);
                            }
                        });
                    }));

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

        private void SalvarBloqueioNoBanco(DateTime data, int hora)
        {
            DateTime dataHoraCompleta = new DateTime(data.Year, data.Month, data.Day, hora, 0, 0);

            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                INSERT INTO agendamento (id_paciente, id_terapeuta, data_agenda, hora_agenda, tipo_registro, status, descricao_sintomas) 
                VALUES (NULL, @idTerapeuta, @dataAgenda, @horaAgenda, 'BLOQUEIO', 'CONFIRMADO', 'Horário bloqueado pelo terapeuta.')";
            
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@idTerapeuta", _terapeuta.IdUsuario);
            cmd.Parameters.AddWithValue("@dataAgenda", data.Date); 
            cmd.Parameters.AddWithValue("@horaAgenda", dataHoraCompleta); 

            cmd.ExecuteNonQuery();
        }

        private Dictionary<int, string> ObterStatusHorariosDoBanco(DateTime data)
        {
            var statusMap = new Dictionary<int, string>();

            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                SELECT DATEPART(HOUR, a.hora_agenda), a.tipo_registro, u.nome 
                FROM agendamento a
                LEFT JOIN usuario u ON a.id_paciente = u.id_usuario
                WHERE CAST(a.data_agenda AS DATE) = @data AND a.status != 'CANCELADO'";
            
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@data", data.Date);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int hora = reader.GetInt32(0);
                string tipo = reader.GetString(1); 
                string nomePaciente = reader.IsDBNull(2) ? "" : reader.GetString(2);

                if (tipo == "BLOQUEIO")
                {
                    statusMap[hora] = "BLOQUEADO";
                }
                else
                {
                    statusMap[hora] = nomePaciente; 
                }
            }

            return statusMap;
        }
    }
}