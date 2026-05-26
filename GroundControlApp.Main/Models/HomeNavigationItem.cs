namespace GroundControlApp.Main.Models;

public sealed record HomeNavigationItem(
    string Title,
    string Description,
    string Status,
    string Metric,
    string Route,
    AppRole RequiredRole,
    string AccentColor,
    string Icon);
