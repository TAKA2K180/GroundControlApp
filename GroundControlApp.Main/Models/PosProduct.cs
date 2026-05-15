namespace GroundControlApp.Main.Models;

public sealed class PosProduct
{
    public PosProduct(Guid id, string name, string category, decimal price)
    {
        Id = id;
        Name = name;
        Category = category;
        Price = price;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Category { get; }

    public decimal Price { get; }

    public string DisplayPrice => Price < 0 ? $"{Price:P0}" : $"PHP {Price:N2}";
}
