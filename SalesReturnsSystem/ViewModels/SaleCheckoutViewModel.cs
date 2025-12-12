using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class CartItemViewModel
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal ExtendedPrice => Price * Quantity;
    }

    public class SalesCheckoutViewModel
    {
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.ExtendedPrice);
        public decimal Discount => Subtotal * DiscountPercent;
        public decimal Tax => (Subtotal - Discount) * 0.05m;
        public decimal Total => Subtotal - Discount + Tax;
    }
}


