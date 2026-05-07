using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class Tariff
    {
        public int TariffId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public decimal TowCost { get; set; }     
        public decimal HourlyRate { get; set; } 
        public decimal? DailyCap { get; set; } 
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }
}
