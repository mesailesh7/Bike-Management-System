using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ServicingSystem.BLL;
using ServicingSystem.ViewModels;
using MudBlazor;
using BYSResults;
using System.Security.Claims;

namespace ProjectWebApp.Components.Pages.Service;

public partial class CustomerLookUp
{
    #region Fields
    private string feedback = string.Empty;
    private string errorMessage = string.Empty;
    private bool noRecords = false;
    private bool hasFeedback => !string.IsNullOrWhiteSpace(feedback);
    private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
    private List<string> errorDetails = new();

    private string lastName = string.Empty;
    public bool hasVehicle = false;
    public string selectedVehicleId = string.Empty;


    private string fullName = string.Empty;
    private string userID = string.Empty;
    private string userRole = string.Empty;


    private bool isAuthorized => userRole.Contains("Mechanic") || userRole.Contains("Parts Manager") || userRole.Contains("Admin - All Roles");

    protected List<CustomerView> Customers { get; set; } = new();
    private CustomerView SelectedCustomer = new();
    protected List<CustomerVehicleView> Vehicle { get; set; } = new();
    #endregion

    #region Properties
    [Inject] protected CustomerLookupService CustomerLookupService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
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
    }
    #endregion

    private void Search()
    {
        noRecords = false;
        errorDetails.Clear();
        errorMessage = string.Empty;
        feedback = string.Empty;

        try
        {
            var result = CustomerLookupService.GetCustomerByLastName(lastName);

            if (result.IsSuccess)
            {
                Customers = result.Value ?? new();
            }
            else
            {
                errorMessage = "Please address the following errors";
                noRecords = true;
                errorDetails = HelperClass.GetErrorMessages(result.Errors.ToList());
            }
        }
        catch (Exception ex)
        {
            errorMessage = HelperClass.GetInnerException(ex).Message;
        }
    }

    private void SelectCustomer(CustomerView customer)
    {
        SelectedCustomer = customer ?? new();
    }

    private async Task Clear()
    {
        bool? result = await DialogService.ShowMessageBox("Confirm Customer Selection",
            "Selecting will reset any entered form data. Are you sure you want to continue?",
            yesText: "Yes", cancelText: "Cancel");

        if (result != true)
            return;

        noRecords = false;
        errorDetails.Clear();
        errorMessage = string.Empty;
        feedback = string.Empty;
        lastName = string.Empty;
        selectedVehicleId = string.Empty;
        SelectedCustomer = new();
        Customers.Clear();
    }

    private void NavigateToServiceScreen()
    {
        if (!string.IsNullOrEmpty(selectedVehicleId))
        {
            var selectedVehicle = SelectedCustomer.Vehicles
                ?.FirstOrDefault(v => v.VehicleIdentification == selectedVehicleId);
            if (selectedVehicle != null)
            {
                NavigationManager.NavigateTo(
                    $"/Service/ServiceScreen/{SelectedCustomer.Id}/{selectedVehicle.CustomerVehicleID}");
            }
        }
    }
}