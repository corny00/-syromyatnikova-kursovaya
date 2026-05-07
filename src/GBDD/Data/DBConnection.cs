using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Data
{
    public static class DBConnection
    {
        private static readonly string _connectionString;

        static DBConnection()
        {
            _connectionString = ConfigurationManager
                .ConnectionStrings["GibddConnection"]
                .ConnectionString;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

      
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();

                connection.Open();
                adapter.Fill(dataTable);
                connection.Close();

                return dataTable;
            }
        }


        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                connection.Close();

                return rowsAffected;
            }
        }

       
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                var result = command.ExecuteScalar();
                connection.Close();

                return result;
            }
        }


        public static int ExecuteScalarInt(string query, SqlParameter[] parameters = null)
        {
            var result = ExecuteScalar(query + "; SELECT SCOPE_IDENTITY();", parameters);
            return Convert.ToInt32(result);
        }
    }
}
