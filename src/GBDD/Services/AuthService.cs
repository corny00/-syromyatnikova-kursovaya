using GBDD.Data;
using GBDD.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Services
{
    internal class UserService
    {
        private List<User> users = new List<User>();

        public UserService()
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            users.Clear();
            string query = "SELECT UserId, Username, PasswordHash, FullName, RoleId FROM Users WHERE IsActive = 1";

            string connectionString = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            RoleId = Convert.ToInt32(reader["RoleId"])
                        });
                    }
                }
            }
        }

        public User Login(string login, string password)
        {
            string hashedPassword = PasswordHasher.ComputeSha256Hash(password);

            return users.Find(u => u.Username == login && u.PasswordHash == hashedPassword);
        }
    }
}
