using GBDD.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

namespace GBDD.Pages
{
    /// <summary>
    /// Логика взаимодействия для AcceptCarPage.xaml
    /// </summary>
    public partial class AcceptCarPage : Page
    {
        private int _registryId = 0;
        private readonly AcceptCarRepository _repo;
        private List<int> _selectedZoneIds = new List<int>();
        private Dictionary<int, CheckBox> _checklistControls = new Dictionary<int, CheckBox>();


        public AcceptCarPage(int registryId = 0)
        {
            InitializeComponent();
            _registryId = registryId;
            _repo = new AcceptCarRepository();

            lblMode.Text = _registryId > 0 ? "Редактирование записи" : "Оформление нового поступления";
            btnSave.Content = _registryId > 0 ? "Обновить" : "Принять на стоянку";

            LoadDropdowns();
            InitializeDamageZones();
            LoadChecklistControls();

            if (_registryId > 0)
                LoadCarData(_registryId);
        }

        private void LoadDropdowns()
        {
            cmbCategory.ItemsSource = _repo.GetVehicleCategories().DefaultView;
            cmbMake.ItemsSource = _repo.GetCarMakes().DefaultView;
            cmbColor.ItemsSource = _repo.GetCarColors().DefaultView;
            cmbInspector.ItemsSource = _repo.GetInspectors().DefaultView;
            cmbTowTruck.ItemsSource = _repo.GetTowTrucks().DefaultView;
            cmbOwner.ItemsSource = _repo.GetVehicleOwners().DefaultView;
            cmbOwner.SelectedIndex = -1;
        }

        private void CmbTowTruck_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTowTruck.SelectedItem is DataRowView row)
                txtTowDriver.Text = row["DriverName"].ToString();
            else
                txtTowDriver.Clear();
        }

        private void InitializeDamageZones()
        {
            var zones = _repo.GetDamageZones();
            ugDamageZones.Children.Clear();

            foreach (DataRow row in zones.Rows)
            {
                var btn = new Button
                {
                    Content = row["DisplayName"],
                    Tag = row["ZoneId"],
                    Style = Application.Current.Resources["SmallButtonStyle"] as Style,
                    Background = new SolidColorBrush(Color.FromRgb(238, 238, 238)),
                    Margin = new Thickness(3),
                    Height = 60,
                    FontWeight = FontWeights.Medium
                };
                btn.Click += DamageZone_Click;
                ugDamageZones.Children.Add(btn);
            }
        }

        private void DamageZone_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int zoneId = Convert.ToInt32(btn.Tag);

            if (_selectedZoneIds.Contains(zoneId))
            {
                _selectedZoneIds.Remove(zoneId);
                btn.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                btn.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                _selectedZoneIds.Add(zoneId);
                btn.Background = new SolidColorBrush(Color.FromRgb(255, 183, 77));
                btn.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void LoadChecklistControls()
        {
            var items = _repo.GetChecklistItems();
            spChecklist.Children.Clear();
            _checklistControls.Clear();

            foreach (DataRow row in items.Rows)
            {
                var chk = new CheckBox
                {
                    Content = row["ItemName"].ToString(),
                    Tag = row["ItemId"],
                    Margin = new Thickness(0, 0, 0, 6),
                    FontSize = 13
                };
                spChecklist.Children.Add(chk);
                _checklistControls[Convert.ToInt32(row["ItemId"])] = chk;
            }
        }

        private void LoadCarData(int id)
        {
            var record = _repo.GetCarRecord(id);
            if (record == null) return;

            txtPlate.Text = record.LicensePlate;
            txtVin.Text = record.VIN;
            cmbCategory.SelectedValue = record.CategoryId;
            cmbMake.SelectedValue = record.MakeId;

            cmbModel.ItemsSource = _repo.GetCarModelsByMake(record.MakeId).DefaultView;
            cmbModel.SelectedValue = record.ModelId;

            cmbColor.SelectedValue = record.ColorId;
            txtProtocol.Text = record.ProtocolNumber;
            cmbInspector.SelectedValue = record.InspectorUserId;
            txtArticle.Text = record.LegalArticle;
            cmbTowTruck.SelectedValue = record.TowTruckId;
            cmbOwner.SelectedValue = record.OwnerId;

            _selectedZoneIds = _repo.GetSelectedDamageZones(id);
            UpdateDamageZonesUI();

            var checklistValues = _repo.GetChecklistValues(id);
            foreach (var kvp in checklistValues)
            {
                if (_checklistControls.TryGetValue(kvp.Key, out var chk))
                    chk.IsChecked = kvp.Value;
            }
        }

        private void UpdateDamageZonesUI()
        {
            foreach (Button btn in ugDamageZones.Children)
            {
                int zoneId = Convert.ToInt32(btn.Tag);
                if (_selectedZoneIds.Contains(zoneId))
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(255, 183, 77));
                    btn.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                    btn.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }


        private int? GetSelectedId(ComboBox cmb)
        {
            if (cmb.SelectedValue == null || cmb.SelectedValue == DBNull.Value)
                return null;

            if (cmb.SelectedValue is DataRowView rowView)
                return Convert.ToInt32(rowView[cmb.SelectedValuePath]);

            return Convert.ToInt32(cmb.SelectedValue);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlate.Text) || cmbMake.SelectedValue == null || cmbModel.SelectedValue == null)
            {
                MessageBox.Show("Заполните обязательные поля: Госномер, Марка, Модель", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var model = new CarRecordSaveModel
                {
                    RegistryId = _registryId,
                    LicensePlate = txtPlate.Text.Trim(),
                    VIN = txtVin.Text.Trim(),
                    MakeId = GetSelectedId(cmbMake),
                    ModelId = GetSelectedId(cmbModel),
                    ColorId = GetSelectedId(cmbColor),
                    CategoryId = GetSelectedId(cmbCategory),
                    ProtocolNumber = txtProtocol.Text.Trim(),
                    InspectorUserId = GetSelectedId(cmbInspector),
                    LegalArticle = txtArticle.Text.Trim(),
                    TowTruckId = GetSelectedId(cmbTowTruck),
                    OwnerId = cmbOwner.SelectedValue as int?,
                    StatusId = 1,
                    CreatedByUserId = App.CurrentUser?.UserId ?? 1
                };

                var checklistValues = _checklistControls
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsChecked == true);

                int savedId = _repo.SaveCarRecord(model, _selectedZoneIds, checklistValues);

                MessageBox.Show("Автомобиль успешно сохранён!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                var main = Window.GetWindow(this) as MainWindow;
                main?.MainFrame.Navigate(new DashboardPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbMake_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMake.SelectedValue == null) return;
            int makeId = Convert.ToInt32(cmbMake.SelectedValue);
            cmbModel.ItemsSource = _repo.GetCarModelsByMake(makeId).DefaultView;
            cmbModel.SelectedIndex = -1;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var main = Window.GetWindow(this) as MainWindow;
            main?.MainFrame.Navigate(new DashboardPage());
        }
    }
}

