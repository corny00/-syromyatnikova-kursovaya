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
    /// Логика взаимодействия для ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
            private readonly ReportsRepository _repo;

            public ReportsPage()
            {
                InitializeComponent();
                _repo = new ReportsRepository();

                dpFrom.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dpTo.SelectedDate = DateTime.Now;

                GenerateReports();
            }

            private void GenerateReports_Click(object sender, RoutedEventArgs e)
            {
                GenerateReports();
            }

            private void ResetFilters_Click(object sender, RoutedEventArgs e)
            {
                dpFrom.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dpTo.SelectedDate = DateTime.Now;
                GenerateReports();
            }

            private void GenerateReports()
            {
                DateTime from = dpFrom.SelectedDate ?? DateTime.Now.AddMonths(-1);
                DateTime to = dpTo.SelectedDate ?? DateTime.Now;
                to = to.Date.AddDays(1).AddTicks(-1);

                LoadSummaryStats(from, to);
                LoadFinanceReport(from, to);
                LoadInspectorsReport(from, to);
                LoadMakesReport(from, to);
                LoadDetailsGrid(from, to);
            }

            private void LoadSummaryStats(DateTime from, DateTime to)
            {
                SummaryStats stats = _repo.GetSummaryStats(from, to);

                lblTotalRevenue.Text = stats.TotalRevenue.ToString("C");
                lblCarsIn.Text = stats.CarsIn.ToString();
                lblCarsOut.Text = stats.CarsOut.ToString();
                lblAvgDays.Text = string.Format("{0} дн.", stats.AvgDays);
            }

            private void LoadFinanceReport(DateTime from, DateTime to)
            {
                DataTable dt = _repo.GetFinanceReport(from, to);
                dgFinance.ItemsSource = dt.DefaultView;
            }

            private void LoadInspectorsReport(DateTime from, DateTime to)
            {
                DataTable dt = _repo.GetInspectorsReport(from, to);
                dgInspectors.ItemsSource = dt.DefaultView;
            }

            private void LoadMakesReport(DateTime from, DateTime to)
            {
                DataTable dt = _repo.GetMakesReport(from, to);
                dgMakes.ItemsSource = dt.DefaultView;
            }

            private void LoadDetailsGrid(DateTime from, DateTime to)
            {
                DataTable dt = _repo.GetDetailsReport(from, to);
                dgDetails.ItemsSource = dt.DefaultView;
            }

            private void Export_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    DateTime from = dpFrom.SelectedDate ?? DateTime.Now.AddMonths(-1);
                    DateTime to = dpTo.SelectedDate ?? DateTime.Now;
                    to = to.Date.AddDays(1).AddTicks(-1);

                    ReportExportData data = _repo.GetExportData(from, to);

                    string path = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        string.Format("Report_{0:yyyyMMdd_HHmm}.csv", DateTime.Now));

                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                    {

                        writer.WriteLine("Отчет по эвакуации ТС");
                        writer.WriteLine(string.Format("Период: с {0:dd.MM.yyyy} по {1:dd.MM.yyyy}", data.FromDate, data.ToDate));
                        writer.WriteLine(string.Format("Дата формирования: {0:dd.MM.yyyy HH:mm}", data.GeneratedAt));
                        writer.WriteLine("");
                        writer.WriteLine("");

                        writer.WriteLine("Сводная статистика");
                        writer.WriteLine(string.Format("Общая выручка: {0:C}", data.Summary.TotalRevenue));
                        writer.WriteLine(string.Format("Принято автомобилей: {0}", data.Summary.CarsIn));
                        writer.WriteLine(string.Format("Выдано автомобилей: {0}", data.Summary.CarsOut));
                        writer.WriteLine(string.Format("Среднее время хранения: {0} дн.", data.Summary.AvgDays));
                        writer.WriteLine("");
                        writer.WriteLine("");

                        writer.WriteLine("Финансы");
                        writer.WriteLine("Способ оплаты;Количество;Сумма;Процент");
                        foreach (DataRowView row in data.FinanceByMethod.DefaultView)
                        {
                            writer.WriteLine(string.Format("{0};{1};{2:C};{3}%",
                                row["MethodName"],
                                row["Count"],
                                Convert.ToDecimal(row["Amount"]),
                                row["Percent"]));
                        }
                        writer.WriteLine("");
                        writer.WriteLine("");

                        writer.WriteLine("Топ инспекторов ");
                        writer.WriteLine("Инспектор;Количество оформлений");
                        foreach (DataRowView row in data.TopInspectors.DefaultView)
                        {
                            writer.WriteLine(string.Format("{0};{1}",
                                row["FullName"],
                                row["Count"]));
                        }
                        writer.WriteLine("");
                        writer.WriteLine("");

                        writer.WriteLine("Топ марок ТС");
                        writer.WriteLine("Марка;Количество");
                        foreach (DataRowView row in data.TopMakes.DefaultView)
                        {
                            writer.WriteLine(string.Format("{0};{1}",
                                row["MakeName"],
                                row["Count"]));
                        }
                        writer.WriteLine("");
                        writer.WriteLine("");

                        writer.WriteLine("Спичок автомобилей");
                        writer.WriteLine("Дата;Госномер;Марка/Модель;Владелец;Протокол;Дней на стоянке;Сумма;Статус");
                        foreach (DataRowView row in data.Details.DefaultView)
                        {
                            writer.WriteLine(string.Format("{0:dd.MM.yyyy};{1};{2};{3};{4};{5};{6:C};{7}",
                                Convert.ToDateTime(row["IntakeDate"]),
                                row["LicensePlate"],
                                row["MakeModel"],
                                row["OwnerName"] != DBNull.Value ? row["OwnerName"].ToString() : "Не указан",
                                row["ProtocolNumber"],
                                row["DaysOnLot"],
                                Convert.ToDecimal(row["Amount"]),
                                row["StatusName"]));
                        }
                        writer.WriteLine("");
                        writer.WriteLine("");
                    }

                    MessageBox.Show(
                        string.Format("Отчёт сохранён:\n{0}\n",path,data.Details.Rows.Count),
                        "Экспорт завершён",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("Ошибка экспорта: {0}", ex.Message),
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
