﻿using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
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
        #region db Connection
        private string connectionString = "Host=localhost;Username=postgres;Password=1234;Database=FitnessClub";

        public DatabaseConnection()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PostgreSqlConnectionString"].ConnectionString;
        }
        #endregion
        #region db Clients and Pass

        // <summary>
        /// Authenticates an employee from the database.
        /// </summary>
        /// <param name="username">The username of the employee.</param>
        /// <param name="password">The password of the employee.</param>
        /// <returns>The employee ID if authentication is successful, otherwise -1.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error authenticating the user.</exception>
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

        /// <summary>
        /// Adds a new client to the database.
        /// </summary>
        /// <param name="firstName">The first name of the client.</param>
        /// <param name="lastName">The last name of the client.</param>
        /// <param name="startDate">The start date of the client's pass.</param>
        /// <param name="endDate">The end date of the client's pass.</param>
        /// <param name="passLength">The length of the client's pass.</param>
        /// <param name="passPrice">The price of the client's pass.</param>
        /// <returns>True if the client was successfully added, otherwise false.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error adding the client to the database.</exception>
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
                throw new ApplicationException("Error while connecting with database", ex);
            }
        }

        /// <summary>
        /// Retrieves a list of clients from the database.
        /// </summary>
        /// <returns>A list of Client objects.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error retrieving clients from the database.</exception>
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
                throw new ApplicationException("Error while database download", ex);
            }
        }

        #endregion
        #region db WareHouse and Shop
        /// <summary>
        /// Retrieves a list of products from the database.
        /// </summary>
        /// <returns>A list of Product objects.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error retrieving products from the database.</exception>
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

        /// <summary>
        /// Adds a new product to the database if it doesn't already exist.
        /// </summary>
        /// <param name="name">The name of the product to add.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        /// <param name="price">The price of the product to add.</param>
        /// <returns>True if the product was successfully added, otherwise false.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error adding the product to the database.</exception>
        public bool AddProduct(string name, int quantity, decimal price)
        {
            try
            {
                if (ProductExists(name))
                {
                    return false;
                }
                // TRANSACTION
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = "INSERT INTO products (name, quantity, price) VALUES (@name, @quantity, @price)";
                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("name", name);
                                command.Parameters.AddWithValue("@quantity", quantity);
                                command.Parameters.AddWithValue("@price", price);

                                int result = command.ExecuteNonQuery();

                                transaction.Commit();
                                return result > 0;
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error adding product to the database", ex);
            }
        }
        /// <summary>
        /// Checks if a product with the given name already exists in the database.
        /// </summary>
        /// <param name="name">The name of the product to check.</param>
        /// <returns>True if the product already exists, otherwise false.</returns>
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

        /// <summary>
        /// Updates a product in the database.
        /// </summary>
        /// <param name="product">The Product object containing updated information.</param>
        /// <returns>True if the product was successfully updated, otherwise false.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error updating the product in the database.</exception>
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

        /// <summary>
        /// Removes a specified quantity of a product from the database.
        /// </summary>
        /// <param name="productId">The ID of the product to remove quantity from.</param>
        /// <param name="quantity">The quantity to remove from the product.</param>
        /// <param name="employeeId">The ID of the employee performing the operation.</param>
        /// <returns>True if the quantity was successfully removed, otherwise false.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error updating the product quantity in the database.</exception>
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
                        command.Parameters.AddWithValue("employeid", employeeId);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            //bool success = RegisterProductSale(employeeId, productId, quantity);
                            //return success;
                            return true;
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

        /// <summary>
        /// Checks the availability of a product in the database.
        /// </summary>
        /// <param name="productId">The ID of the product to check availability for.</param>
        /// <returns>The available quantity of the product.</returns>
        /// <exception cref="ApplicationException">Thrown when there is an error checking product availability in the database.</exception>
        public int CheckProductAvailability(int productId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT quantity FROM products WHERE id = @id";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", productId);
                        int availableQuantity = (int)command.ExecuteScalar();
                        return availableQuantity;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error checking product availability in the database", ex);
            }
        }

        #endregion
        #region db Booking session
        /// <summary>
        /// Retrieves a list of employees suitable for training sessions from the database.
        /// </summary>
        /// <returns>A list of Employee objects representing employees suitable for training sessions.</returns>
        public List<Employee> GetEmployeesForTraining()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM employees";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            List<Employee> employees = new List<Employee>();
                            while (reader.Read())
                            {
                                Employee employee = new Employee
                                {
                                    Id = reader.GetInt32(0),
                                    FirstName = reader.GetString(3),
                                    LastName = reader.GetString(4)
                                };
                                employees.Add(employee);
                            }
                            return employees;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving employees from the database", ex);
            }
        }
        /// <summary>
        /// Retrieves a list of clients suitable for training sessions from the database.
        /// </summary>
        /// <returns>A list of Client objects representing clients suitable for training sessions.</returns>
        public List<Client> GetClientsForTraining()
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
                                    LastName = reader.GetString(2)
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
                throw new ApplicationException("Error retrieving clients from the database", ex);
            }
        }
        /// <summary>
        /// Retrieves a list of available trainings from the database.
        /// </summary>
        /// <returns>A list of Training objects representing available trainings.</returns>
        public List<Training> GetTrainings()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM trainings";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            List<Training> trainings = new List<Training>();
                            while (reader.Read())
                            {
                                Training training = new Training
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                };
                                trainings.Add(training);
                            }
                            return trainings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving trainings from the database", ex);
            }
        }
        /// <summary>
        /// Books a training session in the database.
        /// </summary>
        /// <param name="trainingId">The ID of the training.</param>
        /// <param name="clientId">The ID of the client.</param>
        /// <param name="employeeId">The ID of the employee.</param>
        /// <param name="date">The date and time of the training session.</param>
        /// <returns>True if the booking was successful, otherwise false.</returns>
        public bool BookTrainingSession(int trainingId, int clientId, int employeeId, DateTime date)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO training_sessions (training_id, client_id, employee_id, date_time) VALUES (@trainingId, @clientId, @employeeId, @date)";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@trainingId", trainingId);
                        command.Parameters.AddWithValue("@clientId", clientId);
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        command.Parameters.AddWithValue("@date", date);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error booking training session in the database", ex);
            }
        }
        /// <summary>
        /// Retrieves a list of training sessions from the database.
        /// </summary>
        /// <returns>A list of TrainingSession objects representing training sessions.</returns>
        public List<TrainingSession> GetTrainingSessions()
        {
            var trainingSessions = new List<TrainingSession>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT ts.id, ts.training_id, t.name as training_name, c.firstname as client_firstname, e.firstname as employee_firstname, ts.date_time
            FROM public.training_sessions ts
            JOIN public.trainings t ON ts.training_id = t.id
            JOIN public.clients c ON ts.client_id = c.id
            JOIN public.employees e ON ts.employee_id = e.id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trainingSessions.Add(new TrainingSession
                            {
                                Id = reader.GetInt32(0),
                                TrainingId = reader.GetInt32(1),
                                TrainingName = reader.GetString(2),
                                ClientFirstName = reader.GetString(3),
                                EmployeeFirstName = reader.GetString(4),
                                DateTime = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }

            return trainingSessions;
        }
        #endregion
    }
}
