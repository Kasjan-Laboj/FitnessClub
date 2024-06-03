using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FitnessClub
{
    internal class DatabaseConnection
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=1234;Database=FitnessClub";

        public DatabaseConnection()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PostgreSqlConnectionString"].ConnectionString;
        }
        public int AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id FROM employees WHERE login = @username AND password = @password";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            return (int)result; // Returns employee id
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error authenticating user", ex);
            }

            return -1; // If login went wrong, return -1
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
        public List<Product> GetProducts()
        {
            try
            {
                List<Product> products = new List<Product>();
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM products";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Product product = new Product
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Name = reader["name"].ToString(),
                                    Quantity = Convert.ToInt32(reader["quantity"]),
                                    Price = Convert.ToDecimal(reader["price"])
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
                return products;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error while retrieving products from the database", ex);
            }
        }

        public bool AddProduct(string name, int quantity, decimal price)
        {
            try
            {              
                if (ProductExists(name))
                {
                    return false;
                }

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO products (name, quantity, price) VALUES (@name, @quantity, @price)";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@quantity", quantity);
                        command.Parameters.AddWithValue("@price", price);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error adding product to the database", ex);
            }
        }

        public bool ProductExists(string name)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(1) FROM products WHERE name = @name";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "UPDATE products SET name = @name, quantity = @quantity WHERE id = @id";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", product.Id);
                        command.Parameters.AddWithValue("@name", product.Name);
                        command.Parameters.AddWithValue("@quantity", product.Quantity);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating product in the database", ex);
            }
        }
        public bool RemoveProductQuantity(int productId, int quantity, int employeeId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE products SET quantity = quantity - @quantity WHERE id = @id";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", productId);
                        command.Parameters.AddWithValue("@quantity", quantity);
                        //command.Parameters.AddWithValue("employeid", employeeId);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            //Rejestruj sprzedaż
                            //bool success = RegisterProductSale(employeeId, productId, quantity);
                            return true;
                            //return success;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating product quantity in the database", ex);
            }
        }

        //private bool RegisterProductSale(int employeeId, int productId, int quantity)
        //{
        //    try
        //    {
        //        using (var connection = new NpgsqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            string query = "INSERT INTO product_sales (employee_id, product_id, quantity, sale_date) VALUES (@employeeId, @productId, @quantity, @saleDate)";
        //            using (var command = new NpgsqlCommand(query, connection))
        //            {
        //                command.Parameters.AddWithValue("@employeeId", employeeId);
        //                command.Parameters.AddWithValue("@productId", productId);
        //                command.Parameters.AddWithValue("@quantity", quantity);
        //                command.Parameters.AddWithValue("@saleDate", DateTime.Now);
        //                int result = command.ExecuteNonQuery();
        //                return result > 0;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("Error registering product sale", ex);
        //    }
        //}
    }
}
