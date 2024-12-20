namespace DPowerAPI.models
{
    public class AssignMenuPermissionsRequest
    {
        public int UserID { get; set; }
        public List<int>? MenuIDs { get; set; }
    }
}
