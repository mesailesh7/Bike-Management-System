namespace SalesReturnsSystem.ViewModels
{
    public class SaleViewModel
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total => SubTotal + TaxAmount;
        public int? CouponID { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public List<SaleDetailViewModel> Items { get; set; } = new();
    }
}
