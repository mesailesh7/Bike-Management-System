using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using ReceivingSystem.BLL;
using ReceivingSystem.ViewModels;
using System.Security.Claims;

namespace ProjectWebApp.Components.Pages.Receiving
{
    public partial class CheckInDeliveries
    {

        #region Fields
        [Parameter] public int poId { get; set; }

        //data models
        protected PurchaseOrderHeaderView? poHeader;
        protected List<OrderDetailView> orderDetails = new();
        protected List<UnorderedItemView> unorderedItems = new();
        protected UnorderedItemView newUnordered = new();

        protected string forceCloseReason = string.Empty;

        //user info
        protected string UserFullName { get; set; } = string.Empty;
        protected string UserId { get; set; } = string.Empty;
        protected string UserRole { get; set; } = string.Empty;

        private MudForm? receivingForm;

        #endregion

        #region Feedback / Error Messages
        //alerts
        protected string Feedback { get; set; } = string.Empty;
        protected string ErrorMessage { get; set; } = string.Empty;
        protected List<string> ErrorMsgs { get; set; } = new();
       

        //validation flags
        private bool _isValid;    
        private bool _hasChanges;
        private bool _unorderedQtyError;
        private bool IsAuthenticated;
        private bool IsPartsManager;
        private bool IsShopManager;
        private bool _showSaveCancelButtons = false;

        #endregion

        #region Properties
        [Inject] protected ReceivingService ReceivingService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        #endregion

        #region Helpers


        private bool HasPOChanges() =>
            orderDetails.Any(d =>
                d.Received > 0 ||
                d.Returned > 0 ||
                (d.Returned > 0 && !string.IsNullOrWhiteSpace(d.Reason)));

        // Unordered table
        private bool HasUnorderedChanges() => unorderedItems.Any();

       
        private bool AnyChanges() =>
            HasPOChanges() || HasUnorderedChanges() || !string.IsNullOrWhiteSpace(forceCloseReason);

      
        private static bool HasRowError(OrderDetailView d) =>
            d.Received < 0 ||
            d.Received > d.OutstandingBase ||
            d.Returned < 0 ||
            (d.Returned > 0 && string.IsNullOrWhiteSpace(d.Reason));

      
        private bool HasErrors => _unorderedQtyError || orderDetails.Any(HasRowError);

     
        private bool CanReceive =>
        _isValid &&
        (HasPOChanges() || HasUnorderedChanges() || !string.IsNullOrWhiteSpace(forceCloseReason)) &&
        !HasErrors;

        private void MarkChanged()
        {
            _hasChanges = AnyChanges();       
            _ = receivingForm?.Validate();
            StateHasChanged();
        }




        private bool IsUnorderedValid() =>
        !string.IsNullOrWhiteSpace(newUnordered.Description)
        && !string.IsNullOrWhiteSpace(newUnordered.VendorPartId)
        && newUnordered.Quantity >= 1;

        #endregion

        #region Lifecycle
        protected override void OnInitialized()
        {
            try
            {
                poHeader = ReceivingService.GetPurchaseOrderHeader(poId);
                orderDetails = ReceivingService.GetOrderDetails(poId) ?? new List<OrderDetailView>();
                PrefillEditableFromDbTotals();

               
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading PO {poId}: {ex.Message}";
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            UserFullName = user.FindFirst("FullName")?.Value ?? "Unknown User";
            UserRole = user.FindFirst("Role")?.Value ?? "";
            UserId = user.FindFirst("EmployeeID")?.Value ?? "";
          
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
            IsPartsManager = user.IsInRole("Parts Manager");
            IsShopManager = user.IsInRole("Shop Manager");

        }

        #endregion

        #region Actions

        private void OnReceive()
        {
            ErrorMessage = string.Empty;
            ErrorMsgs.Clear();
            Feedback = string.Empty;

          
            if (!HasPOChanges() && !HasUnorderedChanges() && string.IsNullOrWhiteSpace(forceCloseReason))
            {
                ErrorMessage = "Nothing to receive. Enter a Received/Returned quantity or add an unordered item.";
                StateHasChanged();
                return;
            }

            if (HasErrors)
            {
                ErrorMessage = "Validation failed. Please fix the errors below.";
                StateHasChanged();
                return;
            }

            foreach (var detail in orderDetails)
            {
                if (detail.Received < 0)
                    ErrorMsgs.Add($"Part {detail.PartId}: Received cannot be negative.");

                if (detail.Received > detail.OutstandingBase)
                    ErrorMsgs.Add($"Part {detail.PartId}: Received ({detail.Received}) cannot exceed Outstanding ({detail.OutstandingBase}).");

                if (detail.Returned < 0)
                    ErrorMsgs.Add($"Part {detail.PartId}: Returned cannot be negative.");

                if (detail.Returned > detail.OrderQty)
                    ErrorMsgs.Add($"Part {detail.PartId}: Returned ({detail.Returned}) cannot exceed Ordered ({detail.OrderQty}).");

                if (detail.Returned > 0 && string.IsNullOrWhiteSpace(detail.Reason))
                    ErrorMsgs.Add($"Part {detail.PartId}: Reason required when returning items.");
            }

            if (_unorderedQtyError)
                ErrorMsgs.Add("Unordered Item: Quantity must be greater than zero.");

            if (ErrorMsgs.Any())
            {
                ErrorMessage = "Validation failed. Please fix the errors below.";
            }
            else
            {
                Feedback = "Items successfully validated.";
                _showSaveCancelButtons = true; 
            }

            StateHasChanged();
        }



        private async Task OnForceCloseAsync()
        {
            if (string.IsNullOrWhiteSpace(forceCloseReason))
            {
                ErrorMessage = "Reason required to force close an order.";
                StateHasChanged();
                return;
            }

            var confirm = await DialogService.ShowMessageBox(
                "Confirm Force Close",
                $"Are you sure you want to force close this order for reason: '{forceCloseReason}'?",
                yesText: "Yes, Force Close",
                cancelText: "Cancel",
                options: new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true }
            );

            if (confirm == true)
            {
                try
                {
                    await ReceivingService.SaveReceiveChangesAsync(orderDetails, unorderedItems, UserId);
                    await ReceivingService.ForceClosePurchaseOrderAsync(poId, forceCloseReason);

                    
                    ErrorMessage = string.Empty;
                    ErrorMsgs.Clear();
                    _hasChanges = false;
                    await ResetFormAsync();
                    Feedback = $"Order force closed for reason: {forceCloseReason}.";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error while force closing: {ex.Message}";
                }
                StateHasChanged();
            }
        }

        private async Task OnResetAsync()
        {
            bool? result = await DialogService.ShowMessageBox(
                "Confirm Reset",
                "Are you sure you want to reset all changes? This action cannot be undone.",
                yesText: "Yes, Reset",
                cancelText: "Cancel",
                options: new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small });

            if (result == true)
            {
                Feedback = "Form has been reset.";
                await ResetFormAsync();  
            }
        }




  
        private async Task ResetFormAsync()
        {
            try
            {
              
                poHeader = ReceivingService.GetPurchaseOrderHeader(poId);
                orderDetails = ReceivingService.GetOrderDetailsFresh(poId) ?? new();
                PrefillEditableFromDbTotals();

              
                unorderedItems.Clear();
                newUnordered = new UnorderedItemView();
                forceCloseReason = string.Empty;

                ErrorMsgs.Clear();
                ErrorMessage = string.Empty;
                //Feedback = "Form has been reset.";
                _hasChanges = false;
                _unorderedQtyError = false;

               
                if (receivingForm is not null)
                {
                    receivingForm.ResetValidation();
                    await receivingForm.Validate();
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Reset failed: {ex.Message}";
            }
        }

        private async Task OnCancelAsync()
        {


            bool? confirm = await DialogService.ShowMessageBox(
       "Confirm Cancel",
       "Do you wish to close this page? All unsaved changes will be lost.",
       yesText: "Yes, Close",
       cancelText: "No, Keep Editing");

            if (confirm != true) return;

            NavigationManager.NavigateTo("/receiving/outstanding");

        }


        private string? ValidateReceived(OrderDetailView item, int? v)
        {
            if (!v.HasValue) return "Received qty required.";
            if (v.Value < 0) return "Received cannot be negative.";
            if (v.Value > item.OutstandingBase)
                return $"Received cannot exceed Outstanding ({item.OutstandingBase}).";
            return null;
        }

        private async Task SaveChanges()
        {
            var confirmSave = await DialogService.ShowMessageBox(
                "Confirm Save",
                "Are you sure you want to save the changes?",
                yesText: "Yes, Save",
                cancelText: "No, Cancel",
                options: new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true }
            );

            if (confirmSave == true)
            {
                try
                {
                    await ReceivingService.SaveReceiveChangesAsync(orderDetails, unorderedItems, UserId);

                    //Summarize changes
                    var changes = new List<string>();

                    foreach (var detail in orderDetails)
                    {
                        if (detail.Received > 0 || detail.Returned > 0)
                        {
                            changes.Add(
                                $"Part {detail.PartId}: Received {detail.Received}, Returned {detail.Returned}" +
                                (detail.Returned > 0 && !string.IsNullOrWhiteSpace(detail.Reason) ? $", Reason: {detail.Reason}" : "")
                            );
                        }
                    }

                    foreach (var item in unorderedItems)
                    {
                        changes.Add($"Unordered Item: {item.Description}, Vendor Part ID: {item.VendorPartId}, Quantity: {item.Quantity}");
                    }

                    Feedback = changes.Any()
                        ? "Changes processed:\n" + string.Join("\n", changes)
                        : "No changes were made.";

                    await ResetFormAsync();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error while saving: {ex.Message}";
                }
                _showSaveCancelButtons = false;
                StateHasChanged();
            }
            else
            {
                Feedback = "Save operation was canceled.";
                StateHasChanged();
            }
        }

        private async Task CancelChanges()
        {

            var confirmCancel = await DialogService.ShowMessageBox(
                "Confirm Cancel",
                "Are you sure you want to cancel? All unsaved changes will be lost.",
                yesText: "Yes, Cancel",
                cancelText: "No, Keep Editing",
                options: new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true }
            );

            if (confirmCancel == true)
            {

                Feedback = "Changes have been canceled.";
                _showSaveCancelButtons = false;
                StateHasChanged();
            }
            else
            {

                Feedback = "Cancel operation was canceled.";
                StateHasChanged();
            }
        }




        #endregion

        #region Live handlers
        //live validation for PO

        private void OnReceivedChanged(OrderDetailView row, int newValue)
        {
            ErrorMsgs.RemoveAll(e => e.StartsWith($"Part {row.PartId}: Received"));

            row.Received = newValue;

            string? err = null;
            if (newValue < 0)
                err = "Received cannot be negative.";
            else if (newValue > row.OutstandingBase)
                err = $"Received cannot exceed Outstanding ({row.OutstandingBase}).";

            if (err != null) ErrorMsgs.Add($"Part {row.PartId}: {err}");

            ErrorMessage = ErrorMsgs.Any() ? "Validation failed. Please fix the errors below." : string.Empty;

            MarkChanged(); 
        }


        private void OnReasonChanged(OrderDetailView context, string newValue)
        {
            context.Reason = newValue;
            RefreshValidation(context);
            MarkChanged();
            _ = receivingForm?.Validate();
        }


        private void OnReturnedChanged(OrderDetailView r, int v)
        {
            r.Returned = v;
            ErrorMsgs.RemoveAll(e => e.StartsWith($"Part {r.PartId}: Returned"));
            if (v < 0)
            {
                ErrorMsgs.Add($"Part {r.PartId}: Returned cannot be negative.");
                ErrorMessage = "Validation failed. Please fix the errors below.";
            }
            else
            {
                ErrorMsgs.RemoveAll(e => e.StartsWith($"Part {r.PartId}: Returned"));
            }

            if (r.Returned > 0 && string.IsNullOrWhiteSpace(r.Reason))
            {
                ErrorMsgs.Add($"Part {r.PartId}: Reason required when returning items.");
                ErrorMessage = "Validation failed. Please fix the errors below.";
            }
            else
            {
                ErrorMsgs.RemoveAll(e => e.StartsWith($"Part {r.PartId}: Reason"));
            }

            RefreshValidation(r);
            MarkChanged();
            StateHasChanged();
        }


        #endregion

        #region Validation



        private void RefreshValidation(OrderDetailView r)
        {
            ErrorMsgs.RemoveAll(e => e.Contains($"Part {r.PartId}:"));

            if (r.Received < 0)
                ErrorMsgs.Add($"Part {r.PartId}: Received cannot be negative.");
       
            if (r.Received > r.OutstandingBase)
                ErrorMsgs.Add($"Part {r.PartId}: Received cannot exceed Outstanding ({r.OutstandingBase}).");

            if (r.Returned < 0)
                ErrorMsgs.Add($"Part {r.PartId}: Returned cannot be negative.");

            if (r.Returned > 0 && string.IsNullOrWhiteSpace(r.Reason))
                ErrorMsgs.Add($"Part {r.PartId}: Reason required when returning items.");

            ErrorMessage = ErrorMsgs.Any() ? "Validation failed. Please fix the errors below." : string.Empty;
            StateHasChanged();
        }



        //for unordered items
        private void OnUnorderedQtyChanged(int newValue)
        {
            newUnordered.Quantity = newValue;
            RefreshUnorderedValidation();
            StateHasChanged();
        }

       
        private void RefreshUnorderedValidation()
        {

            ErrorMsgs.RemoveAll(e => e.StartsWith("Unordered Item: Quantity", StringComparison.OrdinalIgnoreCase));

            if (newUnordered.Quantity <= 0)
            {
                _unorderedQtyError = true;
                ErrorMsgs.Add("Unordered Item: Quantity must be greater than zero.");
                ErrorMessage = "Validation failed. Please fix the errors below.";
            }
            else
            {
                _unorderedQtyError = false;


                if (!ErrorMsgs.Any()) ErrorMessage = string.Empty;
            }
        }
        #endregion

        #region Unordered Items Methods (Add, Clear, Remove)

        //Unordered Items Methods
        private void AddUnorderedItem()
        {
          
            ErrorMessage = string.Empty;
            ErrorMsgs.Clear();
            Feedback = string.Empty;

            if (string.IsNullOrWhiteSpace(newUnordered.Description))
                ErrorMsgs.Add("Unordered Item: Description is required.");
            if (string.IsNullOrWhiteSpace(newUnordered.VendorPartId))
                ErrorMsgs.Add("Unordered Item: Vendor Part ID is required.");
            if (newUnordered.Quantity <= 0)
                ErrorMsgs.Add("Unordered Item: Quantity must be greater than zero.");

            if (ErrorMsgs.Any())
            {
                ErrorMessage = "Validation failed. Please fix the errors below.";
                return;
            }

            unorderedItems.Add(new UnorderedItemView
            {
                Description = newUnordered.Description,

                VendorPartId = newUnordered.VendorPartId,
                Quantity = newUnordered.Quantity
            });

          
            newUnordered = new UnorderedItemView();
            Feedback = "Unordered item added successfully.";
            MarkChanged();

        }


        private async Task ClearNewUnordered()
        {
            bool? result = await DialogService.ShowMessageBox(
                "Confirm Clear",
                "Are you sure you want to clear the unordered item input? This action cannot be undone.",
                yesText: "Yes, Clear",
                cancelText: "Cancel",
                options: new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small }
            );

            if (result == true)
            {
                newUnordered = new UnorderedItemView();
                Feedback = "Unordered item input cleared.";
                ErrorMessage = string.Empty;
                ErrorMsgs.Clear();
                StateHasChanged();
            }
        }


        private async Task RemoveUnorderedItem(UnorderedItemView item)
        {
            bool? confirm = await DialogService.ShowMessageBox(
                "Confirm Delete",
                $"Are you sure you want to delete the unordered item: '{item.Description}'?",
                yesText: "Yes, Delete",
                cancelText: "Cancel",
                options: new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small }
            );

            if (confirm == true)
            {
                unorderedItems.Remove(item);
                Feedback = $"Unordered item '{item.Description}' removed.";
                ErrorMessage = string.Empty;
                ErrorMsgs.Clear();
                MarkChanged();
                StateHasChanged();
            }
        }

        #endregion

        #region Extra methods
        private void PrefillEditableFromDbTotals()
        {
            foreach (var d in orderDetails)
            {
                d.Received = 0;
                d.Returned = 0;
                d.Reason = string.Empty;
                d.HasEditedReceived = false;
            }
        }

        private string GetHelperText(OrderDetailView item)
        {
            return $"Previously received: {item.ReceivedToDate}";
        }
        #endregion

    }
}
