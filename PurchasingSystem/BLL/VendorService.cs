using BYSResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PurchasingSystem.DAL;
using PurchasingSystem.ViewModels;

namespace PurchasingSystem.BLL
{
    public class VendorService
    {
        #region Date Field
        private readonly EBikeContext _context;
        #endregion

        // Construction for CategoryService class
        internal VendorService(EBikeContext context)
        {
            // If the passed in context is null, throw an exception.
            _context = context ?? throw new ArgumentException(nameof(context)); ;
        }

        public Result<List<VendorView>> GetVendors(int id, string name)
        {
            var result = new Result<List<VendorView>>();

            Guid tempGuild = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = tempGuild.ToString();
            }

            var vendors = _context.Vendors
                    .Where(x => ((id > 0) ? x.VendorID == id : true) &&
                                ((name != tempGuild.ToString()) ? x.VendorName.ToLower().Contains(name.ToLower()) : true ) &&
                                !x.RemoveFromViewFlag)
                    .Select(x => new VendorView
                    {
                        VendorID = x.VendorID,
                        VendorName = x.VendorName,
                        Phone = x.Phone,
                        Address = x.Address,
                        City = x.City,
                        ProvinceID = x.ProvinceID,
                        PostalCode = x.PostalCode,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    })
                    .ToList();
            if (vendors == null)
            {
                result.AddError(new Error("No vendors", "No any vendors were found"));
                return result;
            }

            return result.WithValue(vendors);
        }

        public Result<List<VendorView>> GetAllVendors()
        {
            var result = new Result<List<VendorView>>();

            var vendors = _context.Vendors
                    .Where(x => !x.RemoveFromViewFlag)
                    .OrderBy(x => x.VendorID)
                    .Select(x => new VendorView
                    {
                        VendorID = x.VendorID,
                        VendorName = x.VendorName,
                        Phone = x.Phone,
                        Address = x.Address,
                        City = x.City,
                        ProvinceID = x.ProvinceID,
                        PostalCode = x.PostalCode,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    })
                    .ToList();
            if (vendors == null || vendors.Count == 0)
            {
                result.AddError(new Error("No vendors", "No any vendors were found"));
                return result;
            }

            return result.WithValue(vendors);
        }
        public Result<VendorView> GetVenderByID(int vendorID)
        {
            var result = new Result<VendorView>();

            var vendors = _context.Vendors
                    .Where(x => x.VendorID == vendorID &&
                                !x.RemoveFromViewFlag)
                    .Select(x => new VendorView
                    {
                        VendorID = x.VendorID,
                        VendorName = x.VendorName,
                        Phone = x.Phone,
                        Address = x.Address,
                        City = x.City,
                        ProvinceID = x.ProvinceID,
                        PostalCode = x.PostalCode,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    })
                    .FirstOrDefault();
            if (vendors == null)
            {
                result.AddError(new Error("No vendors", $"No any vendors were found with ID: {vendorID}"));
                return result;
            }

            return result.WithValue(vendors);
        }
    }
}
