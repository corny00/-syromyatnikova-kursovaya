using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Data
{
    public class ReleaseCarRepository
    {
        private readonly string _connString;

        public ReleaseCarRepository()
        {
            _connString = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;
        }

        public ReleaseCarInfo GetCarInfo(int registryId)
        {
            string q = @"SELECT er.LicensePlate, er.VIN, cm.MakeName, cmo.ModelName, vo.FullName,
                                er.IntakeDate, er.ProtocolNumber, er.CategoryId,
                                u.FullName as DispatcherName
                         FROM EvacuationRegistry er
                         JOIN CarMakes cm ON er.MakeId = cm.MakeId
                         JOIN CarModels cmo ON er.ModelId = cmo.ModelId
                         LEFT JOIN VehicleOwners vo ON er.OwnerId = vo.OwnerId
                         JOIN VehicleCategories vc ON er.CategoryId = vc.CategoryId
                         LEFT JOIN Users u ON er.CreatedByUserId = u.UserId
                         WHERE er.RegistryId = @Id";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Id", registryId);
                conn.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return new ReleaseCarInfo
                        {
                            LicensePlate = r["LicensePlate"].ToString(),
                            VIN = r["VIN"] != DBNull.Value ? r["VIN"].ToString() : "",
                            MakeName = r["MakeName"].ToString(),
                            ModelName = r["ModelName"].ToString(),
                            OwnerName = r["FullName"] != DBNull.Value ? r["FullName"].ToString() : "Не указан",
                            IntakeDate = Convert.ToDateTime(r["IntakeDate"]),
                            ProtocolNumber = r["ProtocolNumber"].ToString(),
                            CategoryId = Convert.ToInt32(r["CategoryId"]),
                            DispatcherName = r["DispatcherName"] != DBNull.Value ? r["DispatcherName"].ToString() : "Диспетчер"
                        };
                    }
                }
            }
            return null;
        }

        public DataTable GetPaymentMethods()
        {
            string pmQ = "SELECT MethodId, MethodName FROM PaymentMethods WHERE IsActive = 1";
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(pmQ, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public TariffInfo GetActiveTariff(int categoryId)
        {
            string q = @"SELECT TOP 1 TowCost, HourlyRate, DailyCap FROM Tariffs
                         WHERE CategoryId = @Cat AND IsActive = 1 AND ValidFrom <= GETDATE() 
                         AND (ValidTo IS NULL OR ValidTo >= GETDATE()) ORDER BY ValidFrom DESC";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@Cat", categoryId);
                conn.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return new TariffInfo
                        {
                            TowCost = Convert.ToDecimal(r["TowCost"]),
                            HourlyRate = Convert.ToDecimal(r["HourlyRate"]),
                            DailyCap = r["DailyCap"] != DBNull.Value ? (decimal?)Convert.ToDecimal(r["DailyCap"]) : null
                        };
                    }
                }
            }
            return null;
        }

        public List<string> GetDamages(int registryId)
        {
            string dmgQ = @"SELECT dz.DisplayName + ' — ' + dt.DisplayName as Info
                    FROM Damages d
                    JOIN DamageZones dz ON d.ZoneId = dz.ZoneId
                    JOIN DamageTypes dt ON d.TypeId = dt.TypeId
                    WHERE d.RegistryId = @Id";

            List<string> damages = new List<string>();
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(dmgQ, conn))
            {
                cmd.Parameters.AddWithValue("@Id", registryId);
                conn.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        damages.Add(r["Info"].ToString());
                }
            }
            return damages;
        }

        public bool ReleaseCar(int registryId, decimal amount, int methodId, string receiptNum,
            bool gibddPermission, bool identityVerified, bool documentsChecked, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {

                        string updQ = "UPDATE EvacuationRegistry SET StatusId = 2, ReleaseDate = GETDATE() WHERE RegistryId = @Id";
                        using (SqlCommand updCmd = new SqlCommand(updQ, conn, tran))
                        {
                            updCmd.Parameters.AddWithValue("@Id", registryId);
                            updCmd.ExecuteNonQuery();
                        }

                        string verQ = @"INSERT INTO ReleaseVerifications 
                                        (RegistryId, GibddPermissionReceived, IdentityVerified, DocumentsChecked, VerifiedByUserId, VerifiedAt)
                                        VALUES (@Id, @Gibdd, @Ident, @Docs, @User, GETDATE())";
                        using (SqlCommand verCmd = new SqlCommand(verQ, conn, tran))
                        {
                            verCmd.Parameters.AddWithValue("@Id", registryId);
                            verCmd.Parameters.AddWithValue("@Gibdd", gibddPermission);
                            verCmd.Parameters.AddWithValue("@Ident", identityVerified);
                            verCmd.Parameters.AddWithValue("@Docs", documentsChecked);
                            verCmd.Parameters.AddWithValue("@User", userId);
                            verCmd.ExecuteNonQuery();
                        }

                        string payQ = @"INSERT INTO Payments (RegistryId, Amount, MethodId, PaymentDate, ReceiptNumber)
                                        VALUES (@Id, @Amount, @Method, GETDATE(), @Receipt)";
                        using (SqlCommand payCmd = new SqlCommand(payQ, conn, tran))
                        {
                            payCmd.Parameters.AddWithValue("@Id", registryId);
                            payCmd.Parameters.AddWithValue("@Amount", amount);
                            payCmd.Parameters.AddWithValue("@Method", methodId);
                            payCmd.Parameters.AddWithValue("@Receipt", receiptNum);
                            payCmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    public class ReleaseCarInfo
    {
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public string MakeName { get; set; }
        public string ModelName { get; set; }
        public string OwnerName { get; set; }
        public DateTime IntakeDate { get; set; }
        public string ProtocolNumber { get; set; }
        public int CategoryId { get; set; }
        public string DispatcherName { get; set; }

        public string VehicleInfoShort
        {
            get { return string.Format("{0} | {1} {2}", LicensePlate, MakeName, ModelName); }
        }

        public string VehicleInfoFull
        {
            get
            {
                return string.Format("<b>Марка/Модель:</b> {0} {1}<br><b>Госномер:</b> {2}<br><b>VIN:</b> {3}",
                    MakeName, ModelName, LicensePlate, VIN);
            }
        }
    }

    public class TariffInfo
    {
        public decimal TowCost { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal? DailyCap { get; set; }
    }
}
