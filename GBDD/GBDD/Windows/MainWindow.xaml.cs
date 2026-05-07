using GBDD.Pages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using Xceed.Document.NET;

namespace GBDD
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Licenser.LicenseKey = "WDN52-KWNUK-548M5-34SA";
            InitializeComponent();
            
            MenuFrame.Visibility = Visibility.Collapsed;
            TestConnection();
            MainFrame.Navigate(new LoginPage());
        }

    public void ShowMenu()
        {
            MenuFrame.Visibility = Visibility.Visible;
            MenuFrame.Navigate(new MenuPage());
        }

        private void TestConnection()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
