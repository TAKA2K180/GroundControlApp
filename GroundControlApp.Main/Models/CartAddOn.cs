namespace GroundControlApp.Main.Models;

public sealed class CartAddOn
{
    public CartAddOn(Guid? addOnId, string name, decimal price)
    {
        AddOnId = addOnId;
        Name = name;
        Price = price;
    }

    public Guid? AddOnId { get; }

    public string Name { get; }

    public decimal Price { get; }

    public string PriceDisplay => $"PHP {Price:N2}";
}
