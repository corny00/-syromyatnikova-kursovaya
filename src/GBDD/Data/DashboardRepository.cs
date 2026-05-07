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
     public class DashboardRepository
    {
        private readonly string _connString;
        public DashboardRepository()
        {

            _connString = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;
        }


        public DataTable GetOccupancyStats()
        {
            string query = @"
                SELECT 
                    COUNT(CASE WHEN StatusId = 1 THEN 1 END) AS Occupied,
                    100 AS TotalCapacity
                FROM EvacuationRegistry";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }


        public DataTable GetDailyStats()
        {
            string query = @"
                SELECT 
                    SUM(CASE WHEN CAST(IntakeDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS ArrivedToday,
                    SUM(CASE WHEN CAST(ReleaseDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS ReleasedToday
                FROM EvacuationRegistry";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }


        public int GetDebtorsCount()
        {
            string query = @"
                SELECT COUNT(*) 
                FROM EvacuationRegistry 
                WHERE StatusId = 1 AND DATEDIFF(DAY, IntakeDate, GETDATE()) > 30";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }


        public DataTable GetDebtorsList()
        {
            string query = @"
                SELECT 
                    er.RegistryId,
                    er.LicensePlate,
                    cm.MakeName + ' ' + cmo.ModelName AS MakeModel,
                    vo.FullName AS OwnerName,
                    er.IntakeDate,
                    DATEDIFF(DAY, er.IntakeDate, GETDATE()) AS DaysOnLot,
                    s.StatusName
                FROM EvacuationRegistry er
                INNER JOIN CarMakes cm ON er.MakeId = cm.MakeId
                INNER JOIN CarModels cmo ON er.ModelId = cmo.ModelId
                LEFT JOIN VehicleOwners vo ON er.OwnerId = vo.OwnerId
                INNER JOIN Statuses s ON er.StatusId = s.StatusId
                WHERE s.StatusName = N'На участке' 
                  AND DATEDIFF(DAY, er.IntakeDate, GETDATE()) > 30
                ORDER BY DaysOnLot DESC";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public bool ArchiveCar(int registryId)
        {
            string query = "UPDATE EvacuationRegistry SET StatusId = 3 WHERE RegistryId = @Id AND StatusId = 1";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", registryId);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public DataTable GetAllCars(string filter, string search)
        {
            string query = @"
                SELECT 
                    er.RegistryId,
                    er.LicensePlate,
                    cm.MakeName + ' ' + cmo.ModelName AS MakeModel,
                    vo.FullName AS OwnerName,
                    er.IntakeDate,
                    DATEDIFF(DAY, er.IntakeDate, GETDATE()) AS DaysOnLot,
                    s.StatusName
                FROM EvacuationRegistry er
                INNER JOIN CarMakes cm ON er.MakeId = cm.MakeId
                INNER JOIN CarModels cmo ON er.ModelId = cmo.ModelId
                LEFT JOIN VehicleOwners vo ON er.OwnerId = vo.OwnerId
                INNER JOIN Statuses s ON er.StatusId = s.StatusId
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (filter == "OnLot")
            {
                query += " AND s.StatusName = N'На участке'";
            }
            else if (filter == "Archived")
            {
                query += " AND s.StatusName = N'Архив'";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (er.LicensePlate LIKE @Search OR vo.FullName LIKE @Search OR cm.MakeName LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", $"%{search}%"));
            }

            query += " ORDER BY er.IntakeDate DESC";

            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn))
            {
                if (parameters.Count > 0) cmd.Parameters.AddRange(parameters.ToArray());
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }

        }
    }
}

