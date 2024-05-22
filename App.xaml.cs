using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FitnessClub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginWindow = new LoginWindow();
            bool? result = loginWindow.ShowDialog();

            if (result == true) // Jeśli logowanie się powiodło
            {
                var mainWindow = new MainWindow();
                mainWindow.Show(); // Wyświetl główne okno aplikacji
            }
            else
            {
                Shutdown(); // Zamknij aplikację, jeśli logowanie się nie powiodło lub zostało anulowane
            }
        }
    }
}
