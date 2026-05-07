using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GBDD.Models
{
    public class VehiclePhoto
    {
        public int PhotoId { get; set; }
        public int RegistryId { get; set; } 
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public BitmapImage ImageSource { get; set; }
    }
}
