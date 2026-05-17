using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Models;

public sealed class MenuIngredientEditorItem : ObservableObject
{
    private bool isIncluded;
    private string quantityText = string.Empty;

    public MenuIngredientEditorItem(Guid ingredientId, string name, string unitOfMeasure)
    {
        IngredientId = ingredientId;
        Name = name;
        UnitOfMeasure = unitOfMeasure;
    }

    public Guid IngredientId { get; }

    public string Name { get; }

    public string UnitOfMeasure { get; }

    public bool IsIncluded
    {
        get => isIncluded;
        set => SetProperty(ref isIncluded, value);
    }

    public string QuantityText
    {
        get => quantityText;
        set => SetProperty(ref quantityText, value);
    }
}
