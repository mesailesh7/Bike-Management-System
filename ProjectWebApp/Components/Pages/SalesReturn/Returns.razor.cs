using Microsoft.AspNetCore.Components;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectWebApp.Components.Pages.SalesReturn;

public partial class Returns : ComponentBase
{
    // Services (inject only here)
    [Inject] private ReturnService ReturnService { get; set; } = default!;
    [Inject] private ReturnTransactionService TxService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    // UI state
    private string? InvoiceNumberText { get; set; }
    private SaleReturnViewModel? Model { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private string Feedback { get; set; } = string.Empty;

    private bool IsProcessing { get; set; } = false;
    
    private bool Processed { get; set; } = false;

    private bool IsWorking = false;

    private bool IsProcessed = false;

    // Button enable rule
    private bool CanProcess =>
        Model is not null &&
        !Processed &&
        !IsProcessing &&
        Model.Items.Any(i => i.QtyToReturn > 0 && !string.IsNullOrWhiteSpace(i.Reason));

    private async Task Search()
    {
        ErrorMessage = Feedback = string.Empty;
        Processed = false;

        if (!int.TryParse(InvoiceNumberText, out var saleId) || saleId <= 0)
        {
            ErrorMessage = "Please enter a valid sale invoice #.";
            StateHasChanged();
            return;
        }

        Model = await ReturnService.GetSaleReturnAsync(saleId);

        if (Model is null)
        {
            ErrorMessage = $"No sale found for invoice #{saleId}.";
        }

        StateHasChanged();
    }

    private void ClearSearch()
    {
        InvoiceNumberText = string.Empty;
        ErrorMessage = Feedback = string.Empty;
        Model = null;
        Processed = false;
    }

    private async Task ProcessReturn()
    {
        if (Model is null || !CanProcess) return;

        ErrorMessage = Feedback = string.Empty;
        IsProcessing = true;
        StateHasChanged();

        try
        {
            var req = new ReturnRequestViewModel
            {
                SaleId = Model.SaleId,
                Items = Model.Items
                    .Where(i => i.QtyToReturn > 0)
                    .Select(i => new ReturnRequestLineViewModel
                    {
                        PartId = i.PartId,
                        Qty = i.QtyToReturn,
                        Price = i.Price,
                        Reason = i.Reason
                    })
                    .ToList()
            };

            var refundId = await TxService.ProcessReturnAsync(req);

            Processed = true;                 // disables the button
            Feedback = $"Return processed successfully. Refund ID: {refundId}.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }

    private void Cancel()
    {

        ErrorMessage = Feedback = string.Empty;
        Model = null;
        Processed = false;
    }
}
