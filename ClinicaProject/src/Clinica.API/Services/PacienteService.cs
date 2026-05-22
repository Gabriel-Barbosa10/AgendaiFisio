using System;
using Microsoft.Data.SqlClient;
using AgendaiFisioConsole.Models;
using AgendaiFisioConsole.Data;

namespace AgendaiFisioConsole.Services
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
                Console.WriteLine($"\n--- BEM VINDO, {_paciente.Nome.ToUpper()} (PACIENTE) ---");
                Console.WriteLine("1. Agendar Consulta");
                Console.WriteLine("2. Minhas Consultas");
                Console.WriteLine("3. Sair / Logout");
                Console.Write("Escolha uma opção: ");
                string opcao = Console.ReadLine();

                if (opcao == "1") AgendarConsulta();
                else if (opcao == "2") MinhasConsultas();
                else if (opcao == "3") break;
                else Console.WriteLine("Opção inválida.");
            }
        }

        private void AgendarConsulta()
        {
            Console.WriteLine("\n--- AGENDAR CONSULTA ---");
            
            // Listar Terapeutas
            using var conn = DatabaseConnection.GetConnection();
            string qTerapeuta = "SELECT id_usuario, nome, crefito FROM usuario WHERE tipo_perfil = 'TERAPEUTA'";
            using var cmdTerapeuta = new SqlCommand(qTerapeuta, conn);
            using var readerTerapeuta = cmdTerapeuta.ExecuteReader();
            
            Console.WriteLine("Terapeutas Disponíveis:");
            bool temTerapeuta = false;
            while (readerTerapeuta.Read())
            {
                temTerapeuta = true;
                Console.WriteLine($"ID: {readerTerapeuta.GetInt32(0)} | Nome: {readerTerapeuta.GetString(1)} | CREFITO: {readerTerapeuta.GetString(2)}");
            }
            readerTerapeuta.Close();

            if (!temTerapeuta)
            {
                Console.WriteLine("Nenhum terapeuta disponível no sistema.");
                return;
            }

            Console.Write("\nDigite o ID do terapeuta desejado: ");
            if (!int.TryParse(Console.ReadLine(), out int idTerapeuta))
            {
                Console.WriteLine("[ERRO] ID inválido.");
                return;
            }

            Console.Write("Data da consulta (dd/MM/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataConsulta))
            {
                Console.WriteLine("[ERRO] Formato de data inválido.");
                return;
            }

            if (dataConsulta.Date < DateTime.Now.Date)
            {
                Console.WriteLine("[ERRO] Não é possível agendar no passado.");
                return;
            }

            Console.Write("Hora da consulta (10 a 20): ");
            if (!int.TryParse(Console.ReadLine(), out int horaConsulta) || horaConsulta < 10 || horaConsulta > 20)
            {
                Console.WriteLine("[ERRO] A clínica atende apenas entre as 10:00 e as 20:00 (de 1 em 1 hora).");
                return;
            }

            DateTime dataHoraCompleta = dataConsulta.AddHours(horaConsulta);

            // Verificar se o terapeuta já tem agendamento/bloqueio neste horário
            string qCheck = "SELECT COUNT(*) FROM agendamento WHERE id_terapeuta = @id_terapeuta AND data_hora = @data_hora AND status NOT IN ('CANCELADO')";
            using var cmdCheck = new SqlCommand(qCheck, conn);
            cmdCheck.Parameters.AddWithValue("@id_terapeuta", idTerapeuta);
            cmdCheck.Parameters.AddWithValue("@data_hora", dataHoraCompleta);
            int ocupado = (int)cmdCheck.ExecuteScalar();

            if (ocupado > 0)
            {
                Console.WriteLine("[ERRO] O terapeuta já possui um compromisso ou bloqueio neste horário.");
                return;
            }

            Console.Write("Descreva brevemente os seus sintomas para o terapeuta: ");
            string sintomas = Console.ReadLine();

            // Salvar Agendamento
            string qInsert = "INSERT INTO agendamento (id_paciente, id_terapeuta, data_hora, descricao_sintomas, status) VALUES (@paciente, @terapeuta, @data_hora, @sintomas, 'PENDENTE')";
            using var cmdInsert = new SqlCommand(qInsert, conn);
            cmdInsert.Parameters.AddWithValue("@paciente", _paciente.IdUsuario);
            cmdInsert.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            cmdInsert.Parameters.AddWithValue("@data_hora", dataHoraCompleta);
            cmdInsert.Parameters.AddWithValue("@sintomas", sintomas);
            cmdInsert.ExecuteNonQuery();

            Console.WriteLine("[SUCESSO] Consulta agendada com sucesso!");
        }

        private void MinhasConsultas()
        {
            Console.WriteLine("\n--- MINHAS CONSULTAS ---");
            using var conn = DatabaseConnection.GetConnection();
            string query = @"
                SELECT a.id_agendamento, a.data_hora, t.nome, a.status 
                FROM agendamento a
                JOIN usuario t ON a.id_terapeuta = t.id_usuario
                WHERE a.id_paciente = @paciente
                ORDER BY a.data_hora DESC";
            
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@paciente", _paciente.IdUsuario);
            using var reader = cmd.ExecuteReader();

            bool achou = false;
            while (reader.Read())
            {
                achou = true;
                int id = reader.GetInt32(0);
                DateTime dataHora = reader.GetDateTime(1);
                string terapeuta = reader.GetString(2);
                string status = reader.GetString(3);

                Console.WriteLine($"Consulta #{id} | Data: {dataHora:dd/MM/yyyy HH:mm} | Terapeuta: {terapeuta} | Status: {status}");
            }

            if (!achou)
            {
                Console.WriteLine("Você não possui nenhuma consulta agendada.");
            }
        }
    }
}
