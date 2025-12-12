using BYSResults;
using Microsoft.EntityFrameworkCore;
using PurchasingSystem.DAL;
using PurchasingSystem.Entities;
using PurchasingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.BLL
{
    public class PurchaseOrderService
    {
        #region Date Field
        private readonly EBikeContext _context;
        #endregion

        // Construction for CategoryService class
        internal PurchaseOrderService(EBikeContext context)
        {
            // If the passed in context is null, throw an exception.
            _context = context ?? throw new ArgumentException(nameof(context)); ;
        }

        public Result<List<PurchaseOrderView>> GetAllOrders()
        {
            var result = new Result<List<PurchaseOrderView>>();

            var orders = _context.PurchaseOrders
                    .Where(x => !x.RemoveFromViewFlag)
                    .OrderBy(x => x.PurchaseOrderID)
                    .Select(x => new PurchaseOrderView
                    {
                        PurchaseOrderID = x.PurchaseOrderID,
                        PurchaseOrderNumber = x.PurchaseOrderNumber,
                        OrderDate = x.OrderDate,
                        TaxAmount = x.TaxAmount,
                        SubTotal = x.SubTotal,
                        Closed = x.Closed,
                        Notes = x.Notes,
                        EmployeeID = x.EmployeeID,
                        VendorID = x.VendorID,
                        VendorName = x.Vendor.VendorName,
                        RemoveFromViewFlag = x.RemoveFromViewFlag,
                    })
                    .ToList();

            if (orders == null || orders.Count == 0)
            {
                result.AddError(new Error("No purchase orders", "No any purchase orders were found"));
                return result;
            }

            return result.WithValue(orders);
        }
        public Result<PurchaseOrderView> GetOrderByID(int orderID)
        {
            var result = new Result<PurchaseOrderView>();

            var order = _context.PurchaseOrders
                    .Where(x => x.PurchaseOrderID == orderID &&
                                !x.RemoveFromViewFlag)
                    .Select(x => new PurchaseOrderView
                    {
                        PurchaseOrderID = x.PurchaseOrderID,
                        PurchaseOrderNumber = x.PurchaseOrderNumber,
                        OrderDate = x.OrderDate,
                        TaxAmount = x.TaxAmount,
                        SubTotal = x.SubTotal,
                        Closed = x.Closed,
                        Notes = x.Notes,
                        EmployeeID = x.EmployeeID,
                        VendorID = x.VendorID,
                        VendorName = x.Vendor.VendorName,
                        RemoveFromViewFlag = x.RemoveFromViewFlag,
                    })
                    .FirstOrDefault();

            if (order == null)
            {
                result.AddError(new Error("No purchase orders", $"No any purchase orders were found by order ID: {orderID}"));
                return result;
            }

            return result.WithValue(order);
        }
        public Result<List<PurchaseOrderView>> GetOrdersByVenderID(int vendorID)
        {
            var result = new Result<List<PurchaseOrderView>>();

            var orders = _context.PurchaseOrders
                    .Where(x => x.VendorID == vendorID &&
                                !x.RemoveFromViewFlag)
                    .OrderBy(x => x.PurchaseOrderID)
                    .Select(x => new PurchaseOrderView
                    {
                        PurchaseOrderID = x.PurchaseOrderID,
                        PurchaseOrderNumber = x.PurchaseOrderNumber,
                        OrderDate = x.OrderDate,
                        TaxAmount = x.TaxAmount,
                        SubTotal = x.SubTotal,
                        Closed = x.Closed,
                        Notes = x.Notes,
                        EmployeeID = x.EmployeeID,
                        VendorID = x.VendorID,
                        VendorName = x.Vendor.VendorName,
                        RemoveFromViewFlag = x.RemoveFromViewFlag,
                    })
                    .ToList();

            if (orders == null || orders.Count == 0)
            {
                result.AddError(new Error("No purchase orders", $"No any purchase orders were found by vendor ID: {vendorID}"));
                return result;
            }

            return result.WithValue(orders);
        }

        public Result<PurchaseOrderView> AddEditOrder(PurchaseOrderView orderView)
        {
            var result = new Result<PurchaseOrderView>();

            if (orderView == null)
            {
                result.AddError(new Error("Missing Order", "No purchase order was supplied"));
                return result;
            }

            // Rule: Vendor ID must be supply
            if (orderView.VendorID == 0)
            {
                result.AddError(new Error("Missing Information", "Please provide a valid vendor ID"));
            }

            // Rule: The must be detail lines provided
            if (orderView.Details.Count == 0)
            {
                result.AddError(new Error("Missing Information", "Purchase order details are required"));
            }

            foreach (var detailView in orderView.Details)
            {
                if (detailView.PartID == 0)
                {
                    result.AddError(new Error("Missing Information", "Missing part ID"));
                    return result;
                }

                // Rule: for each purchase detail line, the price cannot be less than zero
                if (detailView.PurchasePrice < 0)
                {
                    result.AddError(new Error("Invalid Price", $"Part {detailView.Description} has a price that is less than zero"));
                }

                // Rule: for each purchase detail line, the quantity cannot be less than 1
                if (detailView.Quantity < 1)
                {
                    result.AddError(new Error("Invalid Quantity", $"Part {detailView.Description} has a quantity that is less than 1"));
                }
            }

            // Rule: salesParts cannot be duplicated on more than one line
            List<string> duplicatedParts = orderView.Details
                .GroupBy(x => new { x.PartID, x.Description})
                .Where(gb => gb.Count() > 1)
                .OrderBy(gb => gb.Key.PartID)
                .Select(gb => gb.Key.Description)
                .ToList();

            if (duplicatedParts.Count > 0)
            {
                foreach(var description in duplicatedParts)
                {
                    result.AddError(new Error("Duplicate Order Detail Items",
                        $"Part {description} can only be added to the order detail lines once"));
                }
            }

            // Return if there are any errors
            if (result.IsFailure)
            {
                return result;
            }

            // Retrieve the order from the database or create a new one if it doesn't exist
            PurchaseOrder order = _context.PurchaseOrders
                .Where(x => x.PurchaseOrderID == orderView.PurchaseOrderID)
                .FirstOrDefault();
            if (order == null)
            {
                order = new PurchaseOrder();
            }

            // Update purchase order properties from view model
            order.PurchaseOrderNumber = orderView.PurchaseOrderNumber;
            order.OrderDate = orderView.OrderDate;
            order.TaxAmount = orderView.TaxAmount;
            order.SubTotal = orderView.SubTotal;
            order.Closed = orderView.Closed;
            order.Notes = orderView.Notes;
            order.EmployeeID = orderView.EmployeeID;
            order.VendorID = orderView.VendorID;
            order.RemoveFromViewFlag = orderView.RemoveFromViewFlag;

            // Process each line item in the provided view model
            foreach (var detailView in orderView.Details)
            {
                PurchaseOrderDetail orderDetail = _context.PurchaseOrderDetails
                    .Where(x => x.PurchaseOrderDetailID == detailView.PurchaseOrderDetailID &&
                                !x.RemoveFromViewFlag)
                    .FirstOrDefault();
                
                // If the detail item doesn't exist, initialize it.
                if (orderDetail == null)
                {
                    orderDetail = new PurchaseOrderDetail();
                    orderDetail.PartID = detailView.PartID;
                }

                // Update purchase order detail properties from view model
                orderDetail.Quantity = detailView.Quantity;
                orderDetail.PurchasePrice = detailView.PurchasePrice;
                orderDetail.VendorPartNumber = detailView.VendorPartNumber;
                orderDetail.RemoveFromViewFlag = detailView.RemoveFromViewFlag;

                // Handle new or existing detail items
                if (orderDetail.PurchaseOrderDetailID == 0)
                {
                    order.PurchaseOrderDetails.Add(orderDetail);
                }
                else
                {
                    _context.PurchaseOrderDetails.Update(orderDetail);
                }
            }

            // If it is a new order, add it to the collection
            if (order.PurchaseOrderID == 0)
            {
                _context.PurchaseOrders.Add(order);
            }
            else
            {
                _context.PurchaseOrders.Update(order);
            }

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                result.AddError(new Error("Error Saving Changes", ex.InnerException.Message));
                return result;
            }

            return GetOrderByID(order.PurchaseOrderID);
        }
        // Place this order
        //  rule 1: Only set OrderDate=Now for order update, other unsaved order data will be
        //          ingored. So AddEditOrder() needs to be call before calling this function.
        //  rule 2: Update salesParts QOO which are in details
        public Result<int> PlaceOrder(PurchaseOrderView orderView)
        {
            var result = new Result<int>();

            if (orderView == null)
            {
                result.AddError(new Error("Missing Order", "No purchase order was supplied"));
                return result;
            }

            // Rule: Vendor ID must be supply
            if (orderView.VendorID == 0)
            {
                result.AddError(new Error("Missing Information", "Please provide a valid vendor ID"));
            }

            // Rule: The must be detail lines provided
            if (orderView.Details.Count == 0)
            {
                result.AddError(new Error("Missing Information", "Purchase order details are required"));
                return result;
            }

            foreach (var detailView in orderView.Details)
            {
                if (detailView.PartID == 0)
                {
                    result.AddError(new Error("Missing Information", "Missing part ID"));
                    return result;
                }

                // Rule: for each purchase detail line, the price cannot be less than zero
                if (detailView.PurchasePrice < 0)
                {
                    result.AddError(new Error("Invalid Price", $"Part {detailView.Description} has a price that is less than zero"));
                }

                // Rule: for each purchase detail line, the quantity cannot be less than 1
                if (detailView.Quantity < 1)
                {
                    result.AddError(new Error("Invalid Quantity", $"Part {detailView.Description} has a quantity that is less than 1"));
                }
            }

            // Rule: salesParts cannot be duplicated on more than one line
            List<string> duplicatedParts = orderView.Details
                .GroupBy(x => new { x.PartID, x.Description })
                .Where(gb => gb.Count() > 1)
                .OrderBy(gb => gb.Key.PartID)
                .Select(gb => gb.Key.Description)
                .ToList();

            if (duplicatedParts.Count > 0)
            {
                foreach (var description in duplicatedParts)
                {
                    result.AddError(new Error("Duplicate Order Detail Items",
                        $"Part {description} can only be added to the order detail lines once"));
                }
            }

            // Return if there are any errors
            if (result.IsFailure)
            {
                return result;
            }

            // Order must exist
            PurchaseOrder order = _context.PurchaseOrders
                .Where(x => x.PurchaseOrderID == orderView.PurchaseOrderID)
                .FirstOrDefault();
            if (order == null)
            {
                result.AddError(new Error("No order", $"No any order was found by ID: {orderView.PurchaseOrderID}"));
                return result;
            }

            // Update salesParts QuantityOnOrder
            foreach (var detail in orderView.Details)
            {
                var part = _context.Parts
                    .Where(x => x.PartID == detail.PartID)
                    .FirstOrDefault();
                if (part == null)
                {
                    result.AddError(new Error("No part", $"No any part was found by ID: {detail.PartID}"));
                }
                else
                {
                    part.QuantityOnOrder += detail.Quantity;
                    _context.Update(part);
                }
            }
            if (result.IsFailure)
            {
                _context.ChangeTracker.Clear();
                return result;
            }

            // Place this order by setting OrderDate
            order.OrderDate = DateTime.Now;
            _context.Update(order);

            try
            {
                return result.WithValue(_context.SaveChanges());
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                result.AddError(new Error("Error Saving Changes", ex.InnerException.Message));
                return result;
            }
        }
        public Result<int> DeleteOrderLogically(PurchaseOrderView orderView)
        {
            var result = new Result<int>();

            if (orderView == null)
            {
                result.AddError(new Error("Missing Order", "No purchase order was supplied"));
                return result;
            }

            // Find order entity
            var order = _context.PurchaseOrders
                .Where(x => x.PurchaseOrderID == orderView.PurchaseOrderID)
                .FirstOrDefault();
            if (order == null)
            {
                result.AddError(new Error("No order", $"No purchase order was found by ID: {orderView.PurchaseOrderID}"));
                return result;
            }

            // Set all detail lines are deleted
            foreach (var detailView in orderView.Details)
            {
                var detail = _context.PurchaseOrderDetails
                                .Where(x => x.PurchaseOrderDetailID == detailView.PurchaseOrderDetailID)
                                .FirstOrDefault();
                if (detail != null)
                {
                    detail.RemoveFromViewFlag = true;
                    _context.Update(detail);
                }
            }

            // Set this order is deleted
            order.RemoveFromViewFlag = true;
            _context.Update(order);

            // Update to database
            try
            {
                return result.WithValue(_context.SaveChanges());
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                result.AddError(new Error("Error Saving Changes", ex.InnerException.Message));
                return result;
            }
        }
    }
}
