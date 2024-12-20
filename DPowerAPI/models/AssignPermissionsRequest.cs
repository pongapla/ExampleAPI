namespace DPowerAPI.models
{
    public class AssignPermissionsRequest
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public  List<int>? Permissions { get; set; }
    }
}
