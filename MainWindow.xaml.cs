using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FitnessClub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseConnection dbConnection;
        public MainWindow()
        {
            InitializeComponent();
            dbConnection = new DatabaseConnection();
            LoadClients();
            RefreshProductList();
            List<Product> productList = dbConnection.GetProducts();
            ProductComboBox.ItemsSource = productList;
            LoadBookingData();
            LoadTrainingSessions();

        }

        #region ClientPass
        /// <summary>
        /// Handles the click event for adding a new client to the database.
        /// Validates input for client details, calculates the end date of the pass,
        /// adds the client to the database, clears input fields, and refreshes the client list.
        /// </summary>
        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            DateTime startDate = StartDateCalendar.SelectedDate ?? DateTime.Now;

            if (!int.TryParse((PassLengthComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), out int passLength))
            {
                MessageBox.Show("Proszę wybrać długość karnetu.");
                return;
            }

            if (!decimal.TryParse(PassPriceTextBox.Text, out decimal passPrice))
            {
                MessageBox.Show("Cena karnetu musi być liczbą.");
                return;
            }

            DateTime endDate = startDate.AddMonths(passLength);

            var dbConnection = new DatabaseConnection();
            bool success = dbConnection.AddClient(firstName, lastName, startDate, endDate, passLength, passPrice);

            if (success)
            {
                MessageBox.Show("Klient został dodany do bazy.");
                ClearInputFields();
                RefreshClientList();
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas dodawania klienta.");
            }
        }
        /// <summary>
        /// Handles the selection changed event for the pass length combo box.
        /// Calculates the end date of the pass when the selection changes.
        /// </summary>
        private void PassLengthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateEndDate();
        }
        /// <summary>
        /// Handles the selected dates changed event for the start date calendar.
        /// Calculates the end date of the pass when the start date changes.
        /// </summary>
        private void StartDateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateEndDate();
        }
        /// <summary>
        /// Calculates the end date of the pass based on the selected start date and pass length,
        /// and updates the pass price accordingly.
        /// </summary>
        private void CalculateEndDate()
        {
            if (StartDateCalendar.SelectedDate != null && int.TryParse((PassLengthComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), out int months))
            {
                DateTime startDate = StartDateCalendar.SelectedDate.Value;
                DateTime endDate = startDate.AddMonths(months);
                EndDateTextBlock.Text = endDate.ToString("d MMM yyyy");

                // Price of pass depends of how many months clients want to buy
                decimal pricePerMonth;
                if (months <= 6)
                {
                    pricePerMonth = 250;
                }
                else if (months < 12)
                {
                    pricePerMonth = 220;
                }
                else
                {
                    pricePerMonth = 180;
                }

                decimal totalPrice = pricePerMonth * months;
                PassPriceTextBox.Text = totalPrice.ToString("F2"); // after the decimal point (if needed we have integers)
            }
            else
            {
                EndDateTextBlock.Text = string.Empty;
                PassPriceTextBox.Text = string.Empty;
            }

        }
        /// <summary>
        /// Clears all input fields related to adding a new client.
        /// </summary>
        private void ClearInputFields()
        {
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            StartDateCalendar.SelectedDate = null;
            PassLengthComboBox.SelectedIndex = -1;
            PassPriceTextBox.Text = string.Empty;
            EndDateTextBlock.Text = string.Empty;
        }
        /// <summary>
        /// Refreshes the client list by retrieving the latest clients from the database
        /// and updating the client data grid.
        /// </summary>
        private void RefreshClientList()
        {
            // Pobierz nową listę klientów z bazy danych
            List<Client> clients = dbConnection.GetClients();

            // Przypisz nową listę klientów do ItemsSource DataGrid
            ClientDataGrid.ItemsSource = clients;
        }
        #endregion
        #region ClientList
        /// <summary>
        /// Loads clients from the database and assigns them to the data grid for display.
        /// </summary>
        private void LoadClients()
        {
            ClientDataGrid.ItemsSource = dbConnection.GetClients();
        }
        #endregion
        #region Warehouse
        /// <summary>
        /// Handles the click event for adding a new product to the database.
        /// Validates input for product name, quantity, and price, adds the product to the database,
        /// and refreshes the product list.
        /// </summary>
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            string productName = NewProductNameTextBox.Text;
            int quantity;
            decimal price;

            if (!int.TryParse(NewProductQuantityTextBox.Text, out quantity))
            {
                MessageBox.Show("Quantity must be a valid integer.");
                return;
            }

            if (!decimal.TryParse(NewProductPriceTextBox.Text, out price))
            {
                MessageBox.Show("Price must be a valid integer.");
                return;
            }

            // Dodaj nowy produkt do bazy danych
            DatabaseConnection dbConnection = new DatabaseConnection();
            bool success = dbConnection.AddProduct(productName, quantity, price);
            if (success)
            {
                MessageBox.Show("Product added successfully.");
                RefreshProductList();
            }
            else
            {
                if (dbConnection.ProductExists(productName))
                {
                    MessageBox.Show("Product already exists");
                }
                else
                    MessageBox.Show("Failed to add product.");
            }
        }
        /// <summary>
        /// Handles the click event for adding quantity to a selected product.
        /// Validates input for quantity, updates the product quantity in the database,
        /// and refreshes the product list.
        /// </summary>
        private void AddQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductDataGrid.SelectedItem is Product selectedProduct)
            {
                if (int.TryParse(QuantityToModifyTextBox.Text, out int quantityToAdd))
                {
                    DatabaseConnection dbConnection = new DatabaseConnection();
                    selectedProduct.Quantity += quantityToAdd;
                    dbConnection.UpdateProduct(selectedProduct);
                    RefreshProductList();
                }
                else
                {
                    MessageBox.Show("Please enter a valid quantity to add.");
                }
            }
            else
            {
                MessageBox.Show("Please select a product.");
            }       
        }
        /// <summary>
        /// Handles the click event for removing quantity from a selected product.
        /// Validates input for quantity, updates the product quantity in the database,
        /// and refreshes the product list.
        /// </summary>
        private void RemoveQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductDataGrid.SelectedItem is Product selectedProduct)
            {
                if (int.TryParse(QuantityToModifyTextBox.Text, out int quantityToRemove))
                {
                    DatabaseConnection dbConnection = new DatabaseConnection();
                    selectedProduct.Quantity = Math.Max(0, selectedProduct.Quantity - quantityToRemove);
                    dbConnection.UpdateProduct(selectedProduct);
                    RefreshProductList();
                }
                else
                {
                    MessageBox.Show("Please enter a valid quantity to remove.");
                }
            }
            else
            {
                MessageBox.Show("Please select a product.");
            }
        }
        /// <summary>
        /// Refreshes the product list by retrieving the latest products from the database
        /// and updating the product combo box.
        /// </summary>
        private void RefreshProductList()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            List<Product> productList = dbConnection.GetProducts();
            productList.Sort((x, y) => x.Id.CompareTo(y.Id));
            ProductDataGrid.ItemsSource = productList;
        }

        #endregion
        #region Shop 
        /// <summary>
        /// List to hold products added to the shopping cart.
        /// </summary>
        private List<Product> productsInCart = new List<Product>();

        /// <summary>
        /// Refreshes the shopping cart by updating the data grid with the products in the cart
        /// and recalculating the total price.
        /// </summary>
        private void RefreshCart()
        {
            CartDataGrid.ItemsSource = null;
            CartDataGrid.ItemsSource = productsInCart;
            CalculateTotalPrice();
        }
        /// <summary>
        /// Calculates the total price of all products in the shopping cart and updates the TotalPriceTextBlock.
        /// </summary>
        private void CalculateTotalPrice()
        {
            decimal totalPrice = 0;
            foreach (Product product in productsInCart)
            {
                totalPrice += product.Price * product.Quantity; // Pomnóż cenę przez ilość
            }
            TotalPriceTextBlock.Text = totalPrice.ToString("C");
        }
        /// <summary>
        /// Finalising transaction in cart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.CurrentEmployeeId == -1)
            {
                MessageBox.Show("Musisz się zalogować, aby dokonać sprzedaży.");
                return;
            }

            int employeeId = UserSession.CurrentEmployeeId;

            // Take products from warehose and delete accure amount of them in database
            foreach (Product product in productsInCart)
            {
                var dbConnection = new DatabaseConnection();
                bool success = dbConnection.RemoveProductQuantity(product.Id, product.Quantity, employeeId);
                if (!success)
                {
                    MessageBox.Show($"Failed to update product quantity for {product.Name}.");
                    return;
                }
            }

            MessageBox.Show($"Total Price: {TotalPriceTextBlock.Text}");


            productsInCart.Clear();


            RefreshProductList();
            RefreshCart();

            // Refreshing total price
            TotalPriceTextBlock.Text = "0";
        }
        /// <summary>
        /// Handles the click event for removing a product from the shopping cart.
        /// </summary>
        private void RemoveFromCartButton_Click(object sender, RoutedEventArgs e)
        {
            // Take choosen product from cart
            Product selectedProduct = (Product)CartDataGrid.SelectedItem;

            // Delete choosen product from cart
            productsInCart.Remove(selectedProduct);

            // Refresh view
            RefreshCart();
        }
        /// <summary>
        /// Handles the TextChanged event for the quantity text box.
        /// Updates the quantity of the selected product in the cart and refreshes the cart view.
        /// </summary>
        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CartDataGrid.SelectedItem is Product selectedProduct)
            {
                if (int.TryParse(QuantityTextBox.Text, out int quantity))
                {
                    selectedProduct.Quantity = quantity;
                    RefreshCart();
                }
            }
        }
        /// <summary>
        /// Handles the click event for adding a product to the shopping cart.
        /// Checks the availability of the selected product, sets its quantity, and adds it to the cart.
        /// </summary>
        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is Product selectedProduct)
            {
                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity.");
                    return;
                }

                // Sprawdź dostępność produktu w bazie danych
                var dbConnection = new DatabaseConnection();
                int availableQuantity = dbConnection.CheckProductAvailability(selectedProduct.Id);

                if (availableQuantity < quantity)
                {
                    MessageBox.Show($"Insufficient stock. Available quantity: {availableQuantity}. Please enter a valid quantity.");
                    return;
                }

                // Ustaw ilość wybranego produktu
                selectedProduct.Quantity = quantity;

                // Dodaj referencję do wybranego produktu do koszyka
                productsInCart.Add(selectedProduct);

                // Odśwież widok koszyka
                RefreshCart();
            }
            else
            {
                MessageBox.Show("Please select a product to add to cart.");
            }
        }
        #endregion
        #region Booking session
        /// <summary>
        /// Loads booking data such as employees, clients, and trainings from the database
        /// and populates the respective ComboBoxes.
        /// </summary>
        private void LoadBookingData()
        {
            try
            {
                List<Employee> employees = dbConnection.GetEmployeesForTraining();
                EmployeeComboBox.ItemsSource = employees;
                EmployeeComboBox.DisplayMemberPath = "FullName";

                List<Client> clients = dbConnection.GetClientsForTraining();
                ClientComboBox.ItemsSource = clients;
                ClientComboBox.DisplayMemberPath = "FullName";

                List<Training> trainings = dbConnection.GetTrainings();
                TrainingComboBox.ItemsSource = trainings;
                TrainingComboBox.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading booking data: {ex.Message}");
            }
        }
        /// <summary>
        /// Handles the button click event for booking a training session.
        /// Validates selected fields from ComboBoxes and DatePicker,
        /// adds a new training session to the database, and refreshes the training sessions data grid.
        /// </summary>
        private void BookTrainingSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Training selectedTraining = (Training)TrainingComboBox.SelectedItem;
                Client selectedClient = (Client)ClientComboBox.SelectedItem;
                Employee selectedEmployee = (Employee)EmployeeComboBox.SelectedItem;
                DateTime selectedDate = TrainingDatePicker.SelectedDate ?? DateTime.Now;

                if (selectedTraining == null || selectedClient == null || selectedEmployee == null || selectedDate == null)
                {
                    MessageBox.Show("Please select all fields.");
                    return;
                }

                DatabaseConnection dbConnection = new DatabaseConnection();
                bool success = dbConnection.BookTrainingSession(selectedTraining.Id, selectedClient.Id, selectedEmployee.Id, selectedDate);

                if (success)
                {
                    MessageBox.Show("Training session booked successfully.");
                    TrainingComboBox.SelectedIndex = -1;
                    ClientComboBox.SelectedIndex = -1;
                    EmployeeComboBox.SelectedIndex = -1;
                    TrainingDatePicker.SelectedDate = null;

                    LoadTrainingSessions();
                }
                else
                {
                    MessageBox.Show("Failed to book training session.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        #endregion
        #region ListOfTrainings 
        /// <summary>
        /// Loads the training sessions from the database and assigns them to the data grid.
        /// </summary>
        private void LoadTrainingSessions()
        {
            var trainingSessions = dbConnection.GetTrainingSessions();
            dataGridTrainings.ItemsSource = trainingSessions;
        }
        #endregion
    }
}
