using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class SaleReturnItemViewModel
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int OriginalQty { get; set; }
        public int AlreadyReturned { get; set; }
        public bool Refundable { get; set; }
        public int QtyToReturn { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxReturnable => Math.Max(0, OriginalQty - AlreadyReturned);

    }

    public class SaleReturnViewModel
    {
        public int SaleId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public List<SaleReturnItemViewModel> Items { get; set; } = new();
        public decimal Subtotal => Math.Round(Items.Sum(x => x.Price * x.QtyToReturn), 2);
        public decimal Discount => Math.Round(Subtotal * (DiscountPercent / 100m), 2);

        public decimal Tax => Math.Round((Subtotal - Discount) * 0.05m, 2);

        public decimal Total => Subtotal - Discount + Tax;


    }
}
