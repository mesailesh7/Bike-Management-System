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
    public class PurchaseOrderDetailService
    {
        #region Date Field
        private readonly EBikeContext _context;
        #endregion

        // Construction for CategoryService class
        internal PurchaseOrderDetailService(EBikeContext context)
        {
            // If the passed in context is null, throw an exception.
            _context = context ?? throw new ArgumentException(nameof(context)); ;
        }

        public Result<List<PurchaseOrderDetailView>> GetDetailsByOrderID(int orderID)
        {
            var result = new Result<List<PurchaseOrderDetailView>>();

            var details = _context.PurchaseOrderDetails
                    .Where(x => x.PurchaseOrderID == orderID &&
                                !x.RemoveFromViewFlag)
                    .OrderBy(x => x.PurchaseOrderDetailID)
                    .Select(x => new PurchaseOrderDetailView
                    {
                        PurchaseOrderID = x.PurchaseOrderID,
                        PurchaseOrderDetailID = x.PurchaseOrderDetailID,
                        PartID = x.PartID,
                        Description = x.Part.Description,
                        ROL = x.Part.ReorderLevel,
                        QOH = x.Part.QuantityOnHand,
                        QOO = x.Part.QuantityOnOrder,
                        Quantity = x.Quantity,
                        PurchasePrice = x.PurchasePrice,
                        VendorPartNumber = x.VendorPartNumber,
                        RemoveFromViewFlag = x.RemoveFromViewFlag

                    })
                    .ToList();

            if (details == null || details.Count == 0)
            {
                result.AddError(new Error("No orders details", $"No any purchase orders details were found by order ID: {orderID}"));
                return result;
            }

            return result.WithValue(details);
        }
    }
}
