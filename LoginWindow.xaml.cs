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
        /// <summary>
        /// Button to login into application as employee
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            var dbConnection = new DatabaseConnection();
            int employeeId = dbConnection.AuthenticateUser(username, password);

            if (employeeId != -1)
            {
                UserSession.CurrentEmployeeId = employeeId; // Setting up employee id after login
                MessageBox.Show("Login succesfull!");

                // Switching windows from login to main
                var mainWindow = new MainWindow();
                mainWindow.Show();
                PasswordBox.Clear();
            }
            else
            {
                MessageBox.Show("Wrong login or password.");
            }
        }
    }
}
