using SalesReturnsSystem.ViewModels;


namespace ProjectWebApp.Services
{
    public class SalesStateService
    {
        public CustomerViewModel? SelectedCustomer { get; private set; }
        public List<SaleDetailViewModel> CartItems { get; private set; } = new();
        public CouponViewModel? AppliedCoupon { get; private set; }

        public void SetCustomer(CustomerViewModel customer)
        {
            SelectedCustomer = customer;
            CartItems.Clear();
            AppliedCoupon = null;
        }

        public void AddToCart(SaleDetailViewModel item)
        {
            var existing = CartItems.FirstOrDefault(x => x.PartID == item.PartID);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                CartItems.Add(item);
            }
        }

        public void RemoveFromCart(SaleDetailViewModel item)
        {
            CartItems.Remove(item);
        }



        public void ApplyCoupon(CouponViewModel? coupon) => AppliedCoupon = coupon;
        public void ClearCoupon() => AppliedCoupon = null;

        public void ClearCart() => CartItems.Clear();

        public decimal Subtotal
        {
            get
            {
                return CartItems.Sum(item => item.ExtPrice);
            }
        }

    }
}
