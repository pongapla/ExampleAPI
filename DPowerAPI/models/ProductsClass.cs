namespace DPowerAPI.Models;

public class Products
{
    public int? ID { get; set; }
    public int? Product_ID { get; set; }
    public decimal? Cost_Price { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public int? Color_ID { get; set; } 
    public Colors? Color { get; set; }
}

public class ProductDTO : Products
{
    public string? Product_Code { get; set; }
    public string? Product_Name { get; set; }

    public string? Color_Name { get; set; } 
}

public class ProductCreateDTO
{
    public string? Product_Code { get; set; }
    public decimal Cost_Price { get; set; }
    public int? Color_ID { get; set; }
}
