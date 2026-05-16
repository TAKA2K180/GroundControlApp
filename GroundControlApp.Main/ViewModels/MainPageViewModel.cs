using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GroundControlApp.Data.Services;
using GroundControlApp.Main.Models;

namespace GroundControlApp.Main.ViewModels;

public sealed class MainPageViewModel : ObservableObject
{
    private const string TestAccountEmail = "admin@groundcontrol.local";
    private const string TestAccountSecret = "1234";

    private readonly IGroundControlApiClient apiClient;
    private readonly List<PosProduct> allProducts = [];
    private string searchText = string.Empty;
    private string selectedTender = "Card";
    private string shiftStatus = "Point of Sale";
    private string syncStatus = "Ready for API-backed inventory.";
    private string ticketStatus = "New ticket";
    private string selectedCategory = "All";
    private string signInEmail = TestAccountEmail;
    private string signInSecret = string.Empty;
    private string signInMessage = string.Empty;
    private bool isSignInVisible;
    private bool isSignedIn;

    public MainPageViewModel(IGroundControlApiClient apiClient)
    {
        this.apiClient = apiClient;
        Categories = [];

        Products = [];

        CartItems = [];

        KeypadValues = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "CLR", "0", "."];

        SelectCategoryCommand = new RelayCommand(parameter =>
        {
            if (parameter is PosCategory category)
            {
                SelectCategory(category.Name);
            }
        });
        AddProductCommand = new RelayCommand(parameter =>
        {
            if (parameter is PosProduct product)
            {
                AddProduct(product);
            }
        });
        SelectTenderCommand = new RelayCommand(parameter =>
        {
            if (parameter is string tender)
            {
                SelectedTender = tender;
            }
        });
        KeypadCommand = new RelayCommand(_ => { });
        IncreaseQuantityCommand = new RelayCommand(parameter =>
        {
            if (parameter is CartItem item)
            {
                item.Quantity++;
                RefreshTotals();
            }
        });
        DecreaseQuantityCommand = new RelayCommand(parameter =>
        {
            if (parameter is CartItem item)
            {
                if (item.Quantity <= 1)
                {
                    RemoveCartItem(item);
                    return;
                }

                item.Quantity--;
                RefreshTotals();
            }
        });
        RemoveCartItemCommand = new RelayCommand(parameter =>
        {
            if (parameter is CartItem item)
            {
                RemoveCartItem(item);
            }
        });
        AddCartItemAddOnCommand = new RelayCommand(parameter =>
        {
            if (parameter is CartItem item)
            {
                AddCartItemAddOn(item);
            }
        });
        RemoveCartItemAddOnCommand = new RelayCommand(parameter =>
        {
            if (parameter is CartAddOn addOn)
            {
                RemoveCartItemAddOn(addOn);
            }
        });
        ShowSignInCommand = new RelayCommand(() => IsSignInVisible = true);
        HideSignInCommand = new RelayCommand(HideSignIn);
        SignInCommand = new RelayCommand(SignIn);
        OpenAdminCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(nameof(Views.AdminPanelPage)), () => IsSignedIn);
        OpenShiftCommand = new RelayCommand(() => ShiftStatus = "Shift open");
        SyncCommand = new RelayCommand(() => SyncStatus = "Synced");
        HoldTicketCommand = new RelayCommand(() => TicketStatus = "On hold");
        ChargeCommand = new RelayCommand(() => TicketStatus = "Ready to charge");
        DiscountCommand = new RelayCommand(() => TicketStatus = "Discount pending");
        VoidCommand = new RelayCommand(() => TicketStatus = "Void pending");
    }

    public ObservableCollection<PosCategory> Categories { get; }

    public ObservableCollection<PosProduct> Products { get; }

    public ObservableCollection<CartItem> CartItems { get; }

    public ObservableCollection<string> KeypadValues { get; }

    public ICommand SelectCategoryCommand { get; }

    public ICommand AddProductCommand { get; }

    public ICommand SelectTenderCommand { get; }

    public ICommand KeypadCommand { get; }

    public ICommand IncreaseQuantityCommand { get; }

    public ICommand DecreaseQuantityCommand { get; }

    public ICommand RemoveCartItemCommand { get; }

    public ICommand AddCartItemAddOnCommand { get; }

    public ICommand RemoveCartItemAddOnCommand { get; }

    public ICommand ShowSignInCommand { get; }

    public ICommand HideSignInCommand { get; }

    public ICommand SignInCommand { get; }

    public ICommand OpenAdminCommand { get; }

    public ICommand OpenShiftCommand { get; }

    public ICommand SyncCommand { get; }

    public ICommand HoldTicketCommand { get; }

    public ICommand ChargeCommand { get; }

    public ICommand DiscountCommand { get; }

    public ICommand VoidCommand { get; }

    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    public string SelectedTender
    {
        get => selectedTender;
        set => SetProperty(ref selectedTender, value);
    }

    public string SignInEmail
    {
        get => signInEmail;
        set => SetProperty(ref signInEmail, value);
    }

    public string SignInSecret
    {
        get => signInSecret;
        set => SetProperty(ref signInSecret, value);
    }

    public string SignInMessage
    {
        get => signInMessage;
        private set => SetProperty(ref signInMessage, value);
    }

    public bool IsSignInVisible
    {
        get => isSignInVisible;
        private set => SetProperty(ref isSignInVisible, value);
    }

    public bool IsSignedIn
    {
        get => isSignedIn;
        private set
        {
            if (SetProperty(ref isSignedIn, value))
            {
                OnPropertyChanged(nameof(IsGuestVisible));
                OnPropertyChanged(nameof(IsAdminVisible));
                OnPropertyChanged(nameof(SignInButtonText));
                if (OpenAdminCommand is AsyncRelayCommand openAdminCommand)
                {
                    openAdminCommand.RaiseCanExecuteChanged();
                }
            }
        }
    }

    public bool IsGuestVisible => !IsSignedIn;

    public bool IsAdminVisible => IsSignedIn;

    public string SignInButtonText => IsSignedIn ? "Signed In" : "Sign In";

    public string ShiftStatus
    {
        get => shiftStatus;
        private set => SetProperty(ref shiftStatus, value);
    }

    public string SyncStatus
    {
        get => syncStatus;
        private set => SetProperty(ref syncStatus, value);
    }

    public string TicketStatus
    {
        get => ticketStatus;
        private set => SetProperty(ref ticketStatus, value);
    }

    public decimal Subtotal => CartItems.Sum(CalculateCartItemTotal);

    public decimal Total => Subtotal;

    public string SubtotalDisplay => $"PHP {Subtotal:N2}";

    public string TotalDisplay => $"PHP {Total:N2}";

    public string ChargeText => $"Charge {TotalDisplay}";

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            SyncStatus = "Loading menu catalog from API.";
            var menus = await apiClient.GetMenusAsync(cancellationToken);
            Products.Clear();
            Categories.Clear();
            allProducts.Clear();

            var availableMenus = menus
                .Where(menu => menu.IsAvailable)
                .ToArray();

            if (availableMenus.Length > 0)
            {
                if (selectedCategory != "All" &&
                    availableMenus.All(menu => GetMenuCategoryName(menu.Category) != selectedCategory))
                {
                    selectedCategory = "All";
                }

                Categories.Add(new PosCategory("All", selectedCategory == "All"));
            }

            foreach (var categoryName in availableMenus
                .Select(menu => GetMenuCategoryName(menu.Category))
                .Distinct()
                .OrderBy(category => category))
            {
                Categories.Add(new PosCategory(categoryName, categoryName == selectedCategory));
            }

            foreach (var menu in availableMenus)
            {
                allProducts.Add(new PosProduct(menu.Id, menu.Name, GetMenuCategoryName(menu.Category), menu.Price));
            }

            ApplyCategoryFilter();

            SyncStatus = Products.Count == 0
                ? "API returned no available menu items."
                : $"Loaded {Products.Count} menu items from API.";
        }
        catch (Exception ex)
        {
            SyncStatus = $"API unavailable: {ex.Message}";
        }
    }

    private void AddProduct(PosProduct product)
    {
        var existingItem = CartItems.FirstOrDefault(item => item.Name == product.Name && item.UnitPrice == product.Price);
        if (existingItem is not null)
        {
            existingItem.Quantity++;
        }
        else
        {
            var item = new CartItem(product.Name, string.Empty, 1, Math.Max(product.Price, 0));
            item.PropertyChanged += OnCartItemPropertyChanged;
            item.TotalChanged += OnCartItemTotalChanged;
            CartItems.Add(item);
        }

        TicketStatus = $"{CartItems.Sum(item => item.Quantity)} item(s) in ticket";
        RefreshTotals();
    }

    private void SelectCategory(string categoryName)
    {
        selectedCategory = categoryName;
        Categories.Clear();

        var categoryNames = allProducts
            .Select(product => product.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToArray();

        if (categoryNames.Length > 0)
        {
            Categories.Add(new PosCategory("All", selectedCategory == "All"));
        }

        foreach (var category in categoryNames)
        {
            Categories.Add(new PosCategory(category, category == selectedCategory));
        }

        ApplyCategoryFilter();
    }

    private void ApplyCategoryFilter()
    {
        Products.Clear();

        var filteredProducts = selectedCategory == "All"
            ? allProducts
            : allProducts.Where(product => product.Category == selectedCategory);

        foreach (var product in filteredProducts)
        {
            Products.Add(product);
        }
    }

    private void RemoveCartItem(CartItem item)
    {
        item.PropertyChanged -= OnCartItemPropertyChanged;
        item.TotalChanged -= OnCartItemTotalChanged;
        CartItems.Remove(item);
        TicketStatus = CartItems.Count == 0
            ? "New ticket"
            : $"{CartItems.Sum(cartItem => cartItem.Quantity)} item(s) in ticket";
        RefreshTotals();
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(SubtotalDisplay));
        OnPropertyChanged(nameof(TotalDisplay));
        OnPropertyChanged(nameof(ChargeText));
    }

    private void OnCartItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CartItem.Total) or nameof(CartItem.Quantity) or nameof(CartItem.AddOnTotal))
        {
            RefreshTotals();
        }
    }

    private void OnCartItemTotalChanged(object? sender, EventArgs e)
    {
        RefreshTotals();
    }

    private void AddCartItemAddOn(CartItem item)
    {
        if (string.IsNullOrWhiteSpace(item.AddOnNameInput))
        {
            return;
        }

        if (!decimal.TryParse(item.AddOnPriceInput, out var addOnPrice) || addOnPrice < 0)
        {
            return;
        }

        item.AddAddOn(item.AddOnNameInput.Trim(), addOnPrice);
        RefreshCartItem(item);
        RefreshTotals();
    }

    private void RemoveCartItemAddOn(CartAddOn addOn)
    {
        var item = CartItems.FirstOrDefault(cartItem => cartItem.AddOns.Contains(addOn));
        if (item is null)
        {
            return;
        }

        item.RemoveAddOn(addOn);
        RefreshCartItem(item);
        RefreshTotals();
    }

    private void RefreshCartItem(CartItem item)
    {
        var index = CartItems.IndexOf(item);
        if (index >= 0)
        {
            CartItems[index] = item;
        }
    }

    private static decimal CalculateCartItemTotal(CartItem item)
    {
        return item.Quantity * (item.UnitPrice + item.AddOns.Sum(addOn => addOn.Price));
    }

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

    private void SignIn()
    {
        if (!string.Equals(SignInEmail.Trim(), TestAccountEmail, StringComparison.OrdinalIgnoreCase) ||
            SignInSecret != TestAccountSecret)
        {
            SignInMessage = $"Temporary account: {TestAccountEmail} / {TestAccountSecret}";
            return;
        }

        IsSignedIn = true;
        SignInMessage = string.Empty;
        IsSignInVisible = false;
        ShiftStatus = "Admin signed in";
    }

    private void HideSignIn()
    {
        SignInMessage = string.Empty;
        IsSignInVisible = false;
    }

}
