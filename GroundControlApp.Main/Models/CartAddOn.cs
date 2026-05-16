namespace GroundControlApp.Main.Models;

public sealed class CartAddOn
{
    public CartAddOn(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public string Name { get; }

    public decimal Price { get; }

    public string PriceDisplay => $"PHP {Price:N2}";
}
