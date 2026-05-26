namespace GroundControlApp.Main.Models;

public sealed class AddOnOption
{
    public AddOnOption(Guid id, string name, IReadOnlyCollection<Guid> menuIds, decimal price)
    {
        Id = id;
        Name = name;
        MenuIds = menuIds;
        Price = price;
    }

    public Guid Id { get; }

    public string Name { get; }

    public IReadOnlyCollection<Guid> MenuIds { get; }

    public decimal Price { get; }

    public string DisplayPrice => $"PHP {Price:N2}";
}
