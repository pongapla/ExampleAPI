namespace DPowerAPI.models;

public class RolePermissions
{
    
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public virtual Roles? Role { get; set; }
    public virtual Permissions? Permission { get; set; }
}
