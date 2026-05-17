using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.Models;

namespace GroundControlApp.Main.ViewModels;

public sealed class MenuManagementViewModel : ObservableObject
{
    private readonly IMenuService menuService;
    private readonly IIngredientService ingredientService;
    private readonly List<MenuDto> menus = [];
    private Guid? editingMenuId;
    private string pageStatus = "Loading menu data...";
    private string editorTitle = "Add menu item";
    private string name = string.Empty;
    private string description = string.Empty;
    private string sku = string.Empty;
    private string priceText = string.Empty;
    private bool isAvailable = true;
    private bool isLoadingEditor;
    private MenuCategoryOption? selectedCategory;

    public MenuManagementViewModel(IMenuService menuService, IIngredientService ingredientService)
    {
        this.menuService = menuService;
        this.ingredientService = ingredientService;

        MenuItems = [];
        Ingredients = [];
        Categories =
        [
            new MenuCategoryOption(1, "Coffee"),
            new MenuCategoryOption(2, "Non-Coffee"),
            new MenuCategoryOption(3, "Tea"),
            new MenuCategoryOption(4, "Pastry"),
            new MenuCategoryOption(5, "Food"),
            new MenuCategoryOption(6, "Add-On"),
            new MenuCategoryOption(7, "Merchandise")
        ];
        selectedCategory = Categories[0];

        BackCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(".."));
        AddMenuCommand = new RelayCommand(StartAddMenu);
        UpdateRecipeCommand = new RelayCommand(StartRecipeUpdate);
        SetAvailabilityCommand = new AsyncRelayCommand(SetAvailabilityAsync);
        EditMenuCommand = new RelayCommand(parameter =>
        {
            if (parameter is MenuManagementListItem item)
            {
                LoadEditor(item.Menu);
            }
        });
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        RefreshCommand = new AsyncRelayCommand(() => LoadAsync());
    }

    public ObservableCollection<MenuManagementListItem> MenuItems { get; }

    public ObservableCollection<MenuIngredientEditorItem> Ingredients { get; }

    public ObservableCollection<MenuCategoryOption> Categories { get; }

    public ICommand BackCommand { get; }

    public ICommand AddMenuCommand { get; }

    public ICommand UpdateRecipeCommand { get; }

    public ICommand SetAvailabilityCommand { get; }

    public ICommand EditMenuCommand { get; }

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
        set
        {
            if (SetProperty(ref name, value))
            {
                UpdateGeneratedSku();
            }
        }
    }

    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    public string Sku
    {
        get => sku;
        set => SetProperty(ref sku, value);
    }

    public string PriceText
    {
        get => priceText;
        set => SetProperty(ref priceText, value);
    }

    public bool IsAvailable
    {
        get => isAvailable;
        set => SetProperty(ref isAvailable, value);
    }

    public MenuCategoryOption? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (SetProperty(ref selectedCategory, value))
            {
                UpdateGeneratedSku();
            }
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            PageStatus = "Refreshing menu management...";
            var menuTask = menuService.GetMenusAsync(cancellationToken);
            var ingredientTask = ingredientService.GetIngredientsAsync(cancellationToken);
            await Task.WhenAll(menuTask, ingredientTask);

            menus.Clear();
            menus.AddRange(await menuTask);
            MenuItems.Clear();
            foreach (var menu in menus.OrderBy(menu => menu.Name))
            {
                MenuItems.Add(new MenuManagementListItem(menu));
            }

            Ingredients.Clear();
            foreach (var ingredient in (await ingredientTask).Where(ingredient => ingredient.IsActive).OrderBy(ingredient => ingredient.Name))
            {
                Ingredients.Add(new MenuIngredientEditorItem(ingredient.Id, ingredient.Name, ingredient.UnitOfMeasure));
            }

            if (editingMenuId is Guid id && menus.FirstOrDefault(menu => menu.Id == id) is { } activeMenu)
            {
                LoadEditor(activeMenu);
            }
            else
            {
                StartAddMenu();
            }

            PageStatus = $"{menus.Count} menu items loaded.";
        }
        catch (Exception ex)
        {
            PageStatus = $"API unavailable: {ex.Message}";
        }
    }

    private void StartAddMenu()
    {
        editingMenuId = null;
        EditorTitle = "Add menu item";
        Name = string.Empty;
        Description = string.Empty;
        PriceText = string.Empty;
        IsAvailable = true;
        SelectedCategory = Categories.FirstOrDefault();
        UpdateGeneratedSku();

        foreach (var ingredient in Ingredients)
        {
            ingredient.IsIncluded = false;
            ingredient.QuantityText = string.Empty;
        }
    }

    private void StartRecipeUpdate()
    {
        if (editingMenuId is null && menus.Count > 0)
        {
            LoadEditor(menus.OrderBy(menu => menu.Name).First());
        }

        PageStatus = editingMenuId is null
            ? "Select a menu item before updating a recipe."
            : "Edit recipe quantities, then save.";
    }

    private async Task SetAvailabilityAsync(object? _)
    {
        if (editingMenuId is null)
        {
            PageStatus = "Select a menu item before setting availability.";
            return;
        }

        IsAvailable = !IsAvailable;
        await SaveAsync();
    }

    private void LoadEditor(MenuDto menu)
    {
        isLoadingEditor = true;
        editingMenuId = menu.Id;
        EditorTitle = $"Edit {menu.Name}";
        Name = menu.Name;
        Description = menu.Description ?? string.Empty;
        Sku = menu.Sku;
        PriceText = menu.Price.ToString("0.##", CultureInfo.InvariantCulture);
        IsAvailable = menu.IsAvailable;
        SelectedCategory = Categories.FirstOrDefault(category => category.Value == menu.Category) ?? Categories.FirstOrDefault();
        isLoadingEditor = false;

        var recipe = (menu.Ingredients ?? [])
            .ToDictionary(ingredient => ingredient.IngredientId, ingredient => ingredient.Quantity);

        foreach (var ingredient in Ingredients)
        {
            if (recipe.TryGetValue(ingredient.IngredientId, out var quantity))
            {
                ingredient.IsIncluded = true;
                ingredient.QuantityText = quantity.ToString("0.####", CultureInfo.InvariantCulture);
            }
            else
            {
                ingredient.IsIncluded = false;
                ingredient.QuantityText = string.Empty;
            }
        }
    }

    private void UpdateGeneratedSku()
    {
        if (isLoadingEditor)
        {
            return;
        }

        var categoryPart = Slugify(SelectedCategory?.Name ?? "Menu");
        var namePart = Slugify(Name);

        if (string.IsNullOrWhiteSpace(namePart))
        {
            Sku = categoryPart;
            return;
        }

        var baseSku = $"{categoryPart}-{namePart}";
        var candidate = baseSku.Length > 56 ? baseSku[..56].TrimEnd('-') : baseSku;
        var suffix = 2;

        while (menus.Any(menu =>
            menu.Id != editingMenuId &&
            string.Equals(menu.Sku, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            var suffixText = $"-{suffix++}";
            var maxBaseLength = Math.Min(baseSku.Length, 64 - suffixText.Length);
            candidate = $"{baseSku[..maxBaseLength].TrimEnd('-')}{suffixText}";
        }

        Sku = candidate;
    }

    private static string Slugify(string value)
    {
        var chars = value
            .Trim()
            .ToUpperInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();

        return string.Join('-', new string(chars)
            .Split('-', StringSplitOptions.RemoveEmptyEntries))
            .Trim();
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(Sku) ||
            SelectedCategory is null)
        {
            PageStatus = "Name, SKU, and category are required.";
            return;
        }

        if (!decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price < 0)
        {
            PageStatus = "Enter a valid price.";
            return;
        }

        var recipe = new List<SaveMenuIngredientDto>();
        foreach (var ingredient in Ingredients.Where(ingredient => ingredient.IsIncluded))
        {
            if (!decimal.TryParse(ingredient.QuantityText, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity) ||
                quantity <= 0)
            {
                PageStatus = $"Enter a valid quantity for {ingredient.Name}.";
                return;
            }

            recipe.Add(new SaveMenuIngredientDto(ingredient.IngredientId, quantity));
        }

        var request = new SaveMenuDto(
            Name.Trim(),
            string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            SelectedCategory.Value,
            Sku.Trim(),
            price,
            IsAvailable,
            recipe);

        try
        {
            PageStatus = editingMenuId is null ? "Creating menu item..." : "Saving menu item...";
            var savedMenu = editingMenuId is Guid id
                ? await menuService.UpdateMenuAsync(id, request)
                : await menuService.CreateMenuAsync(request);

            editingMenuId = savedMenu.Id;
            await LoadAsync();
            PageStatus = $"{savedMenu.Name} saved.";
        }
        catch (Exception ex)
        {
            PageStatus = $"Save failed: {ex.Message}";
        }
    }
}
