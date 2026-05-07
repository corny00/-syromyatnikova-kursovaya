using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class CarColor
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; } 
        public string HexCode { get; set; }   
        public bool IsActive { get; set; }
    }
}
