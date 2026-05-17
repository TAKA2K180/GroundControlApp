using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Main.Models;

public sealed class IngredientManagementListItem
{
    public IngredientManagementListItem(IngredientDto ingredient)
    {
        Ingredient = ingredient;
    }

    public IngredientDto Ingredient { get; }

    public Guid Id => Ingredient.Id;

    public string Name => Ingredient.Name;

    public string UnitOfMeasure => Ingredient.UnitOfMeasure;

    public string QuantityDisplay => $"{Ingredient.CurrentQuantity:N2} {Ingredient.UnitOfMeasure}";

    public string ReorderDisplay => $"Reorder {Ingredient.ReorderLevel:N2}";

    public string TargetDisplay => $"Target {Ingredient.TargetLevel:N2}";

    public string UnitCostDisplay => $"PHP {Ingredient.UnitCost:N2}";

    public string StatusDisplay => !Ingredient.IsActive
        ? "Inactive"
        : Ingredient.CurrentQuantity <= Ingredient.ReorderLevel
            ? "Low stock"
            : "Active";

    public Color StatusColor => !Ingredient.IsActive
        ? Color.FromArgb("#667085")
        : Ingredient.CurrentQuantity <= Ingredient.ReorderLevel
            ? Color.FromArgb("#B54708")
            : Color.FromArgb("#087443");
}
