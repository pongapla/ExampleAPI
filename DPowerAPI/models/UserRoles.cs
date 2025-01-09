using System.Data;

namespace DPowerAPI.Models;

public class UserRoles
{

       
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public virtual User? User { get; set; }
        public virtual Roles? Role { get; set; }

}
