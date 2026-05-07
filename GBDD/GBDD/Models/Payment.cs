using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int RegistryId { get; set; }  
        public decimal Amount { get; set; }
        public int MethodId { get; set; }
        public string MethodName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReceiptNumber { get; set; }
    }
}
