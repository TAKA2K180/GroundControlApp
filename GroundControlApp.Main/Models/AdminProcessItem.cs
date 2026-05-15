namespace GroundControlApp.Main.Models;

public sealed class AdminProcessItem
{
    public AdminProcessItem(string title, string description, string status, string metric, string route = "")
    {
        Title = title;
        Description = description;
        Status = status;
        Metric = metric;
        Route = route;
    }

    public string Title { get; }

    public string Description { get; }

    public string Status { get; }

    public string Metric { get; }

    public string Route { get; }
}
