using System;
using System.IO;
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
                else Console.WriteLine("Opção inválida.");
            }
        }

        private void VerConsultas()
        {
            Console.WriteLine("\n--- MINHAS CONSULTAS ---");
            using var conn = DatabaseConnection.GetConnection();
            string query = @"
                SELECT a.id_agendamento, a.data_hora, p.nome, a.status, a.descricao_sintomas, a.id_paciente
                FROM agendamento a
                JOIN usuario p ON a.id_paciente = p.id_usuario
                WHERE a.id_terapeuta = @terapeuta
                ORDER BY a.data_hora ASC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
            using var reader = cmd.ExecuteReader();

            bool achou = false;
            while (reader.Read())
            {
                achou = true;
                int id = reader.GetInt32(0);
                DateTime dataHora = reader.GetDateTime(1);
                string paciente = reader.GetString(2);
                string status = reader.GetString(3);
                string sintomas = reader.IsDBNull(4) ? "Nenhum" : reader.GetString(4);
                int idPaciente = reader.GetInt32(5);

                if (idPaciente == _terapeuta.IdUsuario) 
                {
                    Console.WriteLine($"[BLOQUEIO DE AGENDA] #{id} | Data: {dataHora:dd/MM/yyyy HH:mm}");
                }
                else
                {
                    Console.WriteLine($"Consulta #{id} | Data: {dataHora:dd/MM/yyyy HH:mm} | Paciente: {paciente} | Status: {status} | Sintomas: {sintomas}");
                }
            }

            if (!achou)
            {
                Console.WriteLine("Você não possui nenhuma consulta agendada ou realizada.");
            }
        }

        private void BloquearHorario()
        {
            Console.WriteLine("\n--- BLOQUEAR HORÁRIO ---");
            Console.Write("Data do bloqueio (dd/MM/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataBloqueio))
            {
                Console.WriteLine("[ERRO] Formato de data inválido.");
                return;
            }

            if (dataBloqueio.Date < DateTime.Now.Date)
            {
                Console.WriteLine("[ERRO] Não é possível bloquear horários no passado.");
                return;
            }

            Console.Write("Hora do bloqueio (10 a 20): ");
            if (!int.TryParse(Console.ReadLine(), out int horaBloqueio) || horaBloqueio < 10 || horaBloqueio > 20)
            {
                Console.WriteLine("[ERRO] A clínica atende apenas entre as 10:00 e as 20:00.");
                return;
            }

            DateTime dataHoraCompleta = dataBloqueio.AddHours(horaBloqueio);

            using var conn = DatabaseConnection.GetConnection();
            string qCheck = "SELECT COUNT(*) FROM agendamento WHERE id_terapeuta = @id_terapeuta AND data_hora = @data_hora AND status NOT IN ('CANCELADO')";
            using var cmdCheck = new SqlCommand(qCheck, conn);
            cmdCheck.Parameters.AddWithValue("@id_terapeuta", _terapeuta.IdUsuario);
            cmdCheck.Parameters.AddWithValue("@data_hora", dataHoraCompleta);
            int ocupado = (int)cmdCheck.ExecuteScalar();

            if (ocupado > 0)
            {
                Console.WriteLine("[ERRO] Você já possui um compromisso ou bloqueio neste horário.");
                return;
            }

            // Inserimos o próprio ID como paciente para identificar como um bloqueio
            string qInsert = "INSERT INTO agendamento (id_paciente, id_terapeuta, data_hora, descricao_sintomas, status) VALUES (@terapeuta, @terapeuta, @data_hora, 'Bloqueio de Agenda', 'CONFIRMADO')";
            using var cmdInsert = new SqlCommand(qInsert, conn);
            cmdInsert.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
            cmdInsert.Parameters.AddWithValue("@data_hora", dataHoraCompleta);
            cmdInsert.ExecuteNonQuery();

            Console.WriteLine("[SUCESSO] Horário bloqueado com sucesso!");
        }

        private void ProntuarioEvolucao()
        {
            Console.WriteLine("\n--- PRONTUÁRIO E EVOLUÇÃO ---");
            Console.Write("Digite o ID da Consulta (Agendamento) que deseja atualizar: ");
            if (!int.TryParse(Console.ReadLine(), out int idAgendamento)) return;

            using var conn = DatabaseConnection.GetConnection();
            
            // Verificar agendamento e pegar paciente
            string qCheck = "SELECT id_paciente, status FROM agendamento WHERE id_agendamento = @id_agendamento AND id_terapeuta = @id_terapeuta";
            using var cmdCheck = new SqlCommand(qCheck, conn);
            cmdCheck.Parameters.AddWithValue("@id_agendamento", idAgendamento);
            cmdCheck.Parameters.AddWithValue("@id_terapeuta", _terapeuta.IdUsuario);
            
            int idPaciente = 0;
            string status = "";
            using (var reader = cmdCheck.ExecuteReader())
            {
                if (reader.Read())
                {
                    idPaciente = reader.GetInt32(0);
                    status = reader.GetString(1);
                }
                else
                {
                    Console.WriteLine("[ERRO] Consulta não encontrada ou não pertence a você.");
                    return;
                }
            }

            if (idPaciente == _terapeuta.IdUsuario)
            {
                Console.WriteLine("[ERRO] Esta ID de agendamento é um bloqueio de agenda, não uma consulta.");
                return;
            }

            // Marcar como realizada caso ainda não esteja
            if (status != "REALIZADO")
            {
                string qUpd = "UPDATE agendamento SET status = 'REALIZADO' WHERE id_agendamento = @id";
                using var cmdUpd = new SqlCommand(qUpd, conn);
                cmdUpd.Parameters.AddWithValue("@id", idAgendamento);
                cmdUpd.ExecuteNonQuery();
                Console.WriteLine("[INFO] Consulta marcada como REALIZADA.");
            }

            // Obter ou criar Prontuario
            string qPront = "SELECT id_prontuario FROM prontuario WHERE id_paciente = @paciente AND id_terapeuta = @terapeuta";
            using var cmdPront = new SqlCommand(qPront, conn);
            cmdPront.Parameters.AddWithValue("@paciente", idPaciente);
            cmdPront.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
            
            object objPront = cmdPront.ExecuteScalar();
            int idProntuario = 0;

            if (objPront == null)
            {
                Console.WriteLine("[INFO] Nenhum prontuário anterior encontrado para este paciente. Criando um novo...");
                Console.Write("Digite a descrição base do prontuário: ");
                string desc = Console.ReadLine();

                string qInsPront = "INSERT INTO prontuario (id_paciente, id_terapeuta, versao, descricao) OUTPUT INSERTED.id_prontuario VALUES (@paciente, @terapeuta, 1, @desc)";
                using var cmdIns = new SqlCommand(qInsPront, conn);
                cmdIns.Parameters.AddWithValue("@paciente", idPaciente);
                cmdIns.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
                cmdIns.Parameters.AddWithValue("@desc", desc);
                idProntuario = (int)cmdIns.ExecuteScalar();
            }
            else
            {
                idProntuario = (int)objPront;
            }

            // Adicionar Nota de Evolução
            Console.WriteLine("\n--- NOVA NOTA DE EVOLUÇÃO ---");
            Console.Write("Digite o texto da evolução do paciente: ");
            string textoEvolucao = Console.ReadLine();

            string qNota = "INSERT INTO nota_evolucao (id_prontuario, id_terapeuta, id_agendamento, texto_evolucao) VALUES (@pront, @terapeuta, @agen, @texto)";
            using var cmdNota = new SqlCommand(qNota, conn);
            cmdNota.Parameters.AddWithValue("@pront", idProntuario);
            cmdNota.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
            cmdNota.Parameters.AddWithValue("@agen", idAgendamento);
            cmdNota.Parameters.AddWithValue("@texto", textoEvolucao);
            cmdNota.ExecuteNonQuery();

            Console.WriteLine("[SUCESSO] Prontuário e Nota de Evolução salvos com sucesso!");
        }

        private void GerarRelatorioPdf()
        {
            Console.WriteLine("\n--- GERAR RELATÓRIO MENSAL ---");
            Console.Write("Digite o Mês (1-12): ");
            if (!int.TryParse(Console.ReadLine(), out int mes) || mes < 1 || mes > 12) return;
            Console.Write("Digite o Ano (ex: 2026): ");
            if (!int.TryParse(Console.ReadLine(), out int ano)) return;

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

                            using var conn = DatabaseConnection.GetConnection();
                            string query = @"
                                SELECT a.data_hora, p.nome, a.status, a.id_paciente
                                FROM agendamento a
                                JOIN usuario p ON a.id_paciente = p.id_usuario
                                WHERE a.id_terapeuta = @terapeuta AND MONTH(a.data_hora) = @mes AND YEAR(a.data_hora) = @ano
                                ORDER BY a.data_hora ASC";

                            using var cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@terapeuta", _terapeuta.IdUsuario);
                            cmd.Parameters.AddWithValue("@mes", mes);
                            cmd.Parameters.AddWithValue("@ano", ano);
                            using var reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                DateTime data = reader.GetDateTime(0);
                                string pac = reader.GetString(1);
                                string stat = reader.GetString(2);
                                int idPac = reader.GetInt32(3);

                                if (idPac == _terapeuta.IdUsuario)
                                {
                                    pac = "BLOQUEIO DE AGENDA";
                                    stat = "-";
                                }

                                table.Cell().Text(data.ToString("dd/MM/yyyy HH:mm"));
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

            Console.WriteLine($"\n[SUCESSO] Relatório PDF gerado com sucesso em: {filePath}");
        }
    }
}
