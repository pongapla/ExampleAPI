namespace DPowerAPI.models
{
    public class BalanceInventory
    {
        public int Id { get; set; } 
        public string? Code { get; set; }
        public string? Code_Old { get; set; }
        public string? Code_Barcode { get; set; }
        public string? Code_Group { get; set; }
        public string? Name_Group { get; set; }
        public string? Detail_Product { get; set; }
        public decimal? Wait_Location { get; set; } 
        public decimal? Book_CV { get; set; }   
        public string? Book_Product { get; set; }
        public decimal? Book_MT { get; set; }   
        //public string? Book_Product { get; set; }
        public decimal? Available_For_Sale { get; set; }
        public decimal? _OEM { get; set; }
        public decimal? QTY_Product { get; set; }
        public DateTime? Date_Created { get; set; } 

    }
}
