namespace GroundControlApp.Main.Models;

public sealed class PosProduct
{
    public PosProduct(Guid id, string name, int categoryId, string category, decimal price, int? stockOnHand)
    {
        Id = id;
        Name = name;
        CategoryId = categoryId;
        Category = category;
        Price = price;
        StockOnHand = stockOnHand;
    }

    public Guid Id { get; }

    public string Name { get; }

    public int CategoryId { get; }

    public string Category { get; }

    public decimal Price { get; }

    public int? StockOnHand { get; }

    public string DisplayPrice => Price < 0 ? $"{Price:P0}" : $"PHP {Price:N2}";

    public string StockDisplay => StockOnHand is int stock
        ? $"{stock:N0} in stock"
        : "Stock not tracked";
}
