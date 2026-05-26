using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.Models;
using GroundControlApp.Main.Services;

namespace GroundControlApp.Main.ViewModels;

public sealed class MainPageViewModel : ObservableObject
{
    private const string TestAccountName = "Admin User";
    private const string TestAccountPin = "1234";
    private const int AddOnMenuCategory = 6;
    private readonly IMenuService menuService;
    private readonly IAddOnService addOnService;
    private readonly IIngredientService ingredientService;
    private readonly IOrderService orderService;
    private readonly AppIdentitySession identitySession;
    private readonly List<PosProduct> allProducts = [];
    private readonly List<AddOnOption> allAddOnOptions = [];
    private readonly Dictionary<Guid, IReadOnlyCollection<Guid>> menuIngredientIdsByMenuId = [];
    private string searchText = string.Empty;
    private string customerName = string.Empty;
    private string selectedTender = "Cash";
    private string shiftStatus = "Point of Sale";
    private string syncStatus = "Ready for API-backed inventory.";
    private string ticketStatus = "New ticket";
    private string selectedCategory = "All";
    private string signInName = TestAccountName;
    private string signInPin = string.Empty;
    private string signInMessage = string.Empty;
    private CartItem? selectedAddOnCartItem;
    private bool isSignInVisible;
    private bool isAddOnPickerVisible;
    private bool isLowStockNotificationVisible;

    public MainPageViewModel(
        IMenuService menuService,
        IAddOnService addOnService,
        IIngredientService ingredientService,
        IOrderService orderService,
        AppIdentitySession identitySession)
    {
        this.menuService = menuService;
        this.addOnService = addOnService;
        this.ingredientService = ingredientService;
        this.orderService = orderService;
        this.identitySession = identitySession;
        identitySession.PropertyChanged += OnIdentitySessionChanged;
        Categories = [];

        Products = [];

        CartItems = [];

        PendingOrders = [];

        AddOnOptions = [];

        LowStockItems = [];

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
        ShowCartItemAddOnPickerCommand = new RelayCommand(parameter =>
        {
            if (parameter is CartItem item)
            {
                ShowCartItemAddOnPicker(item);
            }
        });
        HideAddOnPickerCommand = new RelayCommand(HideAddOnPicker);
        DismissLowStockNotificationCommand = new RelayCommand(() => IsLowStockNotificationVisible = false);
        SelectCartItemAddOnCommand = new RelayCommand(parameter =>
        {
            if (parameter is AddOnOption addOn)
            {
                SelectCartItemAddOn(addOn);
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
        LogoutCommand = new RelayCommand(Logout);
        OpenHomeCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("//HomePage"));
        OpenAdminCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(nameof(Views.AdminPanelPage)), () => IsAdminVisible);
        ScanCatalogCommand = new AsyncRelayCommand(() => LoadAsync());
        OpenShiftCommand = new RelayCommand(() => ShiftStatus = "Shift open");
        SyncCommand = new RelayCommand(() => SyncStatus = "Synced");
        HoldTicketCommand = new RelayCommand(() => TicketStatus = "On hold");
        ChargeCommand = new AsyncRelayCommand(() => CreatePendingOrderAsync());
        FinishOrderCommand = new AsyncRelayCommand(FinishOrderAsync);
        DiscountCommand = new RelayCommand(() => TicketStatus = "Discount pending");
        VoidCommand = new RelayCommand(() => TicketStatus = "Void pending");
    }

    public ObservableCollection<PosCategory> Categories { get; }

    public ObservableCollection<PosProduct> Products { get; }

    public ObservableCollection<CartItem> CartItems { get; }

    public ObservableCollection<PendingOrderItem> PendingOrders { get; }

    public ObservableCollection<AddOnOption> AddOnOptions { get; }

    public ObservableCollection<LowStockAlertItem> LowStockItems { get; }

    public ICommand SelectCategoryCommand { get; }

    public ICommand AddProductCommand { get; }

    public ICommand SelectTenderCommand { get; }

    public ICommand IncreaseQuantityCommand { get; }

    public ICommand DecreaseQuantityCommand { get; }

    public ICommand RemoveCartItemCommand { get; }

    public ICommand ShowCartItemAddOnPickerCommand { get; }

    public ICommand HideAddOnPickerCommand { get; }

    public ICommand DismissLowStockNotificationCommand { get; }

    public ICommand SelectCartItemAddOnCommand { get; }

    public ICommand RemoveCartItemAddOnCommand { get; }

    public ICommand ShowSignInCommand { get; }

    public ICommand HideSignInCommand { get; }

    public ICommand SignInCommand { get; }

    public ICommand LogoutCommand { get; }

    public ICommand OpenHomeCommand { get; }

    public ICommand OpenAdminCommand { get; }

    public ICommand ScanCatalogCommand { get; }

    public ICommand OpenShiftCommand { get; }

    public ICommand SyncCommand { get; }

    public ICommand HoldTicketCommand { get; }

    public ICommand ChargeCommand { get; }

    public ICommand FinishOrderCommand { get; }

    public ICommand DiscountCommand { get; }

    public ICommand VoidCommand { get; }

    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    public string CustomerName
    {
        get => customerName;
        set => SetProperty(ref customerName, value);
    }

    public string SelectedTender
    {
        get => selectedTender;
        set => SetProperty(ref selectedTender, value);
    }

    public string SignInName
    {
        get => signInName;
        set => SetProperty(ref signInName, value);
    }

    public string SignInPin
    {
        get => signInPin;
        set => SetProperty(ref signInPin, value);
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

    public bool IsAddOnPickerVisible
    {
        get => isAddOnPickerVisible;
        private set => SetProperty(ref isAddOnPickerVisible, value);
    }

    public bool IsLowStockNotificationVisible
    {
        get => isLowStockNotificationVisible;
        private set => SetProperty(ref isLowStockNotificationVisible, value);
    }

    public string AddOnPickerTitle => selectedAddOnCartItem is null
        ? "Select add-on"
        : $"Select add-on for {selectedAddOnCartItem.Name}";

    public string LowStockNotificationTitle => LowStockItems.Count == 1
        ? "1 low-stock item"
        : $"{LowStockItems.Count} low-stock items";

    public bool IsSignedIn
    {
        get => identitySession.IsSignedIn;
    }

    public bool IsGuestVisible => !IsSignedIn;

    public bool IsAdminVisible => identitySession.CanAccess(AppRole.Manager);

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
            SyncStatus = "Loading menu catalog....";
            var menusTask = menuService.GetMenusAsync(cancellationToken);
            var addOnsTask = addOnService.GetAddOnsAsync(cancellationToken);
            var ingredientsTask = ingredientService.GetIngredientsAsync(cancellationToken);

            await Task.WhenAll(menusTask, addOnsTask, ingredientsTask);

            var menus = await menusTask;
            var addOns = await addOnsTask;
            var ingredients = await ingredientsTask;
            Products.Clear();
            Categories.Clear();
            allProducts.Clear();
            allAddOnOptions.Clear();
            menuIngredientIdsByMenuId.Clear();
            AddOnOptions.Clear();
            LowStockItems.Clear();

            var availableMenus = menus
                .Where(menu => menu.IsAvailable)
                .ToArray();
            var sellableMenus = availableMenus
                .Where(menu => menu.Category != AddOnMenuCategory)
                .ToArray();

            if (sellableMenus.Length > 0)
            {
                if (selectedCategory != "All" &&
                    sellableMenus.All(menu => GetMenuCategoryName(menu.Category) != selectedCategory))
                {
                    selectedCategory = "All";
                }

                Categories.Add(new PosCategory("All", selectedCategory == "All"));
            }

            foreach (var categoryName in sellableMenus
                .Select(menu => GetMenuCategoryName(menu.Category))
                .Distinct()
                .OrderBy(category => category))
            {
                Categories.Add(new PosCategory(categoryName, categoryName == selectedCategory));
            }

            foreach (var menu in sellableMenus)
            {
                allProducts.Add(new PosProduct(menu.Id, menu.Name, menu.Category, GetMenuCategoryName(menu.Category), menu.Price));
                menuIngredientIdsByMenuId[menu.Id] = (menu.Ingredients ?? [])
                    .Select(ingredient => ingredient.IngredientId)
                    .Distinct()
                    .ToArray();
            }

            foreach (var addOn in addOns
                .Where(addOn => addOn.IsAvailable)
                .OrderBy(addOn => addOn.Name))
            {
                allAddOnOptions.Add(new AddOnOption(
                    addOn.Id,
                    addOn.Name,
                    addOn.MenuIds,
                    addOn.Price));
            }

            foreach (var ingredient in ingredients
                .Where(ingredient => ingredient.IsActive && ingredient.CurrentQuantity <= ingredient.ReorderLevel)
                .OrderBy(ingredient => ingredient.Name))
            {
                LowStockItems.Add(new LowStockAlertItem(
                    ingredient.Name,
                    ingredient.CurrentQuantity,
                    ingredient.ReorderLevel,
                    ingredient.UnitOfMeasure));
            }

            OnPropertyChanged(nameof(LowStockNotificationTitle));
            IsLowStockNotificationVisible = LowStockItems.Count > 0;

            ApplyCategoryFilter();

            SyncStatus = Products.Count == 0
                ? "API returned no available menu items."
                : $"Loaded {Products.Count} menu items.";
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
            var item = new CartItem(product.Id, product.Name, product.CategoryId, product.Category, string.Empty, 1, Math.Max(product.Price, 0));
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

    private async Task CreatePendingOrderAsync(CancellationToken cancellationToken = default)
    {
        if (CartItems.Count == 0)
        {
            TicketStatus = "Add items before charging.";
            return;
        }

        try
        {
            TicketStatus = "Sending order to queue...";
            var order = await orderService.CreateOrderAsync(
                CreateOrderRequest(),
                $"pos-order-{Guid.NewGuid():N}",
                cancellationToken);

            PendingOrders.Add(new PendingOrderItem(
                order.Id,
                order.InvoiceNumber,
                GetOrderCustomerName(),
                CreateOrderSummary(),
                order.GrandTotal));

            ClearTicket();
            TicketStatus = $"Order {order.InvoiceNumber} pending.";
        }
        catch (Exception ex)
        {
            TicketStatus = $"Order failed: {ex.Message}";
        }
    }

    private async Task FinishOrderAsync(object? parameter)
    {
        if (parameter is not PendingOrderItem pendingOrder)
        {
            return;
        }

        try
        {
            pendingOrder.IsFinishing = true;
            pendingOrder.Status = "Finishing";
            var order = await orderService.PayOrderAsync(
                pendingOrder.Id,
                new PayOrderDto(pendingOrder.Total, GetCurrentUserId()),
                $"pos-payment-{pendingOrder.Id:N}",
                CancellationToken.None);

            await ShowLowStockNotificationForOrderAsync(order, CancellationToken.None);
            PendingOrders.Remove(pendingOrder);
            TicketStatus = $"Order {pendingOrder.InvoiceNumber} finished. Ingredients deducted.";
        }
        catch (Exception ex)
        {
            pendingOrder.Status = "Pending";
            TicketStatus = $"Finish failed: {ex.Message}";
        }
        finally
        {
            pendingOrder.IsFinishing = false;
        }
    }

    private CreateOrderDto CreateOrderRequest()
    {
        return new CreateOrderDto(
            InvoiceNumber: null,
            DueAtUtc: null,
            CustomerName: string.IsNullOrWhiteSpace(CustomerName) ? null : CustomerName.Trim(),
            CustomerEmail: null,
            CustomerPhone: null,
            Subtotal: Subtotal,
            DiscountTotal: 0,
            TaxTotal: 0,
            GrandTotal: Total,
            Notes: $"Tender: {SelectedTender}",
            UserId: GetCurrentUserId(),
            Details: CartItems.Select(item => new CreateOrderDetailDto(
                MenuId: item.MenuId,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                DiscountAmount: 0,
                TaxAmount: 0,
                LineTotal: CalculateCartItemTotal(item),
                Description: item.Name,
                Notes: item.Note,
                AddOns: item.AddOns.Select(addOn => new CreateOrderDetailAddOnDto(addOn.AddOnId, addOn.Name, addOn.Price)).ToList()))
                .ToList());
    }

    private string CreateOrderSummary()
    {
        return string.Join(", ", CartItems.Select(item => $"{item.Quantity}x {item.Name}"));
    }

    private string GetOrderCustomerName()
    {
        return string.IsNullOrWhiteSpace(CustomerName) ? "Walk-in customer" : CustomerName.Trim();
    }

    private void ClearTicket()
    {
        foreach (var item in CartItems)
        {
            item.PropertyChanged -= OnCartItemPropertyChanged;
            item.TotalChanged -= OnCartItemTotalChanged;
        }

        CartItems.Clear();
        CustomerName = string.Empty;
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

    private void ShowCartItemAddOnPicker(CartItem item)
    {
        selectedAddOnCartItem = item;
        ApplyAddOnFilter(item);
        OnPropertyChanged(nameof(AddOnPickerTitle));
        IsAddOnPickerVisible = true;
    }

    private void HideAddOnPicker()
    {
        selectedAddOnCartItem = null;
        AddOnOptions.Clear();
        OnPropertyChanged(nameof(AddOnPickerTitle));
        IsAddOnPickerVisible = false;
    }

    private async Task ShowLowStockNotificationForOrderAsync(
        OrderDto order,
        CancellationToken cancellationToken)
    {
        var orderedIngredientIds = order.Details
            .Select(detail => detail.MenuId)
            .OfType<Guid>()
            .SelectMany(menuId => menuIngredientIdsByMenuId.TryGetValue(menuId, out var ingredientIds)
                ? ingredientIds
                : [])
            .Distinct()
            .ToHashSet();

        if (orderedIngredientIds.Count == 0)
        {
            return;
        }

        var ingredients = await ingredientService.GetIngredientsAsync(cancellationToken);
        LowStockItems.Clear();

        foreach (var ingredient in ingredients
            .Where(ingredient =>
                orderedIngredientIds.Contains(ingredient.Id) &&
                ingredient.IsActive &&
                ingredient.CurrentQuantity <= ingredient.ReorderLevel)
            .OrderBy(ingredient => ingredient.Name))
        {
            LowStockItems.Add(new LowStockAlertItem(
                ingredient.Name,
                ingredient.CurrentQuantity,
                ingredient.ReorderLevel,
                ingredient.UnitOfMeasure));
        }

        OnPropertyChanged(nameof(LowStockNotificationTitle));
        IsLowStockNotificationVisible = LowStockItems.Count > 0;
    }

    private void SelectCartItemAddOn(AddOnOption addOn)
    {
        if (selectedAddOnCartItem is null)
        {
            return;
        }

        selectedAddOnCartItem.AddAddOn(addOn.Id, addOn.Name, Math.Max(addOn.Price, 0));
        RefreshCartItem(selectedAddOnCartItem);
        RefreshTotals();
        HideAddOnPicker();
    }

    private void ApplyAddOnFilter(CartItem item)
    {
        AddOnOptions.Clear();

        if (item.MenuId is not Guid menuId)
        {
            return;
        }

        foreach (var addOn in allAddOnOptions.Where(addOn => addOn.MenuIds.Contains(menuId)))
        {
            AddOnOptions.Add(addOn);
        }
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
        if (!string.Equals(SignInName.Trim(), TestAccountName, StringComparison.OrdinalIgnoreCase) ||
            SignInPin != TestAccountPin)
        {
            SignInMessage = $"Temporary sign in: {TestAccountName} / {TestAccountPin}";
            return;
        }

        identitySession.SignInTemporaryAdmin();
        SignInMessage = string.Empty;
        SignInPin = string.Empty;
        IsSignInVisible = false;
        ShiftStatus = "Admin signed in";
    }

    public void Logout()
    {
        identitySession.SignOut();
        SignInPin = string.Empty;
        SignInMessage = string.Empty;
        IsSignInVisible = false;
        ShiftStatus = "Point of Sale";
    }

    private void HideSignIn()
    {
        SignInMessage = string.Empty;
        IsSignInVisible = false;
    }

    private Guid? GetCurrentUserId()
    {
        return identitySession.CurrentUserId;
    }

    private void OnIdentitySessionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppIdentitySession.IsSignedIn) or nameof(AppIdentitySession.Role))
        {
            OnPropertyChanged(nameof(IsSignedIn));
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
