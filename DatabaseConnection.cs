using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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

        public bool AuthenticateUser(string login, string password)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(1) FROM employees WHERE login = @login AND password = @password";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
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


        public bool AddClient(string firstName, string lastName, DateTime startDate, DateTime endDate, int passLength, decimal passPrice)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO clients (firstname, lastname, startdate, enddate, passlength, passprice) VALUES (@firstname, @lastname, @startdate, @enddate, @passlength, @passprice)";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", firstName);
                        command.Parameters.AddWithValue("@lastname", lastName);
                        command.Parameters.AddWithValue("@startdate", startDate);
                        command.Parameters.AddWithValue("@enddate", endDate);
                        command.Parameters.AddWithValue("@passlength", passLength);
                        command.Parameters.AddWithValue("@passprice", passPrice);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Błąd podczas łączenia z bazą danych", ex);
            }
        }
        public List<Client> GetClients()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM clients";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            List<Client> clients = new List<Client>();
                            while (reader.Read())
                            {
                                Client client = new Client
                                {
                                    Id = reader.GetInt32(0),
                                    FirstName = reader.GetString(1),
                                    LastName = reader.GetString(2),
                                    StartDate = reader.GetDateTime(3),
                                    EndDate = reader.GetDateTime(4),
                                    PassLength = reader.GetInt32(5),
                                    PassPrice = reader.GetDecimal(6)
                                };
                                clients.Add(client);
                            }
                            return clients;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Błąd podczas pobierania klientów z bazy danych", ex);
            }
        }
    }

}
