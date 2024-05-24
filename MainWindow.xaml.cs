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
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas dodawania klienta.");
            }
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
    }
}
