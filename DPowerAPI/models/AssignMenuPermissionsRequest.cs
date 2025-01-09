namespace DPowerAPI.Models
{
    public class AssignMenuPermissionsRequest
    {
        public int UserID { get; set; }
        public List<int>? MenuIDs { get; set; }
    }
}
