using GBDD.Data;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private string _currentFilter = "All";
        private string _currentSearch = "";
        private readonly DashboardRepository _dashRepo;

        public DashboardPage()
        {
            InitializeComponent();
            _dashRepo = new DashboardRepository();
            LoadDashboardData();
            if (App.CurrentUser.RoleId == 3)
            {
                btnAcceptCar.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                var occDt = _dashRepo.GetOccupancyStats();
                if (occDt.Rows.Count > 0)
                {
                    int occupied = Convert.ToInt32(occDt.Rows[0]["Occupied"]);
                    int total = Convert.ToInt32(occDt.Rows[0]["TotalCapacity"]);
                    double percent = total > 0 ? (occupied * 100.0 / total) : 0;

                    txtOccupancy.Text = $"{occupied} / {total} мест";
                    pbOccupancy.Value = percent;
                    pbOccupancy.Maximum = 100;
                }

                var dailyDt = _dashRepo.GetDailyStats();
                if (dailyDt.Rows.Count > 0)
                {
                    txtArrived.Text = dailyDt.Rows[0]["ArrivedToday"].ToString();
                    txtReleased.Text = dailyDt.Rows[0]["ReleasedToday"].ToString();
                }

                int debtorsCount = _dashRepo.GetDebtorsCount();
                txtDebtors.Text = $"{debtorsCount} авто";


                LoadCarsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дашборда: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCarsGrid()
        {

            try
            {
                DataTable dt;
                if (_currentFilter == "Debtors")
                {
                    dt = _dashRepo.GetDebtorsList();
                }
                else
                {
                    dt = _dashRepo.GetAllCars(_currentFilter, _currentSearch);
                }

                dgCars.ItemsSource = dt.DefaultView;

                if (App.CurrentUser.RoleId == 3) 
                {
                    if (dgCars.Columns.Count > 0)
                    {
                        dgCars.Columns[dgCars.Columns.Count - 1].Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки таблицы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e) => ApplyFilters();
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ApplyFilters();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                _currentFilter = btn.Tag.ToString();

                if (btn.Parent is StackPanel panel)
                {
                    foreach (Button b in panel.Children)
                    {
                        if (b.Tag is string tag)
                            b.Background = (Brush)new BrushConverter().ConvertFromString(tag == "Debtors" ? "#E53935" : "#1976D2");
                    }
                }
                btn.Background = (Brush)new BrushConverter().ConvertFromString("#0D47A1");
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            _currentSearch = txtSearch.Text.Trim();
            LoadCarsGrid(); 
        }

        private void DebtorsWidget_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFilter = "Debtors";
            ApplyFilters();
        }


        private void AcceptCar_Click(object sender, RoutedEventArgs e)
        {

            MainWindow parent = Window.GetWindow(this) as MainWindow;
            parent?.MainFrame.Navigate(new AcceptCarPage()); 
        }


        private void EditCar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int registryId)
            {
                MainWindow parent = Window.GetWindow(this) as MainWindow;

                parent?.MainFrame.Navigate(new AcceptCarPage(registryId));
            }
        }

        private void ArchiveCar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int registryId)
            {
                var result = MessageBox.Show("Перевести автомобиль в архив?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                       
                        bool success = _dashRepo.ArchiveCar(registryId);

                        if (success)
                        {
                            MessageBox.Show("Автомобиль архивирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadDashboardData();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось архивировать. Возможно, автомобиль уже не на участке.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgCars_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCars.SelectedItem is DataRowView row)
            {
                int id = Convert.ToInt32(row["RegistryId"]);
                MainWindow parent = Window.GetWindow(this) as MainWindow;
                parent?.MainFrame.Navigate(new CarCardPage(id)); 
            }
        }
    }
}
