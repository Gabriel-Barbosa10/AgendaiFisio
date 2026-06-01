using System;
using Microsoft.Data.SqlClient;

namespace AgendaiFisio.Data.Repositories
{
    public class ProntuarioRepository : IProntuarioRepository
    {
        public int? ObterIdProntuario(int idPaciente, int idTerapeuta)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "SELECT id_prontuario FROM prontuario WHERE id_paciente = @paciente AND id_terapeuta = @terapeuta";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@paciente", idPaciente); 
            cmd.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            
            object result = cmd.ExecuteScalar();
            if (result != null)
            {
                return (int)result;
            }
            return null;
        }

        public int CriarProntuario(int idPaciente, int idTerapeuta, string descricao)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "INSERT INTO prontuario (id_paciente, id_terapeuta, versao, descricao) OUTPUT INSERTED.id_prontuario VALUES (@paciente, @terapeuta, 1, @desc)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@paciente", idPaciente);
            cmd.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            cmd.Parameters.AddWithValue("@desc", descricao);
            
            return (int)cmd.ExecuteScalar();
        }

        public void AdicionarNotaEvolucao(int idProntuario, int idTerapeuta, int idAgendamento, string textoEvolucao)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "INSERT INTO nota_evolucao (id_prontuario, id_terapeuta, id_agendamento, texto_evolucao) VALUES (@pront, @terapeuta, @agen, @texto)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pront", idProntuario);
            cmd.Parameters.AddWithValue("@terapeuta", idTerapeuta);
            cmd.Parameters.AddWithValue("@agen", idAgendamento);
            cmd.Parameters.AddWithValue("@texto", textoEvolucao);
            cmd.ExecuteNonQuery();
        }
    }
}
