namespace GroundControlApp.Main.Models;

public sealed class LowStockAlertItem
{
    public LowStockAlertItem(string name, decimal currentQuantity, decimal reorderLevel, string unitOfMeasure)
    {
        Name = name;
        CurrentQuantity = currentQuantity;
        ReorderLevel = reorderLevel;
        UnitOfMeasure = unitOfMeasure;
    }

    public string Name { get; }

    public decimal CurrentQuantity { get; }

    public decimal ReorderLevel { get; }

    public string UnitOfMeasure { get; }

    public string QuantityDisplay => $"{CurrentQuantity:N2} {UnitOfMeasure}";

    public string ReorderDisplay => $"Reorder at {ReorderLevel:N2} {UnitOfMeasure}";
}
