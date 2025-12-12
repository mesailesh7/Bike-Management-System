using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalesReturnsSystem.DAL;
using SalesReturnsSystem.Entities;
using SalesReturnsSystem.ViewModels;

namespace SalesReturnsSystem.BLL
{
    public class ReturnTransactionService
    {
        private readonly eBike_2025Context _context;

        public ReturnTransactionService(eBike_2025Context context)
        {
            _context = context;
        }

        // 1) Lookup a sale and build the VM used by the page
        public async Task<SaleReturnViewModel?> GetSaleForReturnAsync(int saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleDetails).ThenInclude(sd => sd.Part)
                .Include(s => s.Coupon)
                .FirstOrDefaultAsync(s => s.SaleID == saleId);

            if (sale == null)
                return null;

            var vm = new SaleReturnViewModel
            {
                SaleId = sale.SaleID,
                CustomerName = sale.Customer.FirstName + " " + sale.Customer.LastName,
                SaleDate = sale.SaleDate,
                PaymentMethod = sale.PaymentType,
                // NOTE: your VM expects a FRACTION (0.10 for 10%)
                DiscountPercent = sale.Coupon != null ? sale.Coupon.CouponDiscount / 100m : 0m,
                Items = sale.SaleDetails.Select(d => new SaleReturnItemViewModel
                {
                    PartId = d.PartID,
                    PartName = d.Part.Description,
                    OriginalQty = d.Quantity,
                    AlreadyReturned = _context.SaleRefundDetails
                        .Where(rd => rd.SaleRefund.SaleID == sale.SaleID && rd.PartID == d.PartID)
                        .Sum(rd => rd.Quantity),
                    Price = d.SellingPrice,
                    Refundable = d.Part.Refundable == "Y"
                }).ToList()
            };

            return vm;
        }

        // 2) Process the return using the SAME VM the page edits
        public async Task<int> ProcessReturnAsync(ReturnRequestViewModel request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Items == null || !request.Items.Any(i => i.Qty > 0))
                throw new InvalidOperationException("No quantities selected to return.");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Load sale (for discount) and original lines (to validate remaining qty)
                var sale = await _context.Sales
                    .Include(s => s.Coupon)
                    .FirstOrDefaultAsync(s => s.SaleID == request.SaleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                var details = await _context.SaleDetails
                    .Where(d => d.SaleID == request.SaleId)
                    .Select(d => new
                    {
                        d.PartID,
                        d.Quantity,
                        d.SellingPrice
                    })
                    .ToListAsync();

                // Map returned so far
                var returnedSoFar = await _context.SaleRefundDetails
                    .Where(r => r.SaleRefund.SaleID == request.SaleId)
                    .GroupBy(r => r.PartID)
                    .ToDictionaryAsync(g => g.Key, g => g.Sum(x => x.Quantity));

                // Validate each requested line
                foreach (var line in request.Items.Where(i => i.Qty > 0))
                {
                    var original = details.FirstOrDefault(d => d.PartID == line.PartId)
                        ?? throw new InvalidOperationException($"Part {line.PartId} was not on this sale.");

                    var already = returnedSoFar.TryGetValue(line.PartId, out var r) ? r : 0;
                    var remaining = original.Quantity - already;

                    if (line.Qty < 0) throw new InvalidOperationException("Return quantity cannot be negative.");
                    if (line.Qty > remaining)
                        throw new InvalidOperationException(
                            $"Return qty for part {line.PartId} exceeds remaining ({remaining}).");
                }

                // Totals (recalculate server-side)
                var subtotal = request.Items.Sum(x => x.Price * x.Qty);
                var discountPercent = sale.Coupon != null ? sale.Coupon.CouponDiscount / 100m : 0m;
                var discount = Math.Round(subtotal * discountPercent, 2);
                var tax = Math.Round((subtotal - discount) * 0.05m, 2);

                // Create refund header
                var refund = new SaleRefund
                {
                    SaleRefundDate = DateTime.Now,
                    SaleID = request.SaleId,
                    EmployeeID = "currentUser", // TODO: set from logged-in user
                    SubTotal = subtotal,
                    TaxAmount = tax
                };
                _context.SaleRefunds.Add(refund);
                await _context.SaveChangesAsync(); // get generated SaleRefundID

                // Lines & inventory
                foreach (var line in request.Items.Where(i => i.Qty > 0))
                {
                    _context.SaleRefundDetails.Add(new SaleRefundDetail
                    {
                        SaleRefundID = refund.SaleRefundID,
                        PartID = line.PartId,
                        Quantity = line.Qty,
                        SellingPrice = line.Price,
                        Reason = line.Reason ?? string.Empty
                    });

                    var part = await _context.Parts.FirstAsync(p => p.PartID == line.PartId);
                    part.QuantityOnHand += line.Qty; // put stock back
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return refund.SaleRefundID;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

    }
}
