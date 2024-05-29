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
            List<Product> productList = dbConnection.GetProducts();
            ProductComboBox.ItemsSource = productList;
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

        private void RefreshProductList()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            List<Product> productList = dbConnection.GetProducts();
            productList.Sort((x, y) => x.Id.CompareTo(y.Id));
            ProductDataGrid.ItemsSource = productList;
        }

        #region Shop 

        private List<Product> productsInCart = new List<Product>();
    

        private void RefreshCart()
        {
            CartDataGrid.ItemsSource = null;
            CartDataGrid.ItemsSource = productsInCart;
            CalculateTotalPrice();
        }

        private void CalculateTotalPrice()
        {
            decimal totalPrice = 0;
            foreach (Product product in productsInCart)
            {
                totalPrice += product.Price * product.Quantity; // Pomnóż cenę przez ilość
            }
            TotalPriceTextBlock.Text = totalPrice.ToString("C");
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Pobierz ilość każdego produktu z koszyka i usuń z bazy danych
            foreach (Product product in productsInCart)
            {
                DatabaseConnection dbConnection = new DatabaseConnection();
                bool success = dbConnection.RemoveProductQuantity(product.Id, product.Quantity);
                if (!success)
                {
                    MessageBox.Show($"Failed to update product quantity for {product.Name}.");
                    return;
                }
            }

            MessageBox.Show($"Total Price: {TotalPriceTextBlock.Text}");
            // Wyczyść koszyk
            productsInCart.Clear();

            // Odśwież stan sklepu
            RefreshProductList();
            RefreshCart();

            // Wyświetl cenę łączną

            // Zresetuj cenę łączną
            TotalPriceTextBlock.Text = "0";
        }

        private void RemoveFromCartButton_Click(object sender, RoutedEventArgs e)
        {
            // Pobierz zaznaczony produkt z koszyka
            Product selectedProduct = (Product)CartDataGrid.SelectedItem;

            // Usuń zaznaczony produkt z listy koszyka
            productsInCart.Remove(selectedProduct);

            // Odśwież widok koszyka
            RefreshCart();
        }

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

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is Product selectedProduct)
            {
                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity.");
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

    }
}
