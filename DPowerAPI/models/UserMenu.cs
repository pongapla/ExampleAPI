namespace DPowerAPI.models
{
    public class UserMenu
    {
        public int UserId { get; set; }
        public int MenuId { get; set; }

        public virtual User? User { get; set; }
        public virtual Menu? Menu { get; set; }
    }
}
