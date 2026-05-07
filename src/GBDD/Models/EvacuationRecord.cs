using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class EvacuationRecord
    {
        public int RegistryId { get; set; } 
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public int MakeId { get; set; }
        public string MakeName { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string ProtocolNumber { get; set; }
        public int InspectorUserId { get; set; }
        public string InspectorName { get; set; }
        public string LegalArticle { get; set; }
        public int? TowTruckId { get; set; }
        public string TowTruckPlate { get; set; }
        public string TowDriverName { get; set; }
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }


        public DateTime IntakeDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int StatusId { get; set; }             
        public string StatusName { get; set; }        

        public int? CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int DaysOnLot => ReleaseDate.HasValue
            ? (ReleaseDate.Value - IntakeDate).Days
            : (DateTime.Now - IntakeDate).Days;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsOnLot => StatusName == "На участке";
    }
}
