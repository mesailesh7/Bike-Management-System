using Microsoft.EntityFrameworkCore;
using SalesReturnsSystem.DAL;
using SalesReturnsSystem.ViewModels;
using SalesReturnsSystem.Entities;

namespace SalesReturnsSystem.BLL
{
    public class SalesTransactionService
    {
        private readonly eBike_2025Context _context;

        public SalesTransactionService(eBike_2025Context context)
        {
            _context = context;
        }

        // Search salesCustomers by phone
        public async Task<List<CustomerViewModel>> SearchCustomersAsync(string phone)
        {
            return await _context.Customers
                .Where(c => c.ContactPhone.Contains(phone))
                .Select(c => new CustomerViewModel
                {
                    CustomerID = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    ContactPhone = c.ContactPhone,
                    Address = c.Address + ", " + c.City
                })
                .ToListAsync();
        }

        // Get salesParts optionally filtered by category
        public async Task<List<Part>> GetPartsByCategoryAsync(int? categoryId = null)
        {
            var query = _context.Parts.AsQueryable();
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryID == categoryId.Value);

            return await query.ToListAsync();
        }

        // Validate coupon
        public async Task<decimal> ValidateCouponAsync(string code)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.CouponIDValue == code && c.EndDate >= DateTime.Now);

            return coupon?.CouponDiscount ?? 0;
        }
    }
}
