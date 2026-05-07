using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class CarModel
    {
        public int ModelId { get; set; }
        public int MakeId { get; set; }
        public string ModelName { get; set; } 
        public string MakeName { get; set; }  
        public bool IsActive { get; set; }
    }
}
