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
    public class ReportsRepository
    {
        private readonly string _connString;

        public ReportsRepository()
        {
            _connString = ConfigurationManager.ConnectionStrings["GibddConnection"].ConnectionString;
        }

        public SummaryStats GetSummaryStats(DateTime from, DateTime to)
        {
            string q = @"
                SELECT 
                    ISNULL(SUM(p.Amount), 0) as TotalRevenue,
                    COUNT(DISTINCT CASE WHEN er.IntakeDate BETWEEN @From AND @To THEN er.RegistryId END) as CarsIn,
                    COUNT(DISTINCT CASE WHEN er.ReleaseDate BETWEEN @From AND @To THEN er.RegistryId END) as CarsOut,
                    ISNULL(AVG(DATEDIFF(DAY, er.IntakeDate, ISNULL(er.ReleaseDate, GETDATE()))), 0) as AvgDays
                FROM EvacuationRegistry er
                LEFT JOIN Payments p ON er.RegistryId = p.RegistryId
                WHERE er.IntakeDate <= @To AND (er.ReleaseDate IS NULL OR er.ReleaseDate >= @From)";

            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("@From", from);
                cmd.Parameters.AddWithValue("@To", to);
                conn.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return new SummaryStats
                        {
                            TotalRevenue = Convert.ToDecimal(r["TotalRevenue"]),
                            CarsIn = Convert.ToInt32(r["CarsIn"]),
                            CarsOut = Convert.ToInt32(r["CarsOut"]),
                            AvgDays = Convert.ToInt32(r["AvgDays"])
                        };
                    }
                }
            }
            return new SummaryStats();
        }

        public DataTable GetFinanceReport(DateTime from, DateTime to)
        {
            string q = @"
        DECLARE @TotalAmount DECIMAL(18,2);
        
        SELECT @TotalAmount = ISNULL(SUM(p.Amount), 0)
        FROM Payments p
        JOIN EvacuationRegistry er ON p.RegistryId = er.RegistryId
        WHERE p.PaymentDate BETWEEN @From AND @To;

        SELECT 
            pm.MethodName,
            COUNT(p.PaymentId) as Count,
            SUM(p.Amount) as Amount,
            CASE 
                WHEN @TotalAmount = 0 THEN 0 
                ELSE CAST((SUM(p.Amount) * 100.0 / @TotalAmount) AS INT)
            END as [Percent]
        FROM Payments p
        JOIN PaymentMethods pm ON p.MethodId = pm.MethodId
        JOIN EvacuationRegistry er ON p.RegistryId = er.RegistryId
        WHERE p.PaymentDate BETWEEN @From AND @To
        GROUP BY pm.MethodName, pm.MethodId
        ORDER BY Amount DESC";

            return ExecuteQuery(q, from, to);
        }

        public DataTable GetInspectorsReport(DateTime from, DateTime to)
        {
            string q = @"
                SELECT TOP 5 u.FullName, COUNT(er.RegistryId) as Count
                FROM EvacuationRegistry er
                JOIN Users u ON er.InspectorUserId = u.UserId
                WHERE er.IntakeDate BETWEEN @From AND @To
                GROUP BY u.FullName, u.UserId
                ORDER BY Count DESC";

            return ExecuteQuery(q, from, to);
        }

        public DataTable GetMakesReport(DateTime from, DateTime to)
        {
            string q = @"
                SELECT TOP 5 cm.MakeName, COUNT(er.RegistryId) as Count
                FROM EvacuationRegistry er
                JOIN CarMakes cm ON er.MakeId = cm.MakeId
                WHERE er.IntakeDate BETWEEN @From AND @To
                GROUP BY cm.MakeName
                ORDER BY Count DESC";

            return ExecuteQuery(q, from, to);
        }

        public DataTable GetDetailsReport(DateTime from, DateTime to)
        {
            string q = @"
                SELECT 
                    er.IntakeDate,
                    er.LicensePlate,
                    cm.MakeName + ' ' + cmo.ModelName as MakeModel,
                    vo.FullName as OwnerName,
                    ISNULL(p.Amount, 0) as Amount,
                    s.StatusName,
                    er.ProtocolNumber,
                    DATEDIFF(DAY, er.IntakeDate, ISNULL(er.ReleaseDate, GETDATE())) as DaysOnLot
                FROM EvacuationRegistry er
                JOIN CarMakes cm ON er.MakeId = cm.MakeId
                JOIN CarModels cmo ON er.ModelId = cmo.ModelId
                LEFT JOIN VehicleOwners vo ON er.OwnerId = vo.OwnerId
                LEFT JOIN Payments p ON er.RegistryId = p.RegistryId
                JOIN Statuses s ON er.StatusId = s.StatusId
                WHERE er.IntakeDate BETWEEN @From AND @To
                ORDER BY er.IntakeDate DESC";

            return ExecuteQuery(q, from, to);
        }

        public ReportExportData GetExportData(DateTime from, DateTime to)
        {
            var data = new ReportExportData
            {
                FromDate = from,
                ToDate = to,
                GeneratedAt = DateTime.Now,
                Summary = GetSummaryStats(from, to),
                Details = GetDetailsReport(from, to),
                FinanceByMethod = GetFinanceReport(from, to),
                TopInspectors = GetInspectorsReport(from, to),
                TopMakes = GetMakesReport(from, to)
            };

            return data;
        }

        private DataTable ExecuteQuery(string query, DateTime from, DateTime to)
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@From", from);
                cmd.Parameters.AddWithValue("@To", to);
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }

    public class SummaryStats
    {
        public decimal TotalRevenue { get; set; }
        public int CarsIn { get; set; }
        public int CarsOut { get; set; }
        public int AvgDays { get; set; }
    }

    public class ReportExportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public SummaryStats Summary { get; set; }
        public DataTable Details { get; set; }
        public DataTable FinanceByMethod { get; set; }
        public DataTable TopInspectors { get; set; }
        public DataTable TopMakes { get; set; }
    }
}
