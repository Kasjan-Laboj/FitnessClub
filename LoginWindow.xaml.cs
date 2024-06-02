using Npgsql;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FitnessClub
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            var dbConnection = new DatabaseConnection();
            int employeeId = dbConnection.AuthenticateUser(username, password);

            if (employeeId != -1)
            {
                UserSession.CurrentEmployeeId = employeeId; // Ustawienie identyfikatora pracownika w sesji
                MessageBox.Show("Zalogowano pomyślnie!");

                // Przejście do głównego okna aplikacji (jeśli takie istnieje)
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                MessageBox.Show("Nieprawidłowy login lub hasło.");
            }
        }
    }
}
