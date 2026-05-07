using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class ImpoundChecklist
    {
        public int ChecklistId { get; set; }
        public int RegistryId { get; set; } 
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public bool IsPresent { get; set; }
    }

}
