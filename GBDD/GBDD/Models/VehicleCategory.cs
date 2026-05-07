using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class VehicleCategory
    {
        public int CategoryId { get; set; }
        public string Code { get; set; }  
        public string Name { get; set; }    
        public bool IsActive { get; set; }
    }
}
