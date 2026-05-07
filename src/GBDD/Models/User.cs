using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBDD.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } 
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } 
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
