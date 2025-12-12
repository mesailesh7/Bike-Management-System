using BLL = SalesReturnsSystem.BLL;
using SalesReturnsSystem.ViewModels;
using ProjectWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ProjectWebApp.Components.Pages.SalesReturn
{
    public partial class Sales
    {
        private string phoneSearch = string.Empty;
        private List<CustomerViewModel> salesCustomers = new();
        private CustomerViewModel? salesSelectedCustomer;

        private List<CategoryViewModel> salesCategories = new();
        private CategoryViewModel? salesSelectedCategory;

        private List<PartViewModel> salesParts = new();
        private List<SaleDetailViewModel> salesCartItems = new();

        private decimal subtotal = 0;
        private decimal tax = 0;
        private decimal total = 0;

        private const decimal TaxRate = 0.05m;

        [Inject] private SalesStateService SalesState { get; set; } = default!;
        [Inject] private BLL.SalesService _salesService { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadCategories();
            salesCartItems = SalesState.CartItems;
            salesSelectedCustomer = SalesState.SelectedCustomer;
            UpdateTotals();

            // Load all customers on initial page load
            salesCustomers = await _salesService.SearchCustomersAsync("");
        }

        private async Task SearchCustomers()
        {
            salesCustomers = await _salesService.SearchCustomersAsync(phoneSearch);
        }

        private async Task ClearSearch()
        {
            phoneSearch = string.Empty;
            salesCustomers = await _salesService.SearchCustomersAsync("");
        }

        private async Task SelectCustomer(CustomerViewModel customer)
        {
            if (SalesState.CartItems.Any())
            {
                var confirmed = await Confirm("Switching customer will clear cart. Continue?");
                if (!confirmed) return;

                SalesState.ClearCart();
            }

            SalesState.SetCustomer(customer);
            salesSelectedCustomer = customer;
            salesCartItems = SalesState.CartItems;
        }

        private async Task LoadCategories()
        {
            salesCategories = await _salesService.GetCategories();
        }

        private async Task LoadParts(CategoryViewModel cat)
        {
            salesSelectedCategory = cat;
            salesParts = await _salesService.GetPartsByCategory(cat.CategoryID);
        }

        private void AddPartToCart(PartViewModel part, int qty)
        {
            if (qty <= 0 || qty > part.QOH)
                return;

            var newItem = new SaleDetailViewModel
            {
                PartID = part.PartID,
                PartName = part.Description,
                Price = part.SellingPrice,
                Quantity = qty,
                QOH = part.QOH
            };

            SalesState.AddToCart(newItem);
            salesCartItems = SalesState.CartItems;

            UpdateTotals();
        }

        private void RemoveCartItem(SaleDetailViewModel item)
        {
            SalesState.RemoveFromCart(item);
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            subtotal = SalesState.Subtotal;
            tax = subtotal * TaxRate;
            total = subtotal + tax;
        }

        private bool _confirmDialogVisible;
        private string _confirmMessage = string.Empty;
        private TaskCompletionSource<bool>? _tcsConfirm;

        private async Task<bool> Confirm(string message)
        {
            _confirmMessage = message;
            _confirmDialogVisible = true;
            _tcsConfirm = new TaskCompletionSource<bool>();
            StateHasChanged();
            return await _tcsConfirm.Task;
        }

        private void ConfirmResult(bool result)
        {
            _confirmDialogVisible = false;
            _tcsConfirm?.SetResult(result);
        }

        private async Task ConfirmCustomerChange(CustomerViewModel customer)
        {
            if (SalesState.CartItems.Any())
            {
                var confirmed = await Confirm("Switching customer will clear cart. Continue?");
                if (!confirmed) return;

                SalesState.ClearCart();
            }

            await SelectCustomer(customer);
        }


        private async Task Cancel()
        {
            if (SalesState.CartItems.Any())
            {
                var confirmed = await Confirm("Canceling will clear the cart. Are you sure?");
                if (!confirmed) return;

                SalesState.ClearCart();
            }

            NavManager.NavigateTo("/sales");
        }

        private void NavigateToProductSelector() => NavManager.NavigateTo("/products");
        private void NavigateToCart() => NavManager.NavigateTo("/cart");
        private void NavigateToCheckout() => NavManager.NavigateTo("/checkout");
    }
}
