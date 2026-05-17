using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Models;

public sealed class PendingOrderItem : ObservableObject
{
    private string status = "Pending";
    private bool isFinishing;

    public PendingOrderItem(Guid id, string invoiceNumber, string customerName, string itemsSummary, decimal total)
    {
        Id = id;
        InvoiceNumber = invoiceNumber;
        CustomerName = customerName;
        ItemsSummary = itemsSummary;
        Total = total;
    }

    public Guid Id { get; }

    public string InvoiceNumber { get; }

    public string CustomerName { get; }

    public string ItemsSummary { get; }

    public decimal Total { get; }

    public string TotalDisplay => $"PHP {Total:N2}";

    public string Status
    {
        get => status;
        set => SetProperty(ref status, value);
    }

    public bool IsFinishing
    {
        get => isFinishing;
        set => SetProperty(ref isFinishing, value);
    }
}
