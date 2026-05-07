using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class VehicleOwner
    {
        public int OwnerId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string PassportSeriesNumber { get; set; }
        public string DriverLicenseNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
