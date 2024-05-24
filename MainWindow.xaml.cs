using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }
        private void LoadClients()
        {
            ClientDataGrid.ItemsSource = dbConnection.GetClients();
        }
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
        private void RefreshClientList()
        {
            // Pobierz nową listę klientów z bazy danych
            List<Client> clients = dbConnection.GetClients();

            // Przypisz nową listę klientów do ItemsSource DataGrid
            ClientDataGrid.ItemsSource = clients;
        }
        private void PassLengthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateEndDate();
        }

        private void StartDateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateEndDate();
        }

        private void CalculateEndDate()
        {
            if (StartDateCalendar.SelectedDate != null && int.TryParse((PassLengthComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), out int months))
            {
                DateTime startDate = StartDateCalendar.SelectedDate.Value;
                DateTime endDate = startDate.AddMonths(months);
                EndDateTextBlock.Text = endDate.ToString("d MMM yyyy");

                // Ustawianie ceny karnetu w zależności od długości
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
                PassPriceTextBox.Text = totalPrice.ToString("F2"); // formatowanie jako liczba z dwoma miejscami dziesiętnymi
            }
            else
            {
                EndDateTextBlock.Text = string.Empty;
                PassPriceTextBox.Text = string.Empty;
            }
        }
        private void ClearInputFields()
        {
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            StartDateCalendar.SelectedDate = null;
            PassLengthComboBox.SelectedIndex = -1;
            PassPriceTextBox.Text = string.Empty;
            EndDateTextBlock.Text = string.Empty;
        }

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

        private void AddQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            //Product selectedProduct = (Product)ProductDataGrid.SelectedItem;

            //if (selectedProduct != null)
            //{
            //    // Pobierz ilość, którą chcesz dodać
            //    int quantityToAdd = 1; // Możesz zmienić tę wartość na dowolną inną

            //    // Dodaj ilość do wybranego produktu
            //    selectedProduct.Quantity += quantityToAdd;

            //    // Zaktualizuj produkt w bazie danych
            //    DatabaseConnection dbConnection = new DatabaseConnection();
            //    bool success = dbConnection.UpdateProduct(selectedProduct);

            //    if (success)
            //    {
            //        RefreshProductList(); // Odśwież listę produktów
            //    }
            //    else
            //    {
            //        MessageBox.Show("Failed to update product quantity.");
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Please select a product first.");
            //}


            var selectedProduct = ProductDataGrid.SelectedItem as Product;
            if (selectedProduct == null)
            {
                MessageBox.Show("Please select a product to add quantity to.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            selectedProduct.Quantity++;

            bool success = dbConnection.UpdateProduct(selectedProduct);

            if (success)
            {
                //RefreshProductList();
                ProductDataGrid.Items.Refresh();
            }
        }

        private void RemoveQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            //Product selectedProduct = (Product)ProductDataGrid.SelectedItem;

            //if (selectedProduct != null)
            //{
            //    // Pobierz ilość, którą chcesz odjąć
            //    int quantityToRemove = 1; // Możesz zmienić tę wartość na dowolną inną

            //    // Sprawdź, czy ilość do odjęcia nie przekracza aktualnej ilości produktu
            //    if (selectedProduct.Quantity >= quantityToRemove)
            //    {
            //        // Odejmij ilość od wybranego produktu
            //        selectedProduct.Quantity -= quantityToRemove;

            //        // Zaktualizuj produkt w bazie danych
            //        DatabaseConnection dbConnection = new DatabaseConnection();
            //        bool success = dbConnection.UpdateProduct(selectedProduct);

            //        if (success)
            //        {
            //            RefreshProductList(); // Odśwież listę produktów
            //        }
            //        else
            //        {
            //            MessageBox.Show("Failed to update product quantity.");
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Cannot remove more quantity than available.");
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Please select a product first.");
            //}

            var selectedProduct = ProductDataGrid.SelectedItem as Product;
            if (selectedProduct == null)
            {
                MessageBox.Show("Please select a product to remove quantity from.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selectedProduct.Quantity--;

            bool success = dbConnection.UpdateProduct(selectedProduct);

            if (success)
            {
                ProductDataGrid.Items.Refresh();
            }

        }

        private void RefreshProductList()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            List<Product> products = dbConnection.GetProducts();
            ProductDataGrid.ItemsSource = products;
        }
    }
}
