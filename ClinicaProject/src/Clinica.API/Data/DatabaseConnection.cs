using System;
using Microsoft.Data.SqlClient;

namespace AgendaiFisioConsole.Data
{
    public static class DatabaseConnection
    {
        // Utilizando uma string de conexão padrão local conforme conversado no plano.
        private const string ConnectionString = "Server=localhost;Database=AgendaiFisioDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
        
        public static void TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                Console.WriteLine("Conexão com o banco de dados bem sucedida!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar com o banco de dados: {ex.Message}");
            }
        }

        public static void SeedDatabase()
        {
            try
            {
                using var conn = GetConnection();
                string checkQuery = "SELECT COUNT(*) FROM usuario";
                using var cmdCheck = new SqlCommand(checkQuery, conn);
                int count = (int)cmdCheck.ExecuteScalar();
                
                if (count == 0)
                {
                    Console.WriteLine("Inserindo dados de teste...");
                    
                    // Inserir Terapeuta
                    string insertTerapeuta = "INSERT INTO usuario (nome, email, cpf, senha, crefito, tipo_perfil, aceite_lgpd) VALUES ('Dra. Maria', 'maria@email.com', '11111111111', 'password123', '123456-TO', 'TERAPEUTA', 1)";
                    using var cmd1 = new SqlCommand(insertTerapeuta, conn);
                    cmd1.ExecuteNonQuery();

                    // Inserir Paciente
                    string insertPaciente = "INSERT INTO usuario (nome, email, cpf, senha, tipo_perfil, aceite_lgpd) VALUES ('João Silva', 'joao@email.com', '22222222222', 'password123', 'PACIENTE', 1)";
                    using var cmd2 = new SqlCommand(insertPaciente, conn);
                    cmd2.ExecuteNonQuery();

                    Console.WriteLine("Dados de teste inseridos com sucesso.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inserir dados de teste: {ex.Message}");
            }
        }
    }
}
