using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessClub
{
    internal class DatabaseConnection
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=1234;Database=FitnessClub";

        public DatabaseConnection()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PostgreSqlConnectionString"].ConnectionString;
        }

        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(1) FROM employees WHERE login = @username AND password = @password";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Błąd podczas łączenia z bazą danych", ex);
            }
        }

        public void ConnectAndExecuteQuery()
        {
            // Przykładowa metoda do wykonywania zapytań
        }
    }
}
