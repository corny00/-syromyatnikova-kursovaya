using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class ChecklistDefinition
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }    
        public bool IsDefault { get; set; }     
    }
}
