using GBDD.Data;
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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private readonly string _connString;

        public AdminPage()
        {
            InitializeComponent();
            _connString = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;

            LoadUsers();
            LoadTariffs();
            LoadRoles();
            LoadCategories();
        }
        private void LoadUsers()
        {
            string q = "SELECT u.UserId, u.Username, u.FullName, u.RoleId, r.RoleName, u.IsActive FROM Users u JOIN Roles r ON u.RoleId = r.RoleId";
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlDataAdapter da = new SqlDataAdapter(q, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgUsers.ItemsSource = dt.DefaultView;
            }
        }

        private void LoadRoles()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT RoleId, RoleName FROM Roles", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmbRole.ItemsSource = dt.DefaultView;
                cmbRole.SelectedValuePath = "RoleId";
                cmbRole.DisplayMemberPath = "RoleName";
            }
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is DataRowView row)
            {
                txtLogin.Text = row["Username"].ToString();
                txtFullName.Text = row["FullName"].ToString();
                cmbRole.SelectedValue = row["RoleId"];
                txtPassword.Password = "";
            }
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null && string.IsNullOrEmpty(txtLogin.Text))
            {
                MessageBox.Show("Выберите сотрудника или введите нового", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    conn.Open();
                    bool isNew = dgUsers.SelectedItem == null;
                    int userId = isNew ? 0 : Convert.ToInt32(((DataRowView)dgUsers.SelectedItem)["UserId"]);

                    if (isNew && string.IsNullOrEmpty(txtPassword.Password))
                    {
                        MessageBox.Show("Для нового сотрудника укажите пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string hash = string.IsNullOrEmpty(txtPassword.Password)
                        ? ""
                        : PasswordHasher.ComputeSha256Hash(txtPassword.Password);

                    string sql = isNew
                        ? "INSERT INTO Users (Username, PasswordHash, FullName, RoleId, IsActive) VALUES (@Login, @Hash, @Name, @Role, 1)"
                        : "UPDATE Users SET Username=@Login, FullName=@Name, RoleId=@Role, IsActive=1 WHERE UserId=@Id";

                    if (!isNew && !string.IsNullOrEmpty(txtPassword.Password))
                        sql = "UPDATE Users SET Username=@Login, PasswordHash=@Hash, FullName=@Name, RoleId=@Role, IsActive=1 WHERE UserId=@Id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", txtLogin.Text.Trim());
                        cmd.Parameters.AddWithValue("@Hash", hash);
                        cmd.Parameters.AddWithValue("@Name", txtFullName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Role", cmbRole.SelectedValue);
                        if (!isNew) cmd.Parameters.AddWithValue("@Id", userId);

                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Сотрудник сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null) return;

            var res = MessageBox.Show("Заблокировать сотрудника? (Это мягкое удаление)", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                int id = Convert.ToInt32(((DataRowView)dgUsers.SelectedItem)["UserId"]);
                using (SqlConnection conn = new SqlConnection(_connString))
                using (SqlCommand cmd = new SqlCommand("UPDATE Users SET IsActive=0 WHERE UserId=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                LoadUsers();
            }
        }
        private void LoadTariffs()
        {
            string q = @"SELECT t.TariffId, t.CategoryId, vc.Code as CategoryCode, vc.Name as CategoryName, 
                         t.TowCost, t.HourlyRate, t.DailyCap
                 FROM Tariffs t
                 JOIN VehicleCategories vc ON t.CategoryId = vc.CategoryId
                 WHERE t.IsActive = 1";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlDataAdapter da = new SqlDataAdapter(q, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgTariffs.ItemsSource = dt.DefaultView;
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlDataAdapter da = new SqlDataAdapter("SELECT CategoryId, Name FROM VehicleCategories", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmbTariffCat.ItemsSource = dt.DefaultView;
            }
        }

        private void dgTariffs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTariffs.SelectedItem is DataRowView row)
            {
                cmbTariffCat.SelectedValue = row["CategoryName"].ToString();
                txtTowCost.Text = row["TowCost"].ToString();
                txtHourlyRate.Text = row["HourlyRate"].ToString();
                txtDailyCap.Text = row["DailyCap"].ToString();
            }
        }

        private void SaveTariff_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTariffCat.SelectedValue == null) return;

            string catName = ((DataRowView)cmbTariffCat.SelectedItem)["Name"].ToString();
            int catId = Convert.ToInt32(((DataRowView)cmbTariffCat.SelectedItem)["CategoryId"]);

            decimal tow = decimal.Parse(txtTowCost.Text.Replace("₽", "").Trim());
            decimal rate = decimal.Parse(txtHourlyRate.Text.Replace("₽", "").Trim());
            decimal cap = string.IsNullOrEmpty(txtDailyCap.Text) ? 0 : decimal.Parse(txtDailyCap.Text.Replace("₽", "").Trim());

            try
            {
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    conn.Open();
                    using (SqlCommand checkCmd = new SqlCommand("SELECT TariffId FROM Tariffs WHERE CategoryId=@Cat AND IsActive=1", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Cat", catId);
                        object res = checkCmd.ExecuteScalar();

                        string sql = (res != null)
                            ? "UPDATE Tariffs SET TowCost=@Tow, HourlyRate=@Rate, DailyCap=@Cap WHERE TariffId=@Id"
                            : "INSERT INTO Tariffs (CategoryId, TowCost, HourlyRate, DailyCap, ValidFrom) VALUES (@Cat, @Tow, @Rate, @Cap, GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Tow", tow);
                            cmd.Parameters.AddWithValue("@Rate", rate);
                            cmd.Parameters.AddWithValue("@Cap", cap);
                            cmd.Parameters.AddWithValue("@Cat", catId);
                            if (res != null) cmd.Parameters.AddWithValue("@Id", res);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show("Тариф обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTariffs();
            }
            catch
            {
                MessageBox.Show("Ошибка сохранения тарифа. Проверьте числа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            
            txtLogin.Text = "";
            txtFullName.Text = "";
            cmbRole.SelectedValue = null;
            txtPassword.Password = "";
            dgUsers.SelectedItem = null;
            txtLogin.Focus();
        }
    }
}
