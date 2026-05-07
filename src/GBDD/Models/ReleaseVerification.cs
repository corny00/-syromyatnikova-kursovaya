using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class ReleaseVerification
    {
        public int VerificationId { get; set; }
        public int RegistryId { get; set; }  
        public bool GibddPermissionReceived { get; set; }
        public bool IdentityVerified { get; set; }
        public bool DocumentsChecked { get; set; }
        public int? VerifiedByUserId { get; set; }
        public string VerifiedByUserName { get; set; }
        public DateTime VerifiedAt { get; set; }
    }

}
