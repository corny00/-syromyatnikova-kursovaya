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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GBDD.Pages
{
    /// <summary>
    /// Логика взаимодействия для MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        public MenuPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }
        private void LoadUserInfo()
        {

            if (App.CurrentUser != null)
            {
                lblUserName.Text = App.CurrentUser.FullName;
                lblUserRole.Text = GetRoleName(App.CurrentUser.RoleId);
                int role = App.CurrentUser.RoleId;

                if (role == 3) 
                {
                    btnReports.Visibility = Visibility.Collapsed;
                    btnAccept.Visibility = Visibility.Collapsed;
                }

                if (role != 1)
                {
                    btnAdmin.Visibility = Visibility.Collapsed;
                }
            }
        }

        private string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case 1: return "Администратор";
                case 2: return "Диспетчер";
                case 3: return "Инспектор";
                default: return "Пользователь";
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var main = Window.GetWindow(this) as MainWindow;
            main?.MainFrame.Navigate(new DashboardPage());
            HighlightButton(btnDashboard);
        }

        private void AcceptCar_Click(object sender, RoutedEventArgs e)
        {
            var main = Window.GetWindow(this) as MainWindow;
            main?.MainFrame.Navigate(new AcceptCarPage());
            HighlightButton(btnAccept);

        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var main = Window.GetWindow(this) as MainWindow;
            main?.MainFrame.Navigate(new ReportsPage());
            HighlightButton(btnReports);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Завершить сеанс?", "Выход из системы",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                App.CurrentUser = null;

                var main = Window.GetWindow(this) as MainWindow;
                if (main != null)
                {
                    main.MenuFrame.Visibility = Visibility.Collapsed;
                    main.MainFrame.Navigate(new LoginPage());
                }
            }
        }
        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            var main = Window.GetWindow(this) as MainWindow;
            main?.MainFrame.Navigate(new AdminPage());
            HighlightButton(btnAdmin);
        }

        private void HighlightButton(Button activeButton)
        {

            btnDashboard.Background = null;
            btnAccept.Background = null;
            btnReports.Background = null;
            btnAdmin.Background = null;

            activeButton.Background = (Brush)new BrushConverter().ConvertFromString("#E3F2FD");
        }
    }
}
