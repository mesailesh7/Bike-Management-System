using BYSResults;
using PurchasingSystem.DAL;
using PurchasingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.BLL
{
    public class PartService
    {
        #region Date Field
        private readonly EBikeContext _context;
        #endregion

        // Construction for PartService class
        internal PartService(EBikeContext context)
        {
            // If the passed in context is null, throw an exception.
            _context = context ?? throw new ArgumentException(nameof(context)); ;
        }
        public Result<List<PartView>> GetParts(int vendorID, int categoryID, string description, List<int>existingPartIDs )
        {
            var result = new Result<List<PartView>>();

            if (vendorID == 0 && 
                categoryID == 0 && 
                string.IsNullOrWhiteSpace(description))
            {
                result.AddError(new Error("Missing Information",
                    "Please provide either vendor or a category and/or description"));
                return result;
            }

            Guid tempGuild = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(description))
            {
                description = tempGuild.ToString();
            }

            var salesParts = _context.Parts
                    .Where(x => (vendorID > 0 ? x.VendorID==vendorID : true) &&
                                (categoryID > 0 ? x.CategoryID==categoryID : true) &&
                                ((description != tempGuild.ToString()) ?
                                 x.Description.ToLower().Contains(description.ToLower()) : true) &&
                                !existingPartIDs.Contains(x.PartID) &&
                                !x.RemoveFromViewFlag)
                    .OrderBy(x => x.PartID)
                    .Select(x => new PartView
                    {
                        PartID = x.PartID,
                        Description = x.Description,
                        PurchasePrice = x.PurchasePrice,
                        SellingPrice = x.SellingPrice,
                        QuantityOnHand = x.QuantityOnHand,
                        ReorderLevel = x.ReorderLevel,
                        QuantityOnOrder = x.QuantityOnOrder,
                        CategoryID = x.CategoryID,
                        Refundable = x.Refundable,
                        Discontinued = x.Discontinued,
                        VendorID = x.VendorID,
                        RemoveFromViewFlag = x.RemoveFromViewFlag

                    })
                    .ToList();

            if (salesParts == null || salesParts.Count == 0)
            {
                result.AddError(new Error("No salesParts", "No any salesParts were found"));
                return result;
            }

            return result.WithValue(salesParts);
        }
        public Result<PartView> GetPartByID(int partID)
        {
            var result = new Result<PartView>();

            var part = _context.Parts
                    .Where(x => x.PartID == partID &&
                                !x.RemoveFromViewFlag)
                    .Select(x => new PartView
                    {
                        PartID = x.PartID,
                        Description = x.Description,
                        PurchasePrice = x.PurchasePrice,
                        SellingPrice = x.SellingPrice,
                        QuantityOnHand = x.QuantityOnHand,
                        ReorderLevel = x.ReorderLevel,
                        QuantityOnOrder = x.QuantityOnOrder,
                        CategoryID = x.CategoryID,
                        Refundable = x.Refundable,
                        Discontinued = x.Discontinued,
                        VendorID = x.VendorID,
                        RemoveFromViewFlag = x.RemoveFromViewFlag

                    })
                    .FirstOrDefault();

            if (part == null)
            {
                result.AddError(new Error("No part", $"No any salesParts were found by ID: {partID}"));
                return result;
            }

            return result.WithValue(part);
        }
        public Result<List<PartView>> GetPartsByVendorID(int vendorID)
        {
            var result = new Result<List<PartView>>();

            var salesParts = _context.Parts
                    .Where(x => x.VendorID == vendorID &&
                                !x.RemoveFromViewFlag)
                    .OrderBy(x => x.PartID)
                    .Select(x => new PartView
                    {
                        PartID = x.PartID,
                        Description = x.Description,
                        PurchasePrice = x.PurchasePrice,
                        SellingPrice = x.SellingPrice,
                        QuantityOnHand = x.QuantityOnHand,
                        ReorderLevel = x.ReorderLevel,
                        QuantityOnOrder = x.QuantityOnOrder,
                        CategoryID = x.CategoryID,
                        Refundable = x.Refundable,
                        Discontinued = x.Discontinued,
                        VendorID = x.VendorID,
                        RemoveFromViewFlag = x.RemoveFromViewFlag

                    })
                    .ToList();

            if (salesParts == null || salesParts.Count == 0)
            {
                result.AddError(new Error("No salesParts", $"No any salesParts were found by vendor ID: {vendorID}"));
                return result;
            }

            return result.WithValue(salesParts);
        }
    }
}
