namespace GroundControlApp.Main.Models;

public sealed class CartItem
{
    public CartItem(string name, string note, int quantity, decimal unitPrice)
    {
        Name = name;
        Note = note;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string Name { get; }

    public string Note { get; }

    public int Quantity { get; }

    public decimal UnitPrice { get; }

    public decimal Total => Quantity * UnitPrice;

    public string TotalDisplay => $"PHP {Total:N2}";

    public string Detail => $"Qty {Quantity}{(string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}")}";
}
