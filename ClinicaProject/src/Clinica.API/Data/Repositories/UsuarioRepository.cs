using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using AgendaiFisio.Models;

namespace AgendaiFisio.Data.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        public Usuario ObterPorEmail(string email)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "SELECT id_usuario, nome, email, cpf, senha, crefito, tipo_perfil, aceite_lgpd FROM usuario WHERE email = @email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);

            using var reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                Usuario usuario;
                string tipoPerfil = reader.GetString(6);

                if (tipoPerfil == "TERAPEUTA")
                {
                    usuario = new Terapeuta
                    {
                        Crefito = reader.IsDBNull(5) ? null : reader.GetString(5)
                    };
                }
                else
                {
                    usuario = new Paciente();
                }

                usuario.IdUsuario = reader.GetInt32(0);
                usuario.Nome = reader.GetString(1);
                usuario.Email = reader.GetString(2);
                usuario.Cpf = reader.GetString(3);
                usuario.Senha = reader.GetString(4);
                usuario.AceiteLgpd = reader.GetBoolean(7);

                return usuario;
            }

            return null;
        }

        public bool ExisteEmail(string email)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "SELECT COUNT(1) FROM usuario WHERE email = @email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            int total = (int)cmd.ExecuteScalar();
            
            return total > 0;
        }

        public bool ExisteCpf(string cpf)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "SELECT COUNT(1) FROM usuario WHERE cpf = @cpf";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@cpf", cpf);
            int total = (int)cmd.ExecuteScalar();
            
            return total > 0;
        }

        public void Criar(Usuario usuario)
        {
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "INSERT INTO usuario (nome, email, cpf, senha, crefito, tipo_perfil, aceite_lgpd) VALUES (@nome, @email, @cpf, @senha, @crefito, @tipo_perfil, @aceite_lgpd)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nome", usuario.Nome);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@cpf", usuario.Cpf);
            cmd.Parameters.AddWithValue("@senha", usuario.Senha);
            
            if (usuario is Terapeuta t)
            {
                cmd.Parameters.AddWithValue("@crefito", (object)t.Crefito ?? DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@crefito", DBNull.Value);
            }
            
            cmd.Parameters.AddWithValue("@tipo_perfil", usuario.TipoPerfil);
            cmd.Parameters.AddWithValue("@aceite_lgpd", usuario.AceiteLgpd);

            cmd.ExecuteNonQuery();
        }

        public List<Terapeuta> ListarTerapeutas()
        {
            var lista = new List<Terapeuta>();
            using var conn = DatabaseConnection.GetConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            string query = "SELECT id_usuario, nome, crefito FROM usuario WHERE tipo_perfil = 'TERAPEUTA'";
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                lista.Add(new Terapeuta
                {
                    IdUsuario = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Crefito = reader.GetString(2)
                });
            }
            return lista;
        }
    }
}
