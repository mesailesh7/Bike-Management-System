using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SalesReturnsSystem.DAL;
using SalesReturnsSystem.Entities;
using Microsoft.EntityFrameworkCore;
using SalesReturnsSystem.ViewModels;


namespace SalesReturnsSystem.BLL
{
    public class SalesService
    {
        private readonly eBike_2025Context _context;

        public SalesService(eBike_2025Context context)
        {
            _context = context;
        }

        public async Task<CouponViewModel?> ValidateCouponAsync(string couponCode)
        {
            return await _context.Coupons
                .Where(c => c.CouponIDValue == couponCode
                            && c.StartDate <= DateTime.Now
                            && c.EndDate >= DateTime.Now
                            && !c.RemoveFromViewFlag)
                .Select(c => new CouponViewModel
                {
                    CouponID = c.CouponID,
                    CouponCode = c.CouponIDValue,
                    CouponIDValue = c.CouponIDValue,
                    CouponDiscount = c.CouponDiscount,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    RemoveFromViewFlag = c.RemoveFromViewFlag
                })
                .FirstOrDefaultAsync();
        }


        public async Task<List<CategoryViewModel>> GetCategories()
        {
            return await _context.Categories
                .Where(c => !c.RemoveFromViewFlag)
                .Select(c => new CategoryViewModel
                {
                    CategoryID = c.CategoryID,
                    Description = c.Description,
                    ItemCount = _context.Parts.Count(p => p.CategoryID == c.CategoryID)
                }).ToListAsync();
        }

        public async Task<List<PartViewModel>> GetPartsByCategory(int categoryId)
        {
            return await _context.Parts
                .Where(p => p.CategoryID == categoryId && !p.Discontinued)
                .Select(p => new PartViewModel
                {
                    PartID = p.PartID,
                    Description = p.Description,
                    SellingPrice = p.SellingPrice,
                    QOH = p.QuantityOnHand
                }).ToListAsync();
        }

        // Search salesCustomers by partial phone or name
        public async Task<List<CustomerViewModel>> SearchCustomersAsync(string phone)
        {
            return await _context.Customers
                .Where(c =>
                        (c.ContactPhone.Contains(phone) || c.LastName.Contains(phone))
                         && !c.RemoveFromViewFlag)
                .Select(c => new CustomerViewModel
                {
                    CustomerID = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    ContactPhone = c.ContactPhone,
                    Address = c.Address,
                    City = c.City
                }).ToListAsync();
        }




    }
}
