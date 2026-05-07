using GBDD.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed;
using Xceed.Document.NET;
using Xceed.Words.NET;
using System.Drawing;
using Xceed.Drawing;
using Xcolor = Xceed.Drawing.Color;
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
    /// Логика взаимодействия для ReleaseCarPage.xaml
    /// </summary>
    public partial class ReleaseCarPage : Page
    {
        private readonly int _registryId;
        private readonly ReleaseCarRepository _repo;
        private decimal _totalAmount;
        private ReleaseCarInfo _carInfo;

        public ReleaseCarPage(int registryId)
        {
            InitializeComponent();
            _registryId = registryId;
            _repo = new ReleaseCarRepository();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
 
            _carInfo = _repo.GetCarInfo(_registryId);
            if (_carInfo == null)
            {
                MessageBox.Show("Запись не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            lblVehicleInfo.Text = string.Format("{0} | Владелец: {1}",
                _carInfo.VehicleInfoShort, _carInfo.OwnerName);

            TariffInfo tariff = _repo.GetActiveTariff(_carInfo.CategoryId);
            if (tariff != null)
            {
                _totalAmount = CalculateCost(_carInfo.IntakeDate, tariff);
                lblTotalAmount.Text = _totalAmount.ToString("C");
            }

            DataTable paymentMethods = _repo.GetPaymentMethods();
            cmbPaymentMethod.ItemsSource = paymentMethods.DefaultView;
            cmbPaymentMethod.SelectedIndex = 0;
        }

        private decimal CalculateCost(DateTime intake, TariffInfo tariff)
        {
            TimeSpan diff = DateTime.Now - intake;
            double hours = diff.TotalHours;
            int days = diff.Days;

            decimal storage = (decimal)hours * tariff.HourlyRate;

            if (tariff.DailyCap.HasValue && days > 0)
            {
                decimal capped = days * tariff.DailyCap.Value;
                if (storage > capped)
                    storage = capped;
            }

            return tariff.TowCost + storage;
        }

        private void chkGibddPermission_Checked(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void chkGibddPermission_Unchecked(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void chkIdentity_Checked(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void chkIdentity_Unchecked(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void chkDocuments_Checked(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void chkDocuments_Unchecked(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            bool allChecked = chkGibddPermission.IsChecked == true &&
                              chkIdentity.IsChecked == true &&
                              chkDocuments.IsChecked == true;

            btnRelease.IsEnabled = allChecked;
            lblValidationStatus.Visibility = allChecked ? Visibility.Collapsed : Visibility.Visible;

            if (!allChecked)
                lblValidationStatus.Text = "Отметьте все пункты для продолжения!";
        }

        private void GenerateActWord()
        {
           
           
            List<string> damages = _repo.GetDamages(_registryId);
            string damagesText = damages.Count > 0 ? string.Join("\n• ", damages) : "Повреждений не обнаружено";
            string protocolInfo = $"№ {_carInfo.ProtocolNumber} от {_carInfo.IntakeDate:dd.MM.yyyy}";
            string today = DateTime.Now.ToString("dd.MM.yyyy");
            string timeNow = DateTime.Now.ToString("HH:mm");
            string path = System.IO.Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
          $"Act_{_registryId}.docx");


            using (var doc = DocX.Create(path))
            {
                doc.MarginTop = 76f;   
                doc.MarginRight = 57f;  
                doc.MarginBottom = 76f; 
                doc.MarginLeft = 76f;

                var header = doc.InsertParagraph();
                header.Alignment = Alignment.right;
                header.FontSize(14).Font(new Xceed.Document.NET.Font("Times New Roman")).Bold();
                header.Append("УТВЕРЖДЕН\n")
                    .Append("приказом Департамента\n")
                    .Append("транспорта Московской области\n") 
                    .Append("№ 145-П от 12.03.2024 г.");

                doc.InsertParagraph();
                var title = doc.InsertParagraph();
                title.Alignment = Alignment.center;
                title.FontSize(14).Font(new Xceed.Document.NET.Font("Times New Roman")).Bold();
                title.Append("АКТ приема-передачи транспортного средства\n со специализированной стоянки");

                doc.InsertParagraph();
                var city = doc.AddTable(1, 2);
                city.Alignment = Alignment.left;
                city.Design = TableDesign.None;
                city.Rows[0].Cells[0].Paragraphs.First().Append("г. Серпухов");
                city.Rows[0].Cells[1].Paragraphs.First().Append($"от {today}").Alignment = Alignment.right;
                doc.InsertTable(city);
                doc.InsertParagraph();

                var mainTable = doc.AddTable(4, 2);
                mainTable.Design = TableDesign.TableGrid;
                mainTable.Alignment = Alignment.left;

                mainTable.SetColumnWidth(0, 120f);
                mainTable.SetColumnWidth(1, 330f);

                mainTable.Rows[0].Cells[0].Paragraphs.First().Append("Транспортное средство:").Bold();
                var vehicle = mainTable.Rows[0].Cells[1].Paragraphs.First();
                vehicle.Append("Марка/Модель: ").Bold().Append(_carInfo.MakeName).AppendLine();
                vehicle.Append("Госномер: ").Bold().Append(_carInfo.LicensePlate).AppendLine();
                vehicle.Append("VIN: ").Bold().Append(_carInfo.VIN);


                mainTable.Rows[1].Cells[0].Paragraphs.First().Append("Владелец:").Bold();
                mainTable.Rows[1].Cells[1].Paragraphs.First().Append(_carInfo.OwnerName);

                mainTable.Rows[2].Cells[0].Paragraphs.First().Append("Повреждения:").Bold();
                mainTable.Rows[2].Cells[1].Paragraphs.First().Append(damagesText);

                mainTable.Rows[3].Cells[0].Paragraphs.First().Append("Протокол задержания:").Bold();
                mainTable.Rows[3].Cells[1].Paragraphs.First().Append(protocolInfo);

                doc.InsertTable(mainTable);

                doc.InsertParagraph();
                var claims = doc.InsertParagraph();
                claims.Append("Претензии к хранению: ").Bold();
                claims.Append("Не имеются.");

                doc.InsertParagraph();
                doc.InsertParagraph();


                var dateTimeTable = doc.AddTable(1, 2);
                dateTimeTable.Design = TableDesign.None;
                dateTimeTable.Rows[0].Cells[0].Paragraphs.First()
                    .Append("Дата выдачи: ").Bold().Append(today);
                dateTimeTable.Rows[0].Cells[1].Paragraphs.First()
                    .Append("Время: ").Bold().Append(timeNow).Alignment = Alignment.right;
                doc.InsertTable(dateTimeTable);

                doc.InsertParagraph();
                doc.InsertParagraph();
                doc.InsertParagraph();
                var sign = doc.AddTable(1, 2);
                sign.Design = TableDesign.None;
                sign.Rows[0].Cells[0].Paragraphs.First().Append("Подпись владельца:\n\n\n").Bold()
                    .Append("_____________________ / ").Append(_carInfo.OwnerName);

                sign.Rows[0].Cells[1].Paragraphs.First().Append("Подпись диспетчера:\n\n\n").Bold()
                    .Append("_____________________ / ").Append(_carInfo.DispatcherName);

                doc.InsertTable(sign);

                doc.Save();
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Не удалось открыть файл: {ex.Message}");
            }

            MessageBox.Show($" Акт сформирован и открыт:\n{path}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPaymentMethod.SelectedValue == null)
            {
                MessageBox.Show("Выберите способ оплаты!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var res = MessageBox.Show(
                "Подтвердите выдачу автомобиля. Действие нельзя отменить.",
                "Подтверждение выдачи",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes)
                return;

            try
            {
                int userId = App.CurrentUser != null ? App.CurrentUser.UserId : 1;
                int methodId = Convert.ToInt32(cmbPaymentMethod.SelectedValue);
                string receiptNum = string.Format("REC-{0:yyyyMMdd}-{1}", DateTime.Now, _registryId);

                bool success = _repo.ReleaseCar(
                    _registryId,
                    _totalAmount,
                    methodId,
                    receiptNum,
                    chkGibddPermission.IsChecked == true,
                    chkIdentity.IsChecked == true,
                    chkDocuments.IsChecked == true,
                    userId);

                if (success)
                {
                    GenerateActWord();

                    MessageBox.Show(
                        "Автомобиль успешно выдан!\nКвитанция сохранена на рабочем столе.",
                        "Выдача завершена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    var main = Window.GetWindow(this) as MainWindow;
                    if (main != null)
                        main.MainFrame.Navigate(new DashboardPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка при выдаче: {0}", ex.Message),
                    "Ошибка БД",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var main = Window.GetWindow(this) as MainWindow;
            if (main != null)
                main.MainFrame.Navigate(new DashboardPage());
        }
    }
}