
namespace DPowerAPI.models
{
    public class User
    {
        public int ID { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; } = "IsActive";
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
    public class LoginModel
    {
        public string? userName { get; set; }
        public string? password { get; set; }
    }
}
