using GBDD.Models;
using GBDD.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
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
using System.Windows.Threading;

namespace GBDD.Pages
{
    /// <summary>
    /// Логика взаимодействия для CarCardPage.xaml
    /// </summary>
    public partial class CarCardPage : Page
    {
        private readonly int _registryId;
        private readonly string _connString;
        private DispatcherTimer _timer;
        private DateTime _intakeDate;
        private DateTime? _releaseDate;
        private decimal _hourlyRate;
        private decimal _towCost;
        private decimal? _dailyCap;
        public CarCardPage(int registryId)
        {
            InitializeComponent();

            _registryId = registryId;
            _connString = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;

            LoadCarData();
            StartTimer();
        }
        private void LoadCarData()
        {

            string q = @"
                SELECT 
                    er.*, 
                    cm.MakeName, cmo.ModelName, cc.ColorName, vc.Code as CategoryCode,
                    vo.FullName as OwnerName, vo.Phone as OwnerPhone, vo.PassportSeriesNumber, vo.DriverLicenseNumber,
                    s.StatusName
                FROM EvacuationRegistry er
                INNER JOIN CarMakes cm ON er.MakeId = cm.MakeId
                INNER JOIN CarModels cmo ON er.ModelId = cmo.ModelId
                INNER JOIN CarColors cc ON er.ColorId = cc.ColorId
                INNER JOIN VehicleCategories vc ON er.CategoryId = vc.CategoryId
                LEFT JOIN VehicleOwners vo ON er.OwnerId = vo.OwnerId
                INNER JOIN Statuses s ON er.StatusId = s.StatusId
                WHERE er.RegistryId = @Id";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Id", _registryId);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        lblPlate.Text = reader["LicensePlate"].ToString();
                        lblStatus.Text = reader["StatusName"].ToString();
                        lblPlateDetail.Text = reader["LicensePlate"].ToString();
                        string status = reader["StatusName"].ToString();
                        if (status == "На участке") brdStatus.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F5E9"));
                        else if (status == "Освобожден") brdStatus.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E3F2FD"));
                        else brdStatus.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F5F5F5"));

                        lblVin.Text = reader["VIN"].ToString();
                        lblMakeModel.Text = $"{reader["MakeName"]} {reader["ModelName"]}";
                        lblCatColor.Text = $"Категория {reader["CategoryCode"]} / {reader["ColorName"]}";

                        lblOwnerName.Text = reader["OwnerName"] != DBNull.Value ? reader["OwnerName"].ToString() : "Не указан";
                        lblOwnerPhone.Text = reader["OwnerPhone"] != DBNull.Value ? reader["OwnerPhone"].ToString() : "—";
                        string passport = reader["PassportSeriesNumber"] != DBNull.Value ? reader["PassportSeriesNumber"].ToString() : "—";
                        string license = reader["DriverLicenseNumber"] != DBNull.Value ? reader["DriverLicenseNumber"].ToString() : "—";
                        lblOwnerDocs.Text = $"Паспорт: {passport}\nВУ: {license}";

                        _intakeDate = Convert.ToDateTime(reader["IntakeDate"]);
                        _releaseDate = reader["ReleaseDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReleaseDate"]) : (DateTime?)null;

                        if (status == "На участке")
                            btnRelease.Visibility = Visibility.Visible;

                        LoadTariff(Convert.ToInt32(reader["CategoryId"]));


                        LoadPhotos();
                        LoadDamages();
                        LoadChecklist();
                    }
                }
            }
            if (App.CurrentUser.RoleId == 3)
            {
                btnRelease.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadTariff(int categoryId)
        {
            string q = @"SELECT TOP 1 TowCost, HourlyRate, DailyCap FROM Tariffs 
                         WHERE CategoryId = @CatId AND IsActive = 1 
                         AND ValidFrom <= GETDATE() AND (ValidTo IS NULL OR ValidTo >= GETDATE())
                         ORDER BY ValidFrom DESC";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@CatId", categoryId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _towCost = Convert.ToDecimal(reader["TowCost"]);
                        _hourlyRate = Convert.ToDecimal(reader["HourlyRate"]);
                        _dailyCap = reader["DailyCap"] != DBNull.Value ? Convert.ToDecimal(reader["DailyCap"]) : (decimal?)null;

                        lblTariffInfo.Text = $"Эвакуация: {_towCost:C} | Час хранения: {_hourlyRate:C}";
                        if (_dailyCap.HasValue) lblTariffInfo.Text += $" (Лимит в сутки: {_dailyCap.Value:C})";
                    }
                }
            }
        }

        private void UpdateCostAndTimer()
        {
            DateTime endDate = _releaseDate ?? DateTime.Now;
            TimeSpan diff = endDate - _intakeDate;

            lblTimer.Text = $"{diff.Days} дн. {diff.Hours} ч. {diff.Minutes} мин.";


            double totalHours = diff.TotalHours;
            int totalDays = diff.Days;
            decimal storageCost = (decimal)totalHours * _hourlyRate;

            if (_dailyCap.HasValue && totalDays > 0)
            {
                decimal cappedStorage = totalDays * _dailyCap.Value;
                if (storageCost > cappedStorage) storageCost = cappedStorage;
            }

            decimal total = _towCost + storageCost;
            lblTotalCost.Text = total.ToString("C");
        }

        private void StartTimer()
        {
            if (_releaseDate == null)
            {
                _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
                _timer.Tick += (s, e) => UpdateCostAndTimer();
                _timer.Start();
            }
            UpdateCostAndTimer();
        }

        //фото

        private void SavePhotoToDatabase(string filePath)
        {
            string q = "INSERT INTO VehiclePhotos (RegistryId, FilePath, UploadDate) VALUES (@RegId, @Path, GETDATE())";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@RegId", _registryId);
                cmd.Parameters.AddWithValue("@Path", filePath);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private void DeletePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int photoId)
            {
                var result = MessageBox.Show("Удалить это фото?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        
                        string filePath = GetPhotoPath(photoId);

                        
                        if (File.Exists(filePath))
                            File.Delete(filePath);

                        
                        DeletePhotoFromDatabase(photoId);

                        LoadPhotos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private string GetPhotoPath(int photoId)
        {
            string q = "SELECT FilePath FROM VehiclePhotos WHERE PhotoId = @Id";
            string relativePath = "";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Id", photoId);
                conn.Open();

                object result = cmd.ExecuteScalar();
                if (result != null)
                    relativePath = result.ToString();
            }

            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        private void DeletePhotoFromDatabase(int photoId)
        {
            string q = "DELETE FROM VehiclePhotos WHERE PhotoId = @Id";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Id", photoId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        private void AddPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                Title = "Выберите фотографию автомобиля"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourcePath = openFileDialog.FileName;

                    string photosFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos");
                    if (!Directory.Exists(photosFolder))
                        Directory.CreateDirectory(photosFolder);
                    string fileName = $"{_registryId}_{Guid.NewGuid()}{System.IO.Path.GetExtension(sourcePath)}";
                    string destPath = System.IO.Path.Combine(photosFolder, fileName);
                    File.Copy(sourcePath, destPath, true);

                    string relativePath = System.IO.Path.Combine("Photos", fileName);
                    SavePhotoToDatabase(relativePath);
                    LoadPhotos();

                    MessageBox.Show("Фото успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке фото: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadPhotos()
        {
            string q = "SELECT PhotoId, FilePath, UploadDate FROM VehiclePhotos WHERE RegistryId = @Id ORDER BY UploadDate";
            var photos = new List<VehiclePhoto>();

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Id", _registryId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string relative = reader["FilePath"].ToString();
                        string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);


                        BitmapImage bitmap = new BitmapImage();
                        try
                        {
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(fullPath);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad; 
                            bitmap.EndInit();
                            bitmap.Freeze(); 
                        }
                        catch
                        {

                            bitmap = null;
                        }

                        photos.Add(new VehiclePhoto
                        {
                            PhotoId = Convert.ToInt32(reader["PhotoId"]),
                            FilePath = fullPath,
                            UploadDate = Convert.ToDateTime(reader["UploadDate"]),
                            ImageSource = bitmap
                        });
                    }
                }
            }

            if (photos.Count > 0)
            {
                icPhotos.ItemsSource = photos;
                lblNoPhotos.Visibility = Visibility.Collapsed;
            }
            else
            {
                icPhotos.ItemsSource = null;
                lblNoPhotos.Visibility = Visibility.Visible;
            }
        }

        private void LoadDamages()
        {
            string q = @"SELECT dz.DisplayName + ': ' + dt.DisplayName + ISNULL(' (' + d.Description + ')', '') as Info
                         FROM Damages d
                         INNER JOIN DamageZones dz ON d.ZoneId = dz.ZoneId
                         INNER JOIN DamageTypes dt ON d.TypeId = dt.TypeId
                         WHERE d.RegistryId = @Id";

            var list = new List<string>();
            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Id", _registryId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) list.Add(reader["Info"].ToString());
                }
            }
            lstDamages.ItemsSource = list.Count > 0 ? list : new List<string> { "Повреждений не отмечено" };
        }

        private void LoadChecklist()
        {
            string q = @"SELECT cd.ItemName, ic.IsPresent
                         FROM ImpoundChecklist ic
                         INNER JOIN ChecklistDefinitions cd ON ic.ItemId = cd.ItemId
                         WHERE ic.RegistryId = @Id";

            spChecklistView.Children.Clear();
            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Id", _registryId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var txt = new TextBlock
                        {
                            Text = (bool)reader["IsPresent"] ? $" {reader["ItemName"]}" : $"{reader["ItemName"]}",
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 0, 4)
                        };
                        spChecklistView.Children.Add(txt);
                    }
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            var main = Window.GetWindow(this) as MainWindow;
            main?.MainFrame.Navigate(new DashboardPage());
        }

        private void ReleaseCar_Click(object sender, RoutedEventArgs e)
        {

            _timer?.Stop();

            var main = Window.GetWindow(this) as MainWindow;
            main?.MainFrame.Navigate(new ReleaseCarPage(_registryId));
        }
        private void Photo_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string filePath)
            {
                var viewer = new PhotoViewerWindow(filePath);
                viewer.ShowDialog();
            }
        }
    }
}
