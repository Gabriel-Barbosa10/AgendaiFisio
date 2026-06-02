using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace AgendaiFisio.Data.Repositories
{
    public class AgendamentoRepository : IAgendamentoRepository
    {
        public List<AgendamentoDto> ObterAgendamentosDoTerapeuta(int idTerapeuta)
        {
            var lista = new List<AgendamentoDto>();
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                SELECT a.id_agendamento, a.hora_agenda, p.nome, a.status, a.descricao_sintomas, a.id_paciente, a.tipo_registro
                FROM agendamento a
                LEFT JOIN usuario p ON a.id_paciente = p.id_usuario
                WHERE a.id_terapeuta = @terapeuta
                ORDER BY a.hora_agenda ASC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new AgendamentoDto
                {
                    IdAgendamento = reader.GetInt32(0),
                    HoraAgenda = reader.GetDateTime(1),
                    NomePessoa = reader.IsDBNull(2) ? "Nenhum" : reader.GetString(2),
                    Status = reader.GetString(3),
                    DescricaoSintomas = reader.IsDBNull(4) ? "Nenhum" : reader.GetString(4),
                    IdPaciente = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                    TipoRegistro = reader.GetString(6)
                });
            }
            return lista;
        }

        public List<AgendamentoDto> ObterAgendamentosDoTerapeutaPorMes(int idTerapeuta, int mes, int ano)
        {
            var lista = new List<AgendamentoDto>();
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                SELECT a.hora_agenda, p.nome, a.status, a.id_paciente, a.tipo_registro
                FROM agendamento a
                LEFT JOIN usuario p ON a.id_paciente = p.id_usuario
                WHERE a.id_terapeuta = @terapeuta AND MONTH(a.data_agenda) = @mes AND YEAR(a.data_agenda) = @ano
                ORDER BY a.hora_agenda ASC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            cmd.Parameters.AddWithValue("@mes", mes);
            cmd.Parameters.AddWithValue("@ano", ano);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new AgendamentoDto
                {
                    HoraAgenda = reader.GetDateTime(0),
                    NomePessoa = reader.IsDBNull(1) ? "Nenhum" : reader.GetString(1),
                    Status = reader.GetString(2),
                    IdPaciente = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                    TipoRegistro = reader.GetString(4)
                });
            }
            return lista;
        }

        public List<AgendamentoDto> ObterConsultasDoPaciente(int idPaciente)
        {
            var lista = new List<AgendamentoDto>();
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                SELECT a.id_agendamento, a.hora_agenda, t.nome, a.status 
                FROM agendamento a
                JOIN usuario t ON a.id_terapeuta = t.id_usuario
                WHERE a.id_paciente = @paciente AND a.tipo_registro = 'CONSULTA'
                ORDER BY a.hora_agenda DESC";
            
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@paciente", idPaciente);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new AgendamentoDto
                {
                    IdAgendamento = reader.GetInt32(0),
                    HoraAgenda = reader.GetDateTime(1),
                    NomePessoa = reader.GetString(2), // Terapeuta
                    Status = reader.GetString(3)
                });
            }
            return lista;
        }

        public void CriarAgendamento(int? idPaciente, int idTerapeuta, DateTime dataAgenda, DateTime horaAgenda, string tipoRegistro, string status, string sintomas)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = @"
                INSERT INTO agendamento (id_paciente, id_terapeuta, data_agenda, hora_agenda, tipo_registro, status, descricao_sintomas) 
                VALUES (@paciente, @terapeuta, @dataAgenda, @horaAgenda, @tipoRegistro, @status, @sintomas)";
            
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@paciente", idPaciente.HasValue ? (object)idPaciente.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            cmd.Parameters.AddWithValue("@dataAgenda", dataAgenda.Date);
            cmd.Parameters.AddWithValue("@horaAgenda", horaAgenda);
            cmd.Parameters.AddWithValue("@tipoRegistro", tipoRegistro);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@sintomas", string.IsNullOrWhiteSpace(sintomas) ? (object)DBNull.Value : sintomas);
            cmd.ExecuteNonQuery();
        }

        public Dictionary<int, string> ObterStatusHorariosDoBanco(DateTime data)
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

        public List<int> ObterHorariosOcupadosDoTerapeuta(int idTerapeuta, DateTime data)
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

        public AgendamentoInfo ObterInfoAgendamento(int idAgendamento, int idTerapeuta)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            
            string query = "SELECT id_paciente, status, tipo_registro FROM agendamento WHERE id_agendamento = @id_agendamento AND id_terapeuta = @id_terapeuta";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id_agendamento", idAgendamento);
            cmd.Parameters.AddWithValue("@id_terapeuta", idTerapeuta);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new AgendamentoInfo
                {
                    IdPaciente = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                    Status = reader.GetString(1),
                    TipoRegistro = reader.GetString(2)
                };
            }
            
            return null;
        }

        public void MarcarComoRealizado(int idAgendamento)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "UPDATE agendamento SET status = 'REALIZADO' WHERE id_agendamento = @id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", idAgendamento);
            cmd.ExecuteNonQuery();
        }
    }
}
