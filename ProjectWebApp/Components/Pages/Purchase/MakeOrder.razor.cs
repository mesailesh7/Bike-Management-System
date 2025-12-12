using BYSResults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using PurchasingSystem.BLL;
using PurchasingSystem.ViewModels;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ProjectWebApp.Components.Pages.Purchase
{
    partial class MakeOrder
    {
        #region Fields
        private VendorView vendor = new();
        private PurchaseOrderView order = new();
        private List<PartView> salesParts = new();
        private List<CategoryView> partCategories = new();
        private int categoryID = 0;
        private string description = string.Empty;
        private string saveButtonText => orderID==0 ? "Save" : "Update";
        private bool hasDataChanged = false;
        #endregion

        #region Feedback & Error Messages
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        private List<string> errorDetails = new List<string>();
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage) || errorDetails.Any();
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        #endregion

        #region Properties
        [Inject]
        protected VendorService VendorService { get; set; } = default;
        [Inject]
        protected PurchaseOrderService PurchaseOrderService { get; set; } = default;
        [Inject]
        protected PurchaseOrderDetailService PurchaseOrderDetailService { get; set; } = default;
        [Inject]
        protected PartService PartService { get; set; } = default;
        [Inject]
        protected CategoryService CategoryService { get; set; } = default;
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default;
        [Inject]
        protected IDialogService DialogService { get; set; } = default;

        [Inject]
        protected AuthenticationStateProvider authenticationStateProvider { get; set; } = default;
        [Parameter]
        public int orderID { get; set; }
        [Parameter]
        public int vendorID { get; set; }
        #endregion

        #region Methods
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
        // This function will be called when parameters have changed
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            hasDataChanged = false;

            // Rule: vendorID must be provided
            if (vendorID <= 0)
            {
                vendor = new();
                errorMessage = "Vendor ID was not provided.";
                return;
            }

            try
            {
                // Retrieve the unplaced order by ID
                //  Create a new one if it doesn't exist
                order = PurchaseOrderService.GetOrderByID(orderID)
                                            .Value ?? new();

                // Retrieve all salesParts
                SearchParts();

                // Retrieve the vendor information, MUST be existed
                var result = VendorService.GetVenderByID(vendorID);
                if (result.IsSuccess && result.Value != null)
                {
                    vendor = result.Value;

                    // Rule 1: For unplaced order, retrieve salesParts information from detail list
                    if (order.PurchaseOrderID > 0)
                    {
                        var detailsResult = PurchaseOrderDetailService.GetDetailsByOrderID(order.PurchaseOrderID);
                        if (detailsResult.IsSuccess)
                        {
                            order.Details = detailsResult.Value ?? new();
                        }
                        else
                        {
                            errorDetails = HelperClass.GetErrorMessages(detailsResult.Errors.ToList());
                        }
                    }
                    // Rule 2: For new order, make a suggested salesParts list for a new order
                    //      Add all salesParts which ROL > (ROH + ROO) for this vendor
                    else
                    {
                        // Get salesParts suggested to reoder
                        var partsSuggested = salesParts.FindAll(x => (x.VendorID == vendorID) && 
                                                                (x.ReorderLevel > (x.QuantityOnHand + x.QuantityOnOrder)) &&
                                                                !x.RemoveFromViewFlag);

                        List<PurchaseOrderDetailView> details = new();
                        foreach(var part in partsSuggested)
                        {
                            PurchaseOrderDetailView detail = new();
                            detail.PartID = part.PartID;
                            detail.Description = part.Description;
                            detail.ROL = part.ReorderLevel;
                            detail.QOH = part.QuantityOnHand;
                            detail.QOO = part.QuantityOnOrder;
                            detail.Quantity = detail.ROL - (detail.QOH + detail.QOO);
                            detail.PurchasePrice = part.PurchasePrice;
                            detail.VendorPartNumber = "";
                            detail.RemoveFromViewFlag = false;
                            details.Add(detail);
                        }

                        // Update initial data to the new order
                        order.VendorID = vendorID;
                        order.Details = details;
                        order.PurchaseOrderNumber = 0;  //TODO:
                        order.OrderDate = null;
                        order.Notes = null;
                        UpdateOrderSubtotalAndTax();

                        // Enable Save button
                        hasDataChanged = order.Details.Count > 0 ? true : false;
                    }

                    // Check errors
                    if (errorDetails.Count > 0)
                    {
                        errorMessage = $"Encounter errors when loading order details for vendor ID: {vendorID}";
                    }
                    else
                    {
                        // Retrieve part categories
                        partCategories = CategoryService.GetAllCategories();

                        // Retrieve available salesParts again
                        SearchParts();
                    }
                }
                else
                {
                    errorMessage = $"Failed to load vendor with ID: {vendorID}";
                    errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }

        // Search salesParts by Description string
        private void SearchParts()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            salesParts.Clear();

            // Part IDs which have been added
            List<int> existingPartIDs = order?.Details?
                        .Where(x => !x.RemoveFromViewFlag)
                        .Select(x => x.PartID)
                        .ToList() ?? new();
            try
            {
                var result = PartService.GetParts(vendorID, 
                                                  categoryID, 
                                                  description, 
                                                  existingPartIDs);
                if (result.IsSuccess)
                {
                    salesParts = result.Value ?? new();
                }
                else
                {
                    //errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                }
            }
            catch (Exception ex) 
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }

        // add part to order detail line and remove part from part list
        private async Task AddPart(int partID)
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                var result = PartService.GetPartByID(partID);
                if (result.IsSuccess)
                {
                    PartView part = result.Value;
                    PurchaseOrderDetailView detail = new();
                    detail.PartID = part.PartID;
                    detail.Description = part.Description;
                    detail.ROL = part.ReorderLevel;
                    detail.QOH = part.QuantityOnHand;
                    detail.QOO = part.QuantityOnOrder;
                    detail.Quantity = (detail.ROL > (detail.QOH + detail.QOO)) ?
                                      (detail.ROL - (detail.QOH + detail.QOO)) : 0;
                    detail.PurchasePrice = part.PurchasePrice;
                    detail.VendorPartNumber = "";
                    detail.RemoveFromViewFlag = false;
                    order.Details.Add(detail);

                    UpdateOrderSubtotalAndTax();

                    // remove the current part from the part list
                    salesParts.Remove(salesParts.Where(p => p.PartID == partID)
                        .FirstOrDefault());

                    // enable Save/Update button
                    hasDataChanged = true;

                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        // Remove a order detail line
        private async Task DeleteDetailLine(PurchaseOrderDetailView detailLine)
        {
            bool? results = await DialogService.ShowMessageBox("Confirm Delete",
                $"Are you sure that you wish to remove {detailLine.Description}?",
                yesText: "Remove", cancelText: "Cancel");

            if (results == true)
            {
                // For a new order, remove this detail line directly
                if (orderID == 0)
                {
                    order.Details.Remove(detailLine);
                }
                // For an existing order, set this detail line's RemoveFromViewFlag = true
                else
                {
                    detailLine.RemoveFromViewFlag = true;
                }
                UpdateOrderSubtotalAndTax();

                // Refresh available salesParts
                SearchParts();

                // enable Save/Update button
                hasDataChanged = true;
            }
        }

        // Update this order's Subtotal and tax amount for order
        private void UpdateOrderSubtotalAndTax()
        {
            order.SubTotal = order.Details?
                    .Where(x => !x.RemoveFromViewFlag)
                    .Sum(x => x.Quantity * x.PurchasePrice) ?? 0;

            order.TaxAmount = order.Details?
                    .Where(x => !x.RemoveFromViewFlag)
                    .Sum(x => x.Quantity * x.PurchasePrice * 0.05m) ?? 0;
        }
        #endregion

        #region Events
        private void QuantityEdited(PurchaseOrderDetailView detailView, int newQuantity)
        {
            hasDataChanged = true;
            detailView.Quantity = newQuantity;
            UpdateOrderSubtotalAndTax();
        }
        private void PriceEdited(PurchaseOrderDetailView detailView, decimal newPrice)
        {
            hasDataChanged = true;
            detailView.PurchasePrice = newPrice;
            UpdateOrderSubtotalAndTax();
        }
        private void SyncPrice(PurchaseOrderDetailView detailView)
        {
            //Find the original price of the Part from the database
            try
            {
                var result = PartService.GetPartByID(detailView.PartID);
                if (result.IsSuccess)
                {
                    PartView part = result.Value;
                    detailView.PurchasePrice = part.PurchasePrice;
                    hasDataChanged = true;
                    UpdateOrderSubtotalAndTax();
                }
                else
                {
                    errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        private void VendorPartNumberEdited(PurchaseOrderDetailView detailView, string newNumber)
        {
            hasDataChanged = true;
            detailView.VendorPartNumber = newNumber;
        }

        private async Task Save()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            // Update employee ID
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                order.EmployeeID = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            // Save order to database
            try
            {
                var result = PurchaseOrderService.AddEditOrder(order);
                if (result.IsSuccess && result.Value != null)
                {
                    // An new order was saved
                    if (orderID == 0)
                    {
                        NavigationManager.NavigateTo($"Purchase/MakeOrder/{result.Value.PurchaseOrderID}/{vendorID}");
                        feedbackMessage = $"New purchase order No {orderID} was created";
                    }
                    else
                    {
                        hasDataChanged = false;
                        feedbackMessage = $"Purchase order No {orderID} was updated";
                    }
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                }
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }
        private async Task Place()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            // User confirm
            bool? result = await DialogService.ShowMessageBox("Confirm Place",
                $"Are you sure that you wish to save and  place PO# {orderID}? A placed order cannot be edited again.",
                yesText: "Place", cancelText: "Cancel");
            if (result == null)
            {
                return;
            }

            try
            {
                // Place current order, all order new will be saved
                var resultPlace = PurchaseOrderService.PlaceOrder(order);
                if (resultPlace.IsSuccess)
                {
                    // User can choose create a new unplace order, or return Vendor List page
                    result = await DialogService.ShowMessageBox("Success",
                        $"PO# {orderID} has been placed successfully! Do you want to create an other new order for Vender {vendor.VendorName}?",
                        yesText: "Yes", cancelText: "No");
                    if (result == true)
                    {
                        NavigationManager.NavigateTo($"Purchase/MakeOrder/0/{vendorID}");
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"Purchase/Vendor");
                    }
                }
                else
                {
                    errorDetails = HelperClass.GetErrorMessages(resultPlace.Errors.ToList());
                }
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }
        private async Task Delete()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            // Return if current order is new (has not saved to database)
            if (orderID == 0)
            {
                errorMessage = "This order was not saved, cannot be deleted.";
                return;
            }

            // User confirm
            bool? result = await DialogService.ShowMessageBox("Confirm Delete",
                $"Are you sure that you wish to delete PO# {orderID}?",
                yesText: "Delete", cancelText: "Cancel");
            if (result == null)
            {
                return;
            }
            
            try
            {
                // Delete this order logically
                var deletingOrderID = order.PurchaseOrderID;
                var resultDelete = PurchaseOrderService.DeleteOrderLogically(order);
                if (resultDelete.IsSuccess)
                {
                    // User can choose create a new unplace order, or return Vendor List page
                    result = await DialogService.ShowMessageBox("Success",
                        $"PO# {orderID} has been deleted successfully! Do you want to create an other new order for Vender {vendor.VendorName}?",
                        yesText: "Yes", cancelText: "No");
                    if (result == true)
                    {
                        NavigationManager.NavigateTo($"Purchase/MakeOrder/0/{vendorID}");
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"Purchase/Vendor");
                    }
                }
                else
                {
                    errorDetails = HelperClass.GetErrorMessages(resultDelete.Errors.ToList());
                }
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }
        private async Task Clear()
        {
            // User confirm
            bool? result = await DialogService.ShowMessageBox("Confirm Clear",
                    $"Do you wish to clear current order? All unsaved changes will be lost.",
                    yesText: "Yes", cancelText: "No");
            if (result == null)
            {
                return;
            }

            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            hasDataChanged = false;

            // For a saved unplaced order, refresh page with its original data
            if (orderID > 0)
            {
                order = PurchaseOrderService.GetOrderByID(orderID).Value ?? new();
                order.Details = PurchaseOrderDetailService.GetDetailsByOrderID(orderID).Value ?? new();
            }
            // For a new order, remove all added lines
            else
            {
                order = new();
                order.Details = new();
                order.OrderDate = null;
                order.VendorID = vendorID;
            }

            // Retrieve all salesParts
            SearchParts();
        }

        private async Task Close()
        {
            // User confirm
            if (hasDataChanged)
            {
                bool? result = await DialogService.ShowMessageBox("Confirm Close",
                        $"Do you wish to return Vendor List page? All unsaved changes will be lost.",
                        yesText: "Yes", cancelText: "No");
                if (result == null)
                {
                    return;
                }
            }

            // Back to Vendor Selection page
            NavigationManager.NavigateTo("Purchase/Vendor");
        }
        #endregion
    }
}
