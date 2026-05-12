using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Clinica.API.Models.Entities;
//TODO Conexao esta sendo aberta td vez que faz consulta no banco. Tem que melhorar isso
namespace Clinica.API.Repositories
{
    public class AgendamentoRepository
    {
        private readonly string _connectionString;

        public AgendamentoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ClinicaDb")
                ?? throw new InvalidOperationException("Connection string 'ClinicaDb' não encontrada.");
        }
        //TODO Tem classe pra que?
        // ── Verifica conflito de horário ─────────────────────────────────────
        public int ContarAgendamentosNoHorario(Agendamento agendamento)
        {
            const string sql = @"
                SELECT COUNT(id_agendamento)
                FROM   agendamento
                WHERE  id_terapeuta = @id_terapeuta
                  AND  data_hora    = @data_hora
                  AND  status      != 'CANCELADO';";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id_terapeuta", agendamento.IdTerapeuta);
            command.Parameters.AddWithValue("@data_hora",    agendamento.DataHora.ToString("yyyy-MM-dd HH:mm:ss"));

            return Convert.ToInt32(command.ExecuteScalar());
        }
        //TODO Tem classe pra que?
        // ── Insere novo agendamento ───────────────────────────────────────────
        public int Inserir(Agendamento agendamento)
        {
            const string sql = @"
                INSERT INTO agendamento (id_paciente, id_terapeuta, data_hora, duracao_min, status)
                VALUES (@id_paciente, @id_terapeuta, @data_hora, @duracao_min, @status);
                SELECT last_insert_rowid();";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            
            // Você consome as propriedades que já foram validadas pela classe
            command.Parameters.AddWithValue("@id_paciente",  agendamento.IdPaciente);
            command.Parameters.AddWithValue("@id_terapeuta", agendamento.IdTerapeuta);
            command.Parameters.AddWithValue("@data_hora",    agendamento.DataHora.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@duracao_min",  agendamento.DuracaoMin);
            command.Parameters.AddWithValue("@status",       agendamento.Status);

            return Convert.ToInt32(command.ExecuteScalar());
        }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id_paciente",  idPaciente);
            command.Parameters.AddWithValue("@id_terapeuta", idTerapeuta);
            command.Parameters.AddWithValue("@data_hora",    dataHora.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@duracao_min",  duracaoMin);
            command.Parameters.AddWithValue("@status",       status);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        // ── Lista agendamentos por terapeuta ──────────────────────────────────

        public IEnumerable<Agendamento> ListarPorTerapeuta(int idTerapeuta)
        {
            const string sql = @"
                SELECT id_agendamento,
                       id_paciente,
                       id_terapeuta,
                       data_hora,
                       duracao_min,
                       status
                FROM   agendamento
                WHERE  id_terapeuta = @id_terapeuta
                ORDER  BY data_hora;";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id_terapeuta", idTerapeuta);

            using var reader = command.ExecuteReader();

            var lista = new List<Agendamento>();

            while (reader.Read())
            {
                lista.Add(Agendamento.FromDatabase(
                    id:          reader.GetInt32(0),
                    idPaciente:  reader.GetInt32(1),
                    idTerapeuta: reader.GetInt32(2),
                    dataHora:    reader.GetDateTime(3),
                    duracaoMin:  reader.GetInt32(4),
                    status:      reader.GetString(5)
                ));
            }

            return lista;
        }
    }
}