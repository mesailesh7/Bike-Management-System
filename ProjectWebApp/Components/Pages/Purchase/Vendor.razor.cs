using Microsoft.AspNetCore.Components;
using PurchasingSystem.BLL;
using PurchasingSystem.ViewModels;

namespace ProjectWebApp.Components.Pages.Purchase
{
    partial class Vendor
    {
        #region Data Field
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        private List<string> errorDetails = new();
        // has feedback
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage) || errorDetails.Any();
        #endregion

        #region Properties
        [Inject]
        protected VendorService VendorService { get; set; } = default;
        [Inject]
        protected PurchaseOrderService PurchaseOrderService { get; set; } = default;
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default;
        protected List<VendorView> Vendors { get; set; } = new();
        protected string company = string.Empty;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                // Show all vendors in default
                var result = VendorService.GetVendors(0, "");
                if (result.IsSuccess)
                {
                    Vendors = result.Value ?? new();
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
        private void SearchVendors()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                var result = VendorService.GetVendors(0, company);
                if (result.IsSuccess)
                {
                    Vendors = result.Value ?? new();
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
        private void MakeOrder(int vendorID)
        {
            // Find the unplaced order for this vendor
            var orderID = PurchaseOrderService
                .GetOrdersByVenderID(vendorID)
                .Value?
                .FirstOrDefault(x => x.OrderDate == null)?
                .PurchaseOrderID ?? 0;

            // Open the order edit page
            NavigationManager.NavigateTo($"Purchase/MakeOrder/{orderID}/{vendorID}");
        }
    }
}
