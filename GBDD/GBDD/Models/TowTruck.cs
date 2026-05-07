using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class TowTruck
    {
        public int TowTruckId { get; set; }
        public string PlateNumber { get; set; }  
        public string DriverName { get; set; }  
        public string CompanyName { get; set; } 
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }
}
