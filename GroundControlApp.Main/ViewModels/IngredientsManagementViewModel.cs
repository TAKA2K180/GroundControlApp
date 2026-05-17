using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.Models;

namespace GroundControlApp.Main.ViewModels;

public sealed class IngredientsManagementViewModel : ObservableObject
{
    private readonly IIngredientService ingredientService;
    private readonly List<IngredientDto> ingredients = [];
    private Guid? editingIngredientId;
    private string pageStatus = "Loading ingredients...";
    private string editorTitle = "Add ingredient";
    private string name = string.Empty;
    private string unitOfMeasure = string.Empty;
    private string currentQuantityText = string.Empty;
    private string reorderLevelText = string.Empty;
    private string targetLevelText = string.Empty;
    private string unitCostText = string.Empty;
    private bool isActive = true;

    public IngredientsManagementViewModel(IIngredientService ingredientService)
    {
        this.ingredientService = ingredientService;
        IngredientItems = [];

        BackCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(".."));
        AddIngredientCommand = new RelayCommand(StartAddIngredient);
        SetReorderCommand = new RelayCommand(() =>
        {
            PageStatus = editingIngredientId is null
                ? "Select an ingredient before setting reorder levels."
                : "Update reorder and target levels, then save.";
        });
        ReviewCostCommand = new RelayCommand(() =>
        {
            PageStatus = editingIngredientId is null
                ? "Select an ingredient before reviewing cost."
                : "Review unit cost, then save.";
        });
        EditIngredientCommand = new RelayCommand(parameter =>
        {
            if (parameter is IngredientManagementListItem item)
            {
                LoadEditor(item.Ingredient);
            }
        });
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        RefreshCommand = new AsyncRelayCommand(() => LoadAsync());
    }

    public ObservableCollection<IngredientManagementListItem> IngredientItems { get; }

    public ICommand BackCommand { get; }

    public ICommand AddIngredientCommand { get; }

    public ICommand SetReorderCommand { get; }

    public ICommand ReviewCostCommand { get; }

    public ICommand EditIngredientCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand RefreshCommand { get; }

    public string PageStatus
    {
        get => pageStatus;
        private set => SetProperty(ref pageStatus, value);
    }

    public string EditorTitle
    {
        get => editorTitle;
        private set => SetProperty(ref editorTitle, value);
    }

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public string UnitOfMeasure
    {
        get => unitOfMeasure;
        set => SetProperty(ref unitOfMeasure, value);
    }

    public string CurrentQuantityText
    {
        get => currentQuantityText;
        set => SetProperty(ref currentQuantityText, value);
    }

    public string ReorderLevelText
    {
        get => reorderLevelText;
        set => SetProperty(ref reorderLevelText, value);
    }

    public string TargetLevelText
    {
        get => targetLevelText;
        set => SetProperty(ref targetLevelText, value);
    }

    public string UnitCostText
    {
        get => unitCostText;
        set => SetProperty(ref unitCostText, value);
    }

    public bool IsActive
    {
        get => isActive;
        set => SetProperty(ref isActive, value);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            PageStatus = "Refreshing ingredients...";
            var loadedIngredients = await ingredientService.GetIngredientsAsync(cancellationToken);
            ingredients.Clear();
            ingredients.AddRange(loadedIngredients);

            IngredientItems.Clear();
            foreach (var ingredient in ingredients.OrderBy(ingredient => ingredient.Name))
            {
                IngredientItems.Add(new IngredientManagementListItem(ingredient));
            }

            if (editingIngredientId is Guid id && ingredients.FirstOrDefault(ingredient => ingredient.Id == id) is { } activeIngredient)
            {
                LoadEditor(activeIngredient);
            }
            else
            {
                StartAddIngredient();
            }

            PageStatus = $"{ingredients.Count} ingredients loaded.";
        }
        catch (Exception ex)
        {
            PageStatus = $"API unavailable: {ex.Message}";
        }
    }

    private void StartAddIngredient()
    {
        editingIngredientId = null;
        EditorTitle = "Add ingredient";
        Name = string.Empty;
        UnitOfMeasure = string.Empty;
        CurrentQuantityText = "0";
        ReorderLevelText = "0";
        TargetLevelText = "0";
        UnitCostText = "0";
        IsActive = true;
    }

    private void LoadEditor(IngredientDto ingredient)
    {
        editingIngredientId = ingredient.Id;
        EditorTitle = $"Edit {ingredient.Name}";
        Name = ingredient.Name;
        UnitOfMeasure = ingredient.UnitOfMeasure;
        CurrentQuantityText = ingredient.CurrentQuantity.ToString("0.####", CultureInfo.InvariantCulture);
        ReorderLevelText = ingredient.ReorderLevel.ToString("0.####", CultureInfo.InvariantCulture);
        TargetLevelText = ingredient.TargetLevel.ToString("0.####", CultureInfo.InvariantCulture);
        UnitCostText = ingredient.UnitCost.ToString("0.##", CultureInfo.InvariantCulture);
        IsActive = ingredient.IsActive;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(UnitOfMeasure))
        {
            PageStatus = "Name and unit are required.";
            return;
        }

        if (!TryReadDecimal(CurrentQuantityText, out var currentQuantity) ||
            !TryReadDecimal(ReorderLevelText, out var reorderLevel) ||
            !TryReadDecimal(TargetLevelText, out var targetLevel) ||
            !TryReadDecimal(UnitCostText, out var unitCost))
        {
            PageStatus = "Quantities, reorder, target, and cost must be valid numbers.";
            return;
        }

        if (currentQuantity < 0 || reorderLevel < 0 || targetLevel < 0 || unitCost < 0)
        {
            PageStatus = "Values cannot be negative.";
            return;
        }

        var request = new SaveIngredientDto(
            Name.Trim(),
            UnitOfMeasure.Trim(),
            reorderLevel,
            targetLevel,
            currentQuantity,
            unitCost,
            IsActive);

        try
        {
            PageStatus = editingIngredientId is null ? "Creating ingredient..." : "Saving ingredient...";
            var savedIngredient = editingIngredientId is Guid id
                ? await ingredientService.UpdateIngredientAsync(id, request)
                : await ingredientService.CreateIngredientAsync(request);

            editingIngredientId = savedIngredient.Id;
            await LoadAsync();
            PageStatus = $"{savedIngredient.Name} saved.";
        }
        catch (Exception ex)
        {
            PageStatus = $"Save failed: {ex.Message}";
        }
    }

    private static bool TryReadDecimal(string text, out decimal value)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
