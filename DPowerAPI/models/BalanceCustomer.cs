namespace DPowerAPI.models
{
    public class BalanceCustomer
    {
        public int ID { get; set; }
        public string? codeCustomer { get; set; }  
        public string? nameCustomer { get; set; }
        public decimal? limit { get; set; }
        public decimal? outstandingReceivable { get; set; }
        public int? doc {  get; set; }
        public decimal? creditLimitBalance { get; set; }
        public DateTime dateCreatedAT { get; set; } 
    }
}
