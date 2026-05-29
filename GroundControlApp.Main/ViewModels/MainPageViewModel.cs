using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
    private readonly IUserService userService;
    private readonly AppIdentitySession identitySession;
    private readonly List<PosProduct> allProducts = [];
    private readonly List<AddOnOption> allAddOnOptions = [];
    private readonly List<PendingOrderItem> completedOrders = [];
    private readonly Dictionary<Guid, IReadOnlyCollection<Guid>> menuIngredientIdsByMenuId = [];
    private string searchText = string.Empty;
    private string currentTimeDisplay = DateTime.Now.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);
    private string customerName = string.Empty;
    private string customerPhone = string.Empty;
    private string customerAddress = string.Empty;
    private string orderNotes = string.Empty;
    private string selectedTender = "Cash";
    private string selectedOrderTab = "Pending";
    private string cashAmountText = string.Empty;
    private string cardAmountText = string.Empty;
    private string gcashAmountText = string.Empty;
    private string otherAmountText = string.Empty;
    private string shiftStatus = "Point of Sale";
    private string syncStatus = "Ready for API-backed inventory.";
    private string ticketStatus = "New ticket";
    private string selectedCategory = "All";
    private string signInName = TestAccountName;
    private string signInPin = string.Empty;
    private string signInMessage = string.Empty;
    private CartItem? selectedAddOnCartItem;
    private bool hasOrderDiscount;
    private bool isShiftOpen;
    private Guid? shiftOpenedByUserId;
    private DateTime? shiftOpenedAt;
    private int completedTransactionCount;
    private int voidedTransactionCount;
    private bool isSignInVisible;
    private bool isAddOnPickerVisible;
    private bool isLowStockNotificationVisible;

    public MainPageViewModel(
        IMenuService menuService,
        IAddOnService addOnService,
        IIngredientService ingredientService,
        IOrderService orderService,
        IUserService userService,
        AppIdentitySession identitySession)
    {
        this.menuService = menuService;
        this.addOnService = addOnService;
        this.ingredientService = ingredientService;
        this.orderService = orderService;
        this.userService = userService;
        this.identitySession = identitySession;
        identitySession.PropertyChanged += OnIdentitySessionChanged;
        Categories = [];

        Products = [];

        CartItems = [];

        PendingOrders = [];

        DisplayedOrders = [];

        OrderTabs = [];

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
                FillSelectedTenderRemainder();
            }
        });
        SelectOrderTabCommand = new RelayCommand(parameter =>
        {
            if (parameter is PosCategory tab)
            {
                SelectOrderTab(tab.Name);
            }
            else if (parameter is string tabName)
            {
                SelectOrderTab(tabName);
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
        ApplyLineDiscountCommand = new RelayCommand(parameter =>
        {
            if (parameter is CartItem item)
            {
                item.ToggleDiscount();
                RefreshCartItem(item);
                RefreshTotals();
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
        SignInCommand = new AsyncRelayCommand(() => SignInAsync());
        LogoutCommand = new RelayCommand(Logout);
        OpenHomeCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("//HomePage"));
        OpenAdminCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(nameof(Views.AdminPanelPage)), () => IsAdminVisible);
        ScanCatalogCommand = new AsyncRelayCommand(() => LoadAsync());
        OpenShiftCommand = new RelayCommand(OpenShift);
        SyncCommand = new RelayCommand(() => SyncStatus = "Synced");
        HoldTicketCommand = new RelayCommand(() => TicketStatus = "On hold");
        ChargeCommand = new AsyncRelayCommand(() => CreatePendingOrderAsync());
        FinishOrderCommand = new AsyncRelayCommand(FinishOrderAsync);
        DiscountCommand = new RelayCommand(ToggleOrderDiscount);
        VoidCommand = new RelayCommand(VoidTicket);

        RefreshOrderTabs();
        ApplyOrderTabFilter();
    }

    public ObservableCollection<PosCategory> Categories { get; }

    public ObservableCollection<PosProduct> Products { get; }

    public ObservableCollection<CartItem> CartItems { get; }

    public ObservableCollection<PendingOrderItem> PendingOrders { get; }

    public ObservableCollection<PendingOrderItem> DisplayedOrders { get; }

    public ObservableCollection<PosCategory> OrderTabs { get; }

    public ObservableCollection<AddOnOption> AddOnOptions { get; }

    public ObservableCollection<LowStockAlertItem> LowStockItems { get; }

    public ICommand SelectCategoryCommand { get; }

    public ICommand AddProductCommand { get; }

    public ICommand SelectTenderCommand { get; }

    public ICommand SelectOrderTabCommand { get; }

    public ICommand IncreaseQuantityCommand { get; }

    public ICommand DecreaseQuantityCommand { get; }

    public ICommand RemoveCartItemCommand { get; }

    public ICommand ApplyLineDiscountCommand { get; }

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
        set
        {
            if (SetProperty(ref searchText, value))
            {
                ApplyCategoryFilter();
            }
        }
    }

    public string CustomerName
    {
        get => customerName;
        set => SetProperty(ref customerName, value);
    }

    public string CustomerPhone
    {
        get => customerPhone;
        set => SetProperty(ref customerPhone, value);
    }

    public string CustomerAddress
    {
        get => customerAddress;
        set => SetProperty(ref customerAddress, value);
    }

    public string OrderNotes
    {
        get => orderNotes;
        set => SetProperty(ref orderNotes, value);
    }

    public string SelectedTender
    {
        get => selectedTender;
        set
        {
            if (SetProperty(ref selectedTender, value))
            {
                TicketStatus = $"{SelectedTender} selected";
            }
        }
    }

    public string CashAmountText
    {
        get => cashAmountText;
        set
        {
            if (SetProperty(ref cashAmountText, value))
            {
                RefreshPaymentTotals();
            }
        }
    }

    public string CardAmountText
    {
        get => cardAmountText;
        set
        {
            if (SetProperty(ref cardAmountText, value))
            {
                RefreshPaymentTotals();
            }
        }
    }

    public string GCashAmountText
    {
        get => gcashAmountText;
        set
        {
            if (SetProperty(ref gcashAmountText, value))
            {
                RefreshPaymentTotals();
            }
        }
    }

    public string OtherAmountText
    {
        get => otherAmountText;
        set
        {
            if (SetProperty(ref otherAmountText, value))
            {
                RefreshPaymentTotals();
            }
        }
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

    public bool IsShiftOpen
    {
        get => isShiftOpen;
        private set
        {
            if (SetProperty(ref isShiftOpen, value))
            {
                OnPropertyChanged(nameof(OpenShiftText));
                OnPropertyChanged(nameof(CanOpenShift));
                OnPropertyChanged(nameof(ShiftSummaryDisplay));
                RaiseOpenShiftCanExecuteChanged();
            }
        }
    }

    public bool CanOpenShift => IsSignedIn && identitySession.CanAccess(AppRole.Cashier) && !IsShiftOpen;

    public string OpenShiftText => IsShiftOpen ? "Close Shift" : "Open Shift";

    public string CurrentTimeDisplay
    {
        get => currentTimeDisplay;
        private set => SetProperty(ref currentTimeDisplay, value);
    }

    public string UserStatusDisplay => identitySession.IsSignedIn ? identitySession.DisplayName : "Guest";

    public string RoleStatusDisplay => identitySession.RoleName;

    public string ShiftSummaryDisplay => IsShiftOpen
        ? $"Open {shiftOpenedAt?.ToString("h:mm tt", CultureInfo.InvariantCulture)}"
        : "Closed";

    public string OrderStatusDisplay => $"{PendingOrderCount} pending / {CompletedTransactionCount} done";

    public string TicketStatusDisplay => CartItems.Count == 0
        ? "Ticket empty"
        : $"{CartItems.Sum(item => item.Quantity)} item(s)";

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

    public int PendingOrderCount => PendingOrders.Count(order => order.Status == "Pending");

    public int InProgressOrderCount => PendingOrders.Count(order => order.Status == "Finishing");

    public int CompletedTransactionCount
    {
        get => completedTransactionCount;
        private set
        {
            if (SetProperty(ref completedTransactionCount, value))
            {
                OnPropertyChanged(nameof(ShiftTransactionDisplay));
            }
        }
    }

    public int VoidedTransactionCount
    {
        get => voidedTransactionCount;
        private set
        {
            if (SetProperty(ref voidedTransactionCount, value))
            {
                OnPropertyChanged(nameof(ShiftTransactionDisplay));
            }
        }
    }

    public string ShiftTransactionDisplay => $"{CompletedTransactionCount} completed / {VoidedTransactionCount} voided";

    public string PendingOrderTabText => $"Pending ({PendingOrderCount})";

    public string InProgressOrderTabText => $"In progress ({InProgressOrderCount})";

    public string CompletedOrderTabText => $"Completed ({CompletedTransactionCount})";

    public string PendingOrderTabBackgroundColor => selectedOrderTab.StartsWith("Pending", StringComparison.OrdinalIgnoreCase)
        ? "#F4F6F8"
        : "#243B64";

    public string PendingOrderTabTextColor => selectedOrderTab.StartsWith("Pending", StringComparison.OrdinalIgnoreCase)
        ? "#14213D"
        : "#FFFFFF";

    public string InProgressOrderTabBackgroundColor => selectedOrderTab.StartsWith("In progress", StringComparison.OrdinalIgnoreCase)
        ? "#F4F6F8"
        : "#243B64";

    public string InProgressOrderTabTextColor => selectedOrderTab.StartsWith("In progress", StringComparison.OrdinalIgnoreCase)
        ? "#14213D"
        : "#FFFFFF";

    public string CompletedOrderTabBackgroundColor => selectedOrderTab.StartsWith("Completed", StringComparison.OrdinalIgnoreCase)
        ? "#F4F6F8"
        : "#243B64";

    public string CompletedOrderTabTextColor => selectedOrderTab.StartsWith("Completed", StringComparison.OrdinalIgnoreCase)
        ? "#14213D"
        : "#FFFFFF";

    public decimal Subtotal => CartItems.Sum(CalculateCartItemTotal);

    public decimal LineDiscountTotal => CartItems.Sum(item => item.DiscountAmount);

    public decimal OrderDiscount => hasOrderDiscount ? Math.Round(Subtotal * 0.10m, 2) : 0;

    public decimal Total => Math.Max(0, Subtotal - OrderDiscount);

    public string SubtotalDisplay => $"PHP {Subtotal:N2}";

    public string LineDiscountDisplay => $"- PHP {LineDiscountTotal:N2}";

    public string OrderDiscountDisplay => $"- PHP {OrderDiscount:N2}";

    public string TotalDisplay => $"PHP {Total:N2}";

    public string ChargeText => $"Charge {TotalDisplay}";

    public decimal TenderedTotal =>
        ReadPaymentAmount(CashAmountText) +
        ReadPaymentAmount(CardAmountText) +
        ReadPaymentAmount(GCashAmountText) +
        ReadPaymentAmount(OtherAmountText);

    public decimal BalanceDue => Math.Max(0, Total - TenderedTotal);

    public decimal ChangeDue => Math.Max(0, TenderedTotal - Total);

    public string TenderedTotalDisplay => $"PHP {TenderedTotal:N2}";

    public string BalanceDueDisplay => $"PHP {BalanceDue:N2}";

    public string ChangeDueDisplay => $"PHP {ChangeDue:N2}";

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
                allProducts.Add(new PosProduct(
                    menu.Id,
                    menu.Name,
                    menu.Category,
                    GetMenuCategoryName(menu.Category),
                    menu.Price,
                    EstimateMenuStock(menu, ingredients)));
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

        IEnumerable<PosProduct> filteredProducts = selectedCategory == "All"
            ? allProducts
            : allProducts.Where(product => product.Category == selectedCategory);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var query = SearchText.Trim();
            filteredProducts = filteredProducts.Where(product =>
                product.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                product.Category.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

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

        if (!IsShiftOpen)
        {
            TicketStatus = "Open shift before charging.";
            return;
        }

        if (BalanceDue > 0)
        {
            TicketStatus = $"Collect {BalanceDueDisplay} before charging.";
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
            RefreshOrderCounts();
            ApplyOrderTabFilter();

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
            RefreshOrderCounts();
            var order = await orderService.PayOrderAsync(
                pendingOrder.Id,
                new PayOrderDto(pendingOrder.Total, GetCurrentUserId()),
                $"pos-payment-{pendingOrder.Id:N}",
                CancellationToken.None);

            await ShowLowStockNotificationForOrderAsync(order, CancellationToken.None);
            PendingOrders.Remove(pendingOrder);
            pendingOrder.Status = "Completed";
            completedOrders.Insert(0, pendingOrder);
            CompletedTransactionCount++;
            RefreshOrderCounts();
            ApplyOrderTabFilter();
            TicketStatus = $"Order {pendingOrder.InvoiceNumber} finished. Ingredients deducted.";
        }
        catch (Exception ex)
        {
            pendingOrder.Status = "Pending";
            RefreshOrderCounts();
            ApplyOrderTabFilter();
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
            CustomerPhone: string.IsNullOrWhiteSpace(CustomerPhone) ? null : CustomerPhone.Trim(),
            Subtotal: Subtotal,
            DiscountTotal: OrderDiscount,
            TaxTotal: 0,
            GrandTotal: Total,
            Notes: CreateOrderNotes(),
            UserId: GetCurrentUserId(),
            Details: CartItems.Select(item => new CreateOrderDetailDto(
                MenuId: item.MenuId,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                DiscountAmount: item.DiscountAmount,
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
        CustomerPhone = string.Empty;
        CustomerAddress = string.Empty;
        OrderNotes = string.Empty;
        ClearPayments();
        hasOrderDiscount = false;
        RefreshTotals();
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(LineDiscountTotal));
        OnPropertyChanged(nameof(OrderDiscount));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(SubtotalDisplay));
        OnPropertyChanged(nameof(LineDiscountDisplay));
        OnPropertyChanged(nameof(OrderDiscountDisplay));
        OnPropertyChanged(nameof(TotalDisplay));
        OnPropertyChanged(nameof(ChargeText));
        OnPropertyChanged(nameof(TicketStatusDisplay));
        RefreshPaymentTotals();
    }

    private void OnCartItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CartItem.Total) or nameof(CartItem.Quantity) or nameof(CartItem.AddOnTotal) or nameof(CartItem.DiscountAmount))
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
        return item.Total;
    }

    private void ToggleOrderDiscount()
    {
        if (CartItems.Count == 0)
        {
            TicketStatus = "Add items before discount.";
            return;
        }

        hasOrderDiscount = !hasOrderDiscount;
        TicketStatus = hasOrderDiscount ? "10% order discount applied" : "Order discount removed";
        RefreshTotals();
    }

    private void VoidTicket()
    {
        if (CartItems.Count == 0)
        {
            TicketStatus = "No ticket to void.";
            return;
        }

        ClearTicket();
        VoidedTransactionCount++;
        OnPropertyChanged(nameof(ShiftTransactionDisplay));
        OnPropertyChanged(nameof(OrderStatusDisplay));
        TicketStatus = "Ticket voided.";
    }

    private void RefreshOrderCounts()
    {
        OnPropertyChanged(nameof(PendingOrderCount));
        OnPropertyChanged(nameof(InProgressOrderCount));
        OnPropertyChanged(nameof(CompletedTransactionCount));
        OnPropertyChanged(nameof(ShiftTransactionDisplay));
        OnPropertyChanged(nameof(PendingOrderTabText));
        OnPropertyChanged(nameof(InProgressOrderTabText));
        OnPropertyChanged(nameof(CompletedOrderTabText));
        OnPropertyChanged(nameof(OrderStatusDisplay));
        RefreshOrderTabs();
    }

    public void RefreshStatusClock()
    {
        CurrentTimeDisplay = DateTime.Now.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);
    }

    private void OpenShift()
    {
        if (!IsSignedIn)
        {
            SignInMessage = "Login before opening a shift.";
            IsSignInVisible = true;
            return;
        }

        if (!identitySession.CanAccess(AppRole.Cashier))
        {
            ShiftStatus = "Cashier access or higher is required to open shift.";
            return;
        }

        if (IsShiftOpen)
        {
            CloseShift();
            return;
        }

        completedOrders.Clear();
        CompletedTransactionCount = 0;
        VoidedTransactionCount = 0;
        shiftOpenedByUserId = identitySession.CurrentUserId;
        shiftOpenedAt = DateTime.Now;
        IsShiftOpen = true;
        ShiftStatus = $"Shift open for {identitySession.DisplayName} at {shiftOpenedAt.Value:h:mm tt}";
        RefreshOrderCounts();
        ApplyOrderTabFilter();
    }

    private void CloseShift()
    {
        if (shiftOpenedByUserId != identitySession.CurrentUserId &&
            !identitySession.CanAccess(AppRole.Manager))
        {
            ShiftStatus = "Only the shift opener, manager, or admin can close this shift.";
            return;
        }

        if (PendingOrderCount > 0 || InProgressOrderCount > 0)
        {
            ShiftStatus = "Finish pending orders before closing shift.";
            return;
        }

        var openedAt = shiftOpenedAt;
        shiftOpenedByUserId = null;
        shiftOpenedAt = null;
        IsShiftOpen = false;
        ShiftStatus = openedAt is null
            ? "Shift closed."
            : $"Shift closed. Opened {openedAt.Value:h:mm tt}.";
    }

    private string CreateOrderNotes()
    {
        var notes = new List<string> { CreatePaymentSummary() };

        if (!string.IsNullOrWhiteSpace(CustomerAddress))
        {
            notes.Add($"Address: {CustomerAddress.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(OrderNotes))
        {
            notes.Add(OrderNotes.Trim());
        }

        return string.Join(Environment.NewLine, notes);
    }

    private void SelectOrderTab(string tabName)
    {
        selectedOrderTab = tabName;
        RefreshOrderTabs();
        OnPropertyChanged(nameof(PendingOrderTabBackgroundColor));
        OnPropertyChanged(nameof(PendingOrderTabTextColor));
        OnPropertyChanged(nameof(InProgressOrderTabBackgroundColor));
        OnPropertyChanged(nameof(InProgressOrderTabTextColor));
        OnPropertyChanged(nameof(CompletedOrderTabBackgroundColor));
        OnPropertyChanged(nameof(CompletedOrderTabTextColor));
        ApplyOrderTabFilter();
    }

    private void RefreshOrderTabs()
    {
        OrderTabs.Clear();
        OrderTabs.Add(new PosCategory($"Pending ({PendingOrderCount})", selectedOrderTab.StartsWith("Pending", StringComparison.OrdinalIgnoreCase)));
        OrderTabs.Add(new PosCategory($"In progress ({InProgressOrderCount})", selectedOrderTab.StartsWith("In progress", StringComparison.OrdinalIgnoreCase)));
        OrderTabs.Add(new PosCategory($"Completed ({CompletedTransactionCount})", selectedOrderTab.StartsWith("Completed", StringComparison.OrdinalIgnoreCase)));
    }

    private void ApplyOrderTabFilter()
    {
        DisplayedOrders.Clear();

        IEnumerable<PendingOrderItem> orders = selectedOrderTab switch
        {
            var tab when tab.StartsWith("In progress", StringComparison.OrdinalIgnoreCase) =>
                PendingOrders.Where(order => order.Status == "Finishing"),
            var tab when tab.StartsWith("Completed", StringComparison.OrdinalIgnoreCase) => completedOrders,
            _ => PendingOrders.Where(order => order.Status == "Pending")
        };

        foreach (var order in orders)
        {
            DisplayedOrders.Add(order);
        }
    }

    private void FillSelectedTenderRemainder()
    {
        var remaining = BalanceDue;
        if (remaining <= 0)
        {
            remaining = Total;
        }

        var value = remaining.ToString("0.##", CultureInfo.InvariantCulture);

        switch (SelectedTender)
        {
            case "Cash":
                CashAmountText = value;
                break;
            case "Card":
                CardAmountText = value;
                break;
            case "GCash":
                GCashAmountText = value;
                break;
            case "Other":
                OtherAmountText = value;
                break;
        }
    }

    private void RefreshPaymentTotals()
    {
        OnPropertyChanged(nameof(TenderedTotal));
        OnPropertyChanged(nameof(BalanceDue));
        OnPropertyChanged(nameof(ChangeDue));
        OnPropertyChanged(nameof(TenderedTotalDisplay));
        OnPropertyChanged(nameof(BalanceDueDisplay));
        OnPropertyChanged(nameof(ChangeDueDisplay));
    }

    private void ClearPayments()
    {
        CashAmountText = string.Empty;
        CardAmountText = string.Empty;
        GCashAmountText = string.Empty;
        OtherAmountText = string.Empty;
    }

    private string CreatePaymentSummary()
    {
        var payments = new List<string>();
        AddPaymentSummary(payments, "Cash", CashAmountText);
        AddPaymentSummary(payments, "Card", CardAmountText);
        AddPaymentSummary(payments, "GCash", GCashAmountText);
        AddPaymentSummary(payments, "Other", OtherAmountText);

        var paymentText = payments.Count == 0
            ? $"Tender: {SelectedTender}"
            : $"Payments: {string.Join(", ", payments)}";

        return ChangeDue > 0
            ? $"{paymentText}; Change: PHP {ChangeDue:N2}"
            : paymentText;
    }

    private static void AddPaymentSummary(List<string> payments, string label, string amountText)
    {
        var amount = ReadPaymentAmount(amountText);
        if (amount > 0)
        {
            payments.Add($"{label} PHP {amount:N2}");
        }
    }

    private static decimal ReadPaymentAmount(string amountText)
    {
        if (decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureAmount) ||
            decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out currentCultureAmount))
        {
            return Math.Max(0, currentCultureAmount);
        }

        return 0;
    }

    private static int? EstimateMenuStock(MenuDto menu, IReadOnlyCollection<IngredientDto> ingredients)
    {
        if (menu.Ingredients is null || menu.Ingredients.Count == 0)
        {
            return null;
        }

        var ingredientLookup = ingredients.ToDictionary(ingredient => ingredient.Id);
        var servings = menu.Ingredients
            .Where(recipe => recipe.Quantity > 0 && ingredientLookup.ContainsKey(recipe.IngredientId))
            .Select(recipe => ingredientLookup[recipe.IngredientId].CurrentQuantity / recipe.Quantity)
            .ToArray();

        return servings.Length == 0 ? null : Math.Max(0, (int)Math.Floor(servings.Min()));
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

    private async Task SignInAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(SignInName) || string.IsNullOrWhiteSpace(SignInPin))
        {
            SignInMessage = "Enter your employee number, email, or name and PIN.";
            return;
        }

        try
        {
            var user = await userService.AuthenticateAsync(
                new AuthenticateUserDto(SignInName.Trim(), SignInPin),
                cancellationToken);

            identitySession.SignIn(user);
            SignInMessage = string.Empty;
            SignInPin = string.Empty;
            IsSignInVisible = false;
            ShiftStatus = $"{identitySession.RoleName} signed in";
            OnPropertyChanged(nameof(UserStatusDisplay));
            OnPropertyChanged(nameof(RoleStatusDisplay));
        }
        catch
        {
            SignInMessage = "Login failed. Check the user is active and the PIN is correct.";
        }
    }

    public void Logout()
    {
        identitySession.SignOut();
        SignInPin = string.Empty;
        SignInMessage = string.Empty;
        IsSignInVisible = false;
        IsShiftOpen = false;
        shiftOpenedByUserId = null;
        shiftOpenedAt = null;
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
            OnPropertyChanged(nameof(UserStatusDisplay));
            OnPropertyChanged(nameof(RoleStatusDisplay));
            OnPropertyChanged(nameof(CanOpenShift));
            RaiseOpenShiftCanExecuteChanged();
            if (OpenAdminCommand is AsyncRelayCommand openAdminCommand)
            {
                openAdminCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private void RaiseOpenShiftCanExecuteChanged()
    {
        if (OpenShiftCommand is RelayCommand openShiftCommand)
        {
            openShiftCommand.RaiseCanExecuteChanged();
        }
    }
}
