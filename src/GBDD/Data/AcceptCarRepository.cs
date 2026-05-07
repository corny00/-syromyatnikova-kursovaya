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
    public class AcceptCarRepository
    {
        private readonly string _connString;

        public AcceptCarRepository()
        {
            _connString = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;
        }

        public DataTable GetVehicleCategories()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT CategoryId, Code, Name FROM VehicleCategories WHERE IsActive=1 ORDER BY Name", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetCarMakes()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT MakeId, MakeName FROM CarMakes WHERE IsActive=1 ORDER BY MakeName", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetCarModelsByMake(int makeId)
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT ModelId, ModelName FROM CarModels WHERE MakeId=@MakeId AND IsActive=1 ORDER BY ModelName", conn))
            {
                cmd.Parameters.AddWithValue("@MakeId", makeId);
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public DataTable GetCarColors()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT ColorId, ColorName FROM CarColors WHERE IsActive=1 ORDER BY ColorName", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetInspectors()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT UserId, FullName FROM Users WHERE RoleId=3 AND IsActive=1 ORDER BY FullName", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetTowTrucks()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT TowTruckId, PlateNumber, DriverName FROM TowTrucks WHERE IsActive=1 ORDER BY PlateNumber", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetVehicleOwners()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT OwnerId, FullName FROM VehicleOwners ORDER BY FullName", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetDamageZones()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT ZoneId, DisplayName, ZoneCode FROM DamageZones ORDER BY ZoneId", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public DataTable GetChecklistItems()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand("SELECT ItemId, ItemName FROM ChecklistDefinitions ORDER BY ItemId", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public EvacuationRecord GetCarRecord(int registryId)
        {
            string query = @"SELECT er.*, cm.MakeId, cmo.ModelId, cc.ColorId 
                             FROM EvacuationRegistry er
                             INNER JOIN CarMakes cm ON er.MakeId = cm.MakeId
                             INNER JOIN CarModels cmo ON er.ModelId = cmo.ModelId
                             INNER JOIN CarColors cc ON er.ColorId = cc.ColorId
                             WHERE er.RegistryId = @Id";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", registryId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    return new EvacuationRecord
                    {
                        RegistryId = registryId,
                        LicensePlate = reader["LicensePlate"].ToString(),
                        VIN = reader["VIN"] != DBNull.Value ? reader["VIN"].ToString() : null,
                        MakeId = Convert.ToInt32(reader["MakeId"]),
                        ModelId = Convert.ToInt32(reader["ModelId"]),
                        ColorId = reader["ColorId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ColorId"]) : null,
                        CategoryId = reader["CategoryId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["CategoryId"]) : null,
                        ProtocolNumber = reader["ProtocolNumber"].ToString(),
                        InspectorUserId = reader["InspectorUserId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["InspectorUserId"]) : null,
                        LegalArticle = reader["LegalArticle"].ToString(),
                        TowTruckId = reader["TowTruckId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["TowTruckId"]) : null,
                        OwnerId = reader["OwnerId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["OwnerId"]) : null,
                        IntakeDate = Convert.ToDateTime(reader["IntakeDate"])
                    };
                }
            }
        }

        public List<int> GetSelectedDamageZones(int registryId)
        {
            var zones = new List<int>();
            var query = "SELECT ZoneId FROM Damages WHERE RegistryId = @RegistryId";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RegistryId", registryId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        zones.Add(Convert.ToInt32(reader["ZoneId"]));
                }
            }

            return zones;
        }

        public Dictionary<int, bool> GetChecklistValues(int registryId)
        {
            var checklist = new Dictionary<int, bool>();
            var query = "SELECT ItemId, IsPresent FROM ImpoundChecklist WHERE RegistryId = @RegistryId";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RegistryId", registryId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        checklist[Convert.ToInt32(reader["ItemId"])] = Convert.ToBoolean(reader["IsPresent"]);
                }
            }

            return checklist;
        }

        public int SaveCarRecord(CarRecordSaveModel model, List<int> selectedZones, Dictionary<int, bool> checklistValues)
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int registryId = model.RegistryId;

                        if (registryId == 0)
                        {
                            string insQ = @"INSERT INTO EvacuationRegistry 
                                (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, 
                                 InspectorUserId, LegalArticle, TowTruckId, OwnerId, StatusId, 
                                 IntakeDate, CreatedByUserId, CreatedAt, UpdatedAt)
                                VALUES (@Plate, @Vin, @Make, @Model, @Color, @Cat, @Prot, @Insp, @Art, 
                                        @Tow, @Owner, @Status, GETDATE(), @User, GETDATE(), GETDATE());
                                SELECT SCOPE_IDENTITY();";

                            using (SqlCommand insCmd = new SqlCommand(insQ, conn, tran))
                            {
                                AddCarRecordParameters(insCmd, model);
                                registryId = Convert.ToInt32(insCmd.ExecuteScalar());
                            }
                        }
                        else
                        {
                            string updQ = @"UPDATE EvacuationRegistry SET 
                                LicensePlate=@Plate, VIN=@Vin, MakeId=@Make, ModelId=@Model, ColorId=@Color, CategoryId=@Cat,
                                ProtocolNumber=@Prot, InspectorUserId=@Insp, LegalArticle=@Art, TowTruckId=@Tow, 
                                OwnerId=@Owner, UpdatedAt=GETDATE()
                                WHERE RegistryId=@Id";

                            using (SqlCommand updCmd = new SqlCommand(updQ, conn, tran))
                            {
                                AddCarRecordParameters(updCmd, model);
                                updCmd.Parameters.AddWithValue("@Id", registryId);
                                updCmd.ExecuteNonQuery();
                            }

                            using (SqlCommand delDam = new SqlCommand("DELETE FROM Damages WHERE RegistryId=@Id", conn, tran))
                            {
                                delDam.Parameters.AddWithValue("@Id", registryId);
                                delDam.ExecuteNonQuery();
                            }

                            using (SqlCommand delCheck = new SqlCommand("DELETE FROM ImpoundChecklist WHERE RegistryId=@Id", conn, tran))
                            {
                                delCheck.Parameters.AddWithValue("@Id", registryId);
                                delCheck.ExecuteNonQuery();
                            }
                        }

                        foreach (var zoneId in selectedZones)
                        {
                            using (SqlCommand dCmd = new SqlCommand(
                                "INSERT INTO Damages (RegistryId, ZoneId, TypeId, IsPreExisting) VALUES (@Reg, @Zone, @Type, 1)",
                                conn, tran))
                            {
                                dCmd.Parameters.AddWithValue("@Reg", registryId);
                                dCmd.Parameters.AddWithValue("@Zone", zoneId);
                                dCmd.Parameters.AddWithValue("@Type", 1);
                                dCmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var item in checklistValues)
                        {
                            using (SqlCommand cCmd = new SqlCommand(
                                "INSERT INTO ImpoundChecklist (RegistryId, ItemId, IsPresent) VALUES (@Reg, @Item, @Present)",
                                conn, tran))
                            {
                                cCmd.Parameters.AddWithValue("@Reg", registryId);
                                cCmd.Parameters.AddWithValue("@Item", item.Key);
                                cCmd.Parameters.AddWithValue("@Present", item.Value);
                                cCmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        return registryId;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private void AddCarRecordParameters(SqlCommand cmd, CarRecordSaveModel model)
        {
            cmd.Parameters.AddWithValue("@Plate", string.IsNullOrWhiteSpace(model.LicensePlate) ? (object)DBNull.Value : model.LicensePlate.Trim());
            cmd.Parameters.AddWithValue("@Vin", string.IsNullOrWhiteSpace(model.VIN) ? (object)DBNull.Value : model.VIN.Trim());
            cmd.Parameters.AddWithValue("@Make", model.MakeId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Model", model.ModelId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Color", model.ColorId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Cat", model.CategoryId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Prot", string.IsNullOrWhiteSpace(model.ProtocolNumber) ? (object)DBNull.Value : model.ProtocolNumber.Trim());
            cmd.Parameters.AddWithValue("@Insp", model.InspectorUserId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Art", string.IsNullOrWhiteSpace(model.LegalArticle) ? (object)DBNull.Value : model.LegalArticle.Trim());
            cmd.Parameters.AddWithValue("@Tow", model.TowTruckId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Owner", model.OwnerId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", model.StatusId ?? 1);
            cmd.Parameters.AddWithValue("@User", model.CreatedByUserId ?? 1);
        }
    }

    public class CarRecordSaveModel
    {
        public int RegistryId { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }
        public int? ColorId { get; set; }
        public int? CategoryId { get; set; }
        public string ProtocolNumber { get; set; }
        public int? InspectorUserId { get; set; }
        public string LegalArticle { get; set; }
        public int? TowTruckId { get; set; }
        public int? OwnerId { get; set; }
        public int? StatusId { get; set; }
        public int? CreatedByUserId { get; set; }
    }

    public class EvacuationRecord
    {
        public int RegistryId { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public int MakeId { get; set; }
        public int ModelId { get; set; }
        public int? ColorId { get; set; }
        public int? CategoryId { get; set; }
        public string ProtocolNumber { get; set; }
        public int? InspectorUserId { get; set; }
        public string LegalArticle { get; set; }
        public int? TowTruckId { get; set; }
        public int? OwnerId { get; set; }
        public DateTime IntakeDate { get; set; }
    }
}