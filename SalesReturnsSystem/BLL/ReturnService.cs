using Microsoft.EntityFrameworkCore;
using SalesReturnsSystem.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SalesReturnsSystem.DAL;

namespace SalesReturnsSystem.BLL
{
    public class ReturnService
    {
        private readonly eBike_2025Context _context;
        public ReturnService(eBike_2025Context context) { _context = context; }

        public async Task<SaleReturnViewModel?> GetSaleReturnAsync(int saleId)
        {
            // 1) Load the sale header as an entity first (one round-trip)
            var sale = await _context.Sales
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Coupon)
                .Where(s => s.SaleID == saleId)
                .SingleOrDefaultAsync();

            if (sale == null) return null;

            // 2) Load the lines in a second, independent query
            var lines = await _context.SaleDetails
                .AsNoTracking()
                .Where(d => d.SaleID == saleId)
                .Select(d => new SaleReturnItemViewModel
                {
                    PartId = d.PartID,
                    PartName = d.Part.Description,
                    OriginalQty = d.Quantity,
                    AlreadyReturned = _context.SaleRefundDetails
                        .Where(rd => rd.SaleRefund.SaleID == saleId && rd.PartID == d.PartID)
                        .Sum(rd => (int?)rd.Quantity) ?? 0,
                    Refundable = d.Part.Refundable == "Y",
                    Price = d.SellingPrice,
                    QtyToReturn = 0,
                    Reason = string.Empty
                })
                .ToListAsync();

            // 3) Build the VM after both queries are finished
            var vm = new SaleReturnViewModel
            {
                SaleId = sale.SaleID,
                CustomerName = sale.Customer.FirstName + " " + sale.Customer.LastName,
                SaleDate = sale.SaleDate,
                PaymentMethod = sale.PaymentType,
                DiscountPercent = sale.Coupon != null ? sale.Coupon.CouponDiscount / 100m : 0m,
                Items = lines
            };

            return vm;
        }

    }
}
