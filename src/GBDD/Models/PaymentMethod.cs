using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class PaymentMethod
    {
        public int MethodId { get; set; }
        public string MethodName { get; set; } 
        public bool IsActive { get; set; }
    }
}
