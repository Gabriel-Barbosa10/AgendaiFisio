using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using AgendaiFisio.Models;
using AgendaiFisio.Data;

namespace AgendaiFisio.Services
{
    public class PacienteService
    {
        private readonly Usuario _paciente;

        public PacienteService(Usuario paciente)
        {
            _paciente = paciente;
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
            
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            // 1. LISTAR TERAPEUTAS DISPONÍVEIS
            string qTerapeuta = "SELECT id_usuario, nome, crefito FROM usuario WHERE tipo_perfil = 'TERAPEUTA'";
            using var cmdTerapeuta = new SqlCommand(qTerapeuta, conn);
            using var readerTerapeuta = cmdTerapeuta.ExecuteReader();
            
            Console.WriteLine("Terapeutas Disponíveis:");
            bool temTerapeuta = false;
            var terapeutasValidos = new List<int>();

            while (readerTerapeuta.Read())
            {
                temTerapeuta = true;
                int id = readerTerapeuta.GetInt32(0);
                terapeutasValidos.Add(id);
                Console.WriteLine($"ID: {id} | Nome: {readerTerapeuta.GetString(1)} | CREFITO: {readerTerapeuta.GetString(2)}");
            }
            readerTerapeuta.Close();

            if (!temTerapeuta)
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
            List<int> horasOcupadas = ObterHorariosOcupadosDoTerapeuta(idTerapeuta, dataConsulta);

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
            string qInsert = @"
                INSERT INTO agendamento (id_paciente, id_terapeuta, data_agenda, hora_agenda, tipo_registro, status, descricao_sintomas) 
                VALUES (@paciente, @terapeuta, @dataAgenda, @horaAgenda, 'CONSULTA', 'PENDENTE', @sintomas)";
            
            using var cmdInsert = new SqlCommand(qInsert, conn);
            cmdInsert.Parameters.AddWithValue("@paciente", _paciente.IdUsuario);
            cmdInsert.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            cmdInsert.Parameters.AddWithValue("@dataAgenda", dataConsulta.Date);
            cmdInsert.Parameters.AddWithValue("@horaAgenda", dataHoraCompleta);
            cmdInsert.Parameters.AddWithValue("@sintomas", string.IsNullOrWhiteSpace(sintomas) ? (object)DBNull.Value : sintomas);
            cmdInsert.ExecuteNonQuery();

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
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                SELECT a.id_agendamento, a.hora_agenda, t.nome, a.status 
                FROM agendamento a
                JOIN usuario t ON a.id_terapeuta = t.id_usuario
                WHERE a.id_paciente = @paciente AND a.tipo_registro = 'CONSULTA'
                ORDER BY a.hora_agenda DESC";
            
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@paciente", _paciente.IdUsuario);
            using var reader = cmd.ExecuteReader();

            bool achou = false;
            while (reader.Read())
            {
                achou = true;
                int id = reader.GetInt32(0);
                DateTime horaAgenda = reader.GetDateTime(1);
                string terapeuta = reader.GetString(2);
                string status = reader.GetString(3);

                Console.WriteLine($"Consulta #{id} | Data/Hora: {horaAgenda:dd/MM/yyyy HH:mm} | Terapeuta: {terapeuta} | Status: {status}");
            }

            if (!achou)
            {
                Console.WriteLine("Você não possui nenhuma consulta agendada.");
            }

            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey(true);
        }

        private List<int> ObterHorariosOcupadosDoTerapeuta(int idTerapeuta, DateTime data)
        {
            var horas = new List<int>();
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                SELECT DATEPART(HOUR, hora_agenda) 
                FROM agendamento 
                WHERE id_terapeuta = @idTerapeuta AND data_agenda = @data AND status != 'CANCELADO'";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@idTerapeuta", idTerapeuta);
            cmd.Parameters.AddWithValue("@data", data.Date);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                horas.Add(reader.GetInt32(0));
            }
            return horas;
        }
    }
}