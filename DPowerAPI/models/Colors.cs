using System.ComponentModel.DataAnnotations.Schema;

namespace DPowerAPI.Models;

public class Colors
{
    public int ID { get; set; }
    public string? Code { get; set; }
    public string? Color_Name { get; set; }
    public string Status { get; set; } = "IsActive";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}
