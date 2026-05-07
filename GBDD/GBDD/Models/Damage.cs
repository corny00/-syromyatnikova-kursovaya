using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class Damage
    {
        public int DamageId { get; set; }
        public int RegistryId { get; set; } 
        public int ZoneId { get; set; }
        public string ZoneDisplayName { get; set; }
        public int TypeId { get; set; }
        public string TypeDisplayName { get; set; }
        public string Description { get; set; }
        public bool IsPreExisting { get; set; }
    }
}
