using BYSResults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ServicingSystem.BLL;
using ServicingSystem.ViewModels;
using System.Security.Claims;

namespace ProjectWebApp.Components.Pages.Service
{
    partial class ServiceScreen
    {
        #region Parameters
        [Parameter] public int CustomerId { get; set; }
        [Parameter] public int VehicleId { get; set; }
        #endregion

        #region Fields
        private List<ServiceCartItemView> serviceCart = new();
        private List<PartCartItemView> partCart = new();
        private List<CategoryView> salesCategories = new();
        private List<PartView> availableParts = new();
        private Dictionary<int, int> partQuantities = new();
        private List<PartView> removedVehicleParts = new();
        private List<PartView> unifiedParts = new();

        private ServiceView? selectedStandardService;
        private ServiceCartItemView? selectedServiceCartItem;
        private PartCartItemView? selectedPartCartItem;
        private CategoryView? salesSelectedCategory;
        private string customServiceDescription = string.Empty;
        private decimal serviceHours = 0;
        private string serviceComment = string.Empty;
        private string couponCode = string.Empty;
        private decimal discount = 0;
        private decimal discountPercentage = 0;

        private bool noRecords = false;
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        private List<string> errorDetails = new();
        private List<PartView> VehicleParts = new();

        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage) || errorDetails.Any();

        private CustomerView? CurrentCustomer;
        private CustomerVehicleView? CurrentVehicle;


        private string fullName = string.Empty;
        private string userID = string.Empty;
        private string userRole = string.Empty;


        #endregion

        #region Properties
        [Inject]
        protected ServiceManagementService ServiceManagementService { get; set; } = default!;

        [Inject]
        protected CustomerLookupService CustomerLookupService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        protected IDialogService DialogService { get; set; } = default;

        protected List<ServiceView> StandardServices { get; set; } = new();

        private decimal serviceSubtotal => serviceCart
            .Where(x => !x.RemoveFromViewFlag)
            .Sum(s => s.ExtPrice);

        private decimal partsSubtotal => unifiedParts
            .Where(x => !x.RemoveFromViewFlag)
            .Sum(p => p.Total);

        private decimal subtotal => serviceSubtotal + partsSubtotal;
        private decimal gst => (subtotal - discount) * 0.05m;
        private decimal total => subtotal - discount + gst;
        #endregion

        #region Methods
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();


            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            
            if (user.Identity is { IsAuthenticated: true })
            {
                fullName = user.FindFirst("FullName")?.Value ?? "Unknown User";
                userID = user.FindFirst("EmployeeID")?.Value ?? "";
                var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
                userRole = string.Join(", ", roles);
            }

            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                if (CustomerId > 0 && VehicleId > 0)
                {
                    await LoadCustomerAndVehicle();
                }

                var result = ServiceManagementService.GetStandardServices(0, 0);
                if (result.IsSuccess)
                {
                    StandardServices = result.Value;
                }
                else
                {
                    noRecords = true;
                    errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                }

                await LoadCategories();

                var couponResult = ServiceManagementService.GetJobCoupon(VehicleId);
                if (couponResult.IsSuccess && !string.IsNullOrEmpty(couponResult.Value))
                {
                    couponCode = couponResult.Value;
                    await VerifyCoupon();
                }
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }

        private async Task LoadCustomerAndVehicle()
        {
            try
            {
                var customerResult = ServiceManagementService.GetCustomerById(CustomerId);
                if (customerResult.IsSuccess)
                {
                    CurrentCustomer = customerResult.Value;
                    CurrentVehicle = CurrentCustomer.Vehicles.FirstOrDefault(v => v.CustomerVehicleID == VehicleId);

                    if (CurrentVehicle != null)
                    {
                        var runningServicesResult = ServiceManagementService.GetStandardServices(CustomerId, VehicleId);
                        if (runningServicesResult.IsSuccess)
                        {

                            var newServices = serviceCart.Where(s => !s.IsRunning).ToList();
                            serviceCart.Clear();
                            serviceCart.AddRange(newServices);


                            serviceCart.AddRange(runningServicesResult.Value.Select(rs => new ServiceCartItemView
                            {
                                ServiceID = rs.ServiceID,
                                Description = rs.Description,
                                Hours = rs.StandardHours,
                                Rate = rs.Rate,
                                StartDate = rs.StartDate,
                                Comment = rs.Comment
                            }));
                        }


                        var vehicleResult = ServiceManagementService.GetPartsForVehicle(VehicleId);
                        if (vehicleResult.IsSuccess)
                        {
                            VehicleParts = vehicleResult.Value;
                            LoadUnifiedParts();
                        }
                    }
                }
                else
                {
                    errorDetails = HelperClass.GetErrorMessages(customerResult.Errors.ToList());
                }
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }

        private async Task LoadCategories()
        {
            try
            {
                var result = ServiceManagementService.GetCategories();
                if (result.IsSuccess)
                {
                    salesCategories = result.Value;
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

        private async Task LoadParts()
        {
            if (salesSelectedCategory == null)
            {
                availableParts.Clear();
                partQuantities.Clear();
                feedbackMessage = "No category selected";
                return;
            }

            try
            {
                feedbackMessage = $"Searching for salesParts in category ID: {salesSelectedCategory.CategoryID}";
                var result = ServiceManagementService.GetPartsByCategory(salesSelectedCategory.CategoryID, VehicleId);
                if (result.IsSuccess)
                {
                    availableParts = result.Value;
                    partQuantities.Clear();
                    foreach (var part in availableParts)
                    {
                        partQuantities[part.PartID] = 0;
                    }
                    feedbackMessage = $"Found {availableParts.Count} salesParts for this vehicle";
                }
                else
                {
                    availableParts.Clear();
                    partQuantities.Clear();
                    errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                    feedbackMessage = "No salesParts found for this category and vehicle";
                }
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
                availableParts.Clear();
                partQuantities.Clear();
            }
        }

        private void UpdateTotals()
        {
            UpdateDiscount();
            StateHasChanged();
        }

        

        #endregion

        #region Services

        private async Task OnCategoryChanged(CategoryView category)
        {
            salesSelectedCategory = category;
            feedbackMessage = $"Loading salesParts for category: {category?.Description ?? "None"}";
            await LoadParts();
            StateHasChanged();
        }

        private async Task OnStandardServiceChanged(ServiceView? service)
        {
            selectedStandardService = service;
            if (service != null)
            {
                serviceHours = service.StandardHours;
                customServiceDescription = string.Empty;
            }
            StateHasChanged();
        }

        private void AddService()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                string description = selectedStandardService?.Description ?? customServiceDescription;


                if (string.IsNullOrWhiteSpace(description))
                {
                    errorMessage = "Please select a service or enter a custom service description.";
                    return;
                }

                if (serviceHours <= 0)
                {
                    errorMessage = "Service hours must be greater than 0.";
                    return;
                }

                if (serviceHours > 24)
                {
                    errorMessage = "Service hours cannot exceed 24 hours per service.";
                    return;
                }


                if (serviceCart.Any(s => s.Description.Equals(description, StringComparison.OrdinalIgnoreCase) && !s.RemoveFromViewFlag))
                {
                    errorMessage = $"Service '{description}' has already been added to the cart.";
                    return;
                }


                serviceCart.Add(new ServiceCartItemView
                {
                    Description = description,
                    Hours = serviceHours,
                    Comment = serviceComment ?? string.Empty,
                    IsCustom = selectedStandardService == null,
                    Rate = 65.50m
                });


                selectedStandardService = null;
                customServiceDescription = string.Empty;
                serviceHours = 0;
                serviceComment = string.Empty;

                UpdateTotals();
                feedbackMessage = $"Service '{description}' added successfully.";
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while adding the service:";
                errorDetails.Add(HelperClass.GetInnerException(ex).Message);
            }
        }

        private async Task ResetService()
        {
            bool? result = await DialogService.ShowMessageBox("Confirm Service Selection",
            "Selecting will reset any entered form data. " +
            "Are you sure you want to continue?",
            yesText: "Yes", cancelText: "Cancel");

            if (result != true)
            {
                return;
            }

            selectedStandardService = null;
            customServiceDescription = string.Empty;
            serviceHours = 0;
            serviceComment = string.Empty;


        }

        private async Task RemoveService(ServiceCartItemView service)
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                if (service.IsRunning)
                {
                    bool? result = await DialogService.ShowMessageBox("Confirm Remove",
                        $"Are you sure you want to remove the running service '{service.Description}'?\n\n" +
                        "Note: This will remove the service.",
                        yesText: "Remove", cancelText: "Cancel");

                    if (result != true)
                    {
                        return;
                    }


                    var removeResult = ServiceManagementService.RemoveRunningService(service.ServiceID);
                    if (removeResult.IsSuccess)
                    {
                        service.RemoveFromViewFlag = true;
                        feedbackMessage = $"Running service '{service.Description}' has been marked for removal.";
                    }
                    else
                    {
                        errorMessage = "Failed to remove running service:";
                        errorDetails = HelperClass.GetErrorMessages(removeResult.Errors.ToList());
                        return;
                    }
                }
                else
                {

                    bool? result = await DialogService.ShowMessageBox("Confirm Remove",
                        $"Are you sure you want to remove the service '{service.Description}' from the cart?",
                        yesText: "Remove", cancelText: "Cancel");

                    if (result != true)
                    {
                        return;
                    }

                    service.RemoveFromViewFlag = true;
                    feedbackMessage = $"Service '{service.Description}' has been removed from the cart.";
                }

                UpdateTotals();
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while removing the service:";
                errorDetails.Add(HelperClass.GetInnerException(ex).Message);
            }
        }



        private void OnServiceHoursChanged(ServiceCartItemView service, decimal newHours)
        {
            service.Hours = newHours;
            UpdateTotals();
        }

        private void OnServiceRateChanged(ServiceCartItemView service, decimal newRate)
        {
            service.Rate = newRate;
            UpdateTotals();
        }

        private void AddPart(PartView part)
        {
            // Clear previous error/feedback messages
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            errorDetails.Clear();

            if (!partQuantities.TryGetValue(part.PartID, out int qty) || qty <= 0)
            {
                errorMessage = "Please specify a quantity greater than 0.";
                return;
            }

            var existingItem = partCart.FirstOrDefault(p => p.PartID == part.PartID && !p.RemoveFromViewFlag);
            int quantityAlreadyInCart = existingItem?.Quantity ?? 0;

            if ((qty + quantityAlreadyInCart) > part.Quantity)
            {
                errorMessage = $"Cannot add {qty} more units of {part.Description}. You have {part.Quantity} available in stock.";
                return;
            }

            if (existingItem != null)
            {
                existingItem.Quantity += qty;
            }
            else
            {
                partCart.Add(new PartCartItemView
                {
                    PartID = part.PartID,
                    Description = part.Description,
                    Price = part.SellingPrice,
                    Quantity = qty
                });
            }

            partQuantities[part.PartID] = 0;
            LoadUnifiedParts();
            UpdateTotals();
            feedbackMessage = $"Added {qty} {part.Description} to cart.";
        }


        private void LoadUnifiedParts()
        {
            unifiedParts.Clear();


            if (VehicleParts != null)
            {
                foreach (var vehiclePart in VehicleParts.Where(p => !removedVehicleParts.Contains(p)))
                {
                    unifiedParts.Add(new PartView
                    {
                        PartID = vehiclePart.PartID,
                        Description = vehiclePart.Description,
                        SellingPrice = vehiclePart.SellingPrice,
                        Quantity = vehiclePart.Quantity,
                        CategoryDescription = vehiclePart.CategoryDescription,
                        IsExistingVehiclePart = true,
                        IsNewPart = false
                    });
                }
            }


            foreach (var cartPart in partCart.Where(p => !p.RemoveFromViewFlag))
            {

                var availablePart = availableParts.FirstOrDefault(ap => ap.PartID == cartPart.PartID);

                unifiedParts.Add(new PartView
                {
                    PartID = cartPart.PartID,
                    Description = cartPart.Description,
                    SellingPrice = cartPart.Price,
                    Quantity = cartPart.Quantity,
                    CategoryDescription = availablePart?.CategoryDescription ?? "",
                    IsExistingVehiclePart = false,
                    IsNewPart = true
                });
            }
        }

        private void OnUnifiedPartQuantityChanged(PartView part, int newQuantity)
        {
            part.Quantity = newQuantity;


            if (part.IsExistingVehiclePart)
            {
                var vehiclePart = VehicleParts.FirstOrDefault(p => p.PartID == part.PartID);
                if (vehiclePart != null)
                {
                    vehiclePart.Quantity = newQuantity;
                }
            }
            else if (part.IsNewPart)
            {
                var cartPart = partCart.FirstOrDefault(p => p.PartID == part.PartID && !p.RemoveFromViewFlag);
                if (cartPart != null)
                {
                    cartPart.Quantity = newQuantity;
                }
            }

            UpdateTotals();
        }

        private void RemoveUnifiedPart(PartView part)
        {
            if (part.IsExistingVehiclePart)
            {
                var vehiclePart = VehicleParts.FirstOrDefault(p => p.PartID == part.PartID);
                if (vehiclePart != null)
                {
                    removedVehicleParts.Add(vehiclePart);
                }
            }
            else if (part.IsNewPart)
            {
                var cartPart = partCart.FirstOrDefault(p => p.PartID == part.PartID && !p.RemoveFromViewFlag);
                if (cartPart != null)
                {
                    cartPart.RemoveFromViewFlag = true;
                }
            }

            part.RemoveFromViewFlag = true;
            LoadUnifiedParts();
            UpdateTotals();
        }

        private void UpdateDiscount()
        {
            if (discountPercentage > 0)
            {
                discount = subtotal * (discountPercentage / 100m);
            }
            else
            {
                discount = 0;
            }
        }

        private async Task VerifyCoupon()
        {
            try
            {
                var result = ServiceManagementService.ValidateCoupon(couponCode);
                if (result.IsSuccess)
                {
                    discountPercentage = result.Value;
                    feedbackMessage = $"Coupon applied: {result.Value}% discount";
                    errorMessage = string.Empty;
                    errorDetails.Clear();
                }
                else
                {
                    discountPercentage = 0;
                    errorMessage = "Invalid or expired coupon";
                    errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
                }
                UpdateTotals();
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }

        private void ClearCoupon()
        {
            couponCode = string.Empty;
            discountPercentage = 0;
            feedbackMessage = "Coupon cleared";
            errorMessage = string.Empty;
            errorDetails.Clear();
            UpdateTotals();

            var result = ServiceManagementService.RemoveJobCoupon(VehicleId);
            if (!result.IsSuccess)
            {
                errorMessage = "Failed to remove coupon from the job.";
                errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
            }
        }

        private async Task RegisterServiceOrder()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(couponCode))
                {
                    var removeResult = ServiceManagementService.RemoveJobCoupon(VehicleId);
                    if (!removeResult.IsSuccess)
                    {
                        errorMessage = "Failed to remove coupon from the job.";
                        errorDetails.AddRange(HelperClass.GetErrorMessages(removeResult.Errors.ToList()));
                        return;
                    }
                }

                var saveResult = ServiceManagementService.SaveServiceOrder(CustomerId, VehicleId, serviceCart, unifiedParts, couponCode);

                if (!saveResult.IsSuccess)
                {
                    errorMessage = "Failed to save service order:";
                    errorDetails.AddRange(HelperClass.GetErrorMessages(saveResult.Errors.ToList()));
                    return;
                }

                var jobId = saveResult.Value;
                feedbackMessage = $"Service order registered successfully! Job ID: {jobId}. " +
                                $"Services: {serviceCart.Count(s => !s.RemoveFromViewFlag)}, " +
                                $"Parts: {unifiedParts.Count(p => !p.RemoveFromViewFlag)}, " +
                                $"Total: {total:C2}";

                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                errorMessage = HelperClass.GetInnerException(ex).Message;
            }
        }

        private async Task CancelWithConfirmation()
        {
            bool? result = await DialogService.ShowMessageBox("Confirm Cancel",
                "Are you sure you want to cancel? All unsaved data will be lost.",
                yesText: "Yes, Cancel", cancelText: "No");

            if (result == true)
            {
                Cancel();
                NavigationManager.NavigateTo(
                  $"/Service/CustomerLookup");

            }
        }

        private void Cancel()
        {
            serviceCart.Clear();
            partCart.Clear();
            unifiedParts.Clear();
            removedVehicleParts.Clear();
            ResetService();
            couponCode = string.Empty;
            discountPercentage = 0;
            feedbackMessage = string.Empty;
            errorMessage = string.Empty;
            errorDetails.Clear();
            UpdateTotals();
        }
        #endregion
    }
}
