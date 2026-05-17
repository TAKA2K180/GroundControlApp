using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Main.Models;

public sealed class MenuManagementListItem
{
    public MenuManagementListItem(MenuDto menu)
    {
        Menu = menu;
    }

    public MenuDto Menu { get; }

    public Guid Id => Menu.Id;

    public string Name => Menu.Name;

    public string Description => string.IsNullOrWhiteSpace(Menu.Description) ? "No description" : Menu.Description;

    public string CategoryDisplay => GetMenuCategoryName(Menu.Category);

    public string Sku => Menu.Sku;

    public string PriceDisplay => $"PHP {Menu.Price:N2}";

    public string AvailabilityDisplay => Menu.IsAvailable ? "Available" : "Unavailable";

    public string RecipeDisplay => Menu.Ingredients is null || Menu.Ingredients.Count == 0
        ? "No recipe"
        : $"{Menu.Ingredients.Count} ingredient(s)";

    private static string GetMenuCategoryName(int category)
    {
        return category switch
        {
            1 => "Coffee",
            2 => "Non-Coffee",
            3 => "Tea",
            4 => "Pastry",
            5 => "Food",
            6 => "Add-On",
            7 => "Merchandise",
            _ => "Menu"
        };
    }
}
