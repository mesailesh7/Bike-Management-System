using Microsoft.AspNetCore.Components;
using ProjectWebApp.Services;
using SalesReturnsSystem.ViewModels;

namespace ProjectWebApp.Components.Pages.SalesReturn;

public partial class Checkout
{

    private string CouponCode { get; set; } = string.Empty;

    private CouponViewModel? _verifiedCoupon;

    private string _paymentType = string.Empty;
    private bool SaleCompleted { get; set; }
    private int SaleId { get; set; }
    private bool IsWorking { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private string Feedback { get; set; } = string.Empty;

    // computed totals
    private decimal Subtotal => SalesState.CartItems.Sum(x => x.ExtPrice);
    private decimal Discount => _verifiedCoupon is null ? 0 : Math.Round(Subtotal * (_verifiedCoupon.CouponDiscount / 100m), 2);
    private decimal Tax => Math.Round((Subtotal - Discount) * 0.05m, 2);
    private decimal Total => Subtotal - Discount + Tax;
    private string DiscountPercentDisplay => _verifiedCoupon is null ? "0%" : $"{_verifiedCoupon.CouponDiscount:0}%";

    // services
    [Inject] private SalesStateService SalesState { get; set; } = default!;
    [Inject] private SalesReturnsSystem.BLL.SalesService SalesService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
    }


    private void SetCouponCode(string code)
    {
        CouponCode = code;
    }

    private async Task VerifyCoupon()
    {
        ErrorMessage = Feedback = string.Empty;

        if (string.IsNullOrWhiteSpace(CouponCode))
        {
            _verifiedCoupon = null;
            return;
        }

        _verifiedCoupon = await SalesService.ValidateCouponAsync(CouponCode);
        SalesState.ApplyCoupon(_verifiedCoupon);
    }

    private void ClearCoupon()
    {
        ErrorMessage = Feedback = string.Empty;
        CouponCode = string.Empty;
        _verifiedCoupon = null;
        SalesState.ClearCoupon();
    }

    private string PaymentType
    {
        get => _paymentType;
        set
        {
            _paymentType = value;

            ErrorMessage = string.Empty;
            Feedback = string.Empty;
            StateHasChanged();
        }
    }

    private async Task PlaceOrder()
    {
        if (IsWorking || SaleCompleted) return;

        ErrorMessage = Feedback = string.Empty;
        IsWorking = true;
        try
        {
            if (!SalesState.CartItems.Any())
                throw new InvalidOperationException("Your cart is empty.");
            if (!string.IsNullOrWhiteSpace(CouponCode) && _verifiedCoupon is null)
                throw new InvalidOperationException("Verify the coupon before placing the order.");


            await Task.Delay(250);
            SaleId = new Random().Next(10_000, 99_999);

            SaleCompleted = true;
            SalesState.ClearCart();
            Feedback = $"Order placed successfully. Sale ID: {SaleId}";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsWorking = false;
        }
    }

    private void ResetCheckout()
    {

        CouponCode = string.Empty;
        _verifiedCoupon = null;


        SalesState.ClearCoupon();
        SalesState.ClearCart();


        PaymentType = string.Empty;
        SaleCompleted = false;
        SaleId = 0;


        ErrorMessage = string.Empty;
        Feedback = string.Empty;
    }

    private void CancelOrder()
    {
        ResetCheckout();
        Navigation.NavigateTo("/customer-search", forceLoad: false);
    }

    private void ClearOrder()
    {
        ResetCheckout();
        Navigation.NavigateTo("/customer-search", forceLoad: false);
    }
}
