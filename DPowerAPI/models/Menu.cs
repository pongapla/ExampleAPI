namespace DPowerAPI.models
{
    public class Menu
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }


        public string Status { get; set; } = "IsActive";


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
