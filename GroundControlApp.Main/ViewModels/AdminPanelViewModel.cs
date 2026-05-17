using System.Collections.ObjectModel;
using System.Windows.Input;
using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.Models;
using GroundControlApp.Main.Views;

namespace GroundControlApp.Main.ViewModels;

public sealed class AdminPanelViewModel : ObservableObject
{
    private readonly IMenuService menuService;
    private readonly IIngredientService ingredientService;
    private readonly IStockService stockService;
    private readonly IOrderService orderService;
    private readonly ISaleService saleService;
    private readonly ITimeEntryService timeEntryService;
    private readonly IUserService userService;
    private readonly MainPageViewModel mainPageViewModel;
    private string todaySalesMetric = "Loading";
    private string openOrdersMetric = "Loading";
    private string lowStockMetric = "Loading";
    private string activeStaffMetric = "Loading";

    public AdminPanelViewModel(
        IMenuService menuService,
        IIngredientService ingredientService,
        IStockService stockService,
        IOrderService orderService,
        ISaleService saleService,
        ITimeEntryService timeEntryService,
        IUserService userService,
        MainPageViewModel mainPageViewModel)
    {
        this.menuService = menuService;
        this.ingredientService = ingredientService;
        this.stockService = stockService;
        this.orderService = orderService;
        this.saleService = saleService;
        this.timeEntryService = timeEntryService;
        this.userService = userService;
        this.mainPageViewModel = mainPageViewModel;

        ProcessItems =
        [
            new AdminProcessItem("Menu Management", "Menus, menu categories, pricing, SKU availability, and recipe ingredients.", "Coffee menu active", "9 items", nameof(MenuManagementPage)),
            new AdminProcessItem("Ingredients", "Ingredient master data, units of measure, reorder levels, target levels, and unit cost.", "Reorder watch", "3 low", nameof(IngredientsPage)),
            new AdminProcessItem("Stocks", "Batch receiving, supplier notes, expiration dates, stock movement review, and inventory adjustments.", "Needs receiving", "12 batches", nameof(StocksPage)),
            new AdminProcessItem("Orders", "Draft, due, paid, and fulfilled order headers with customer contact and payment state.", "Open queue", "6 orders", nameof(OrdersPage)),
            new AdminProcessItem("Sales", "Receipts, completed sales, discounts, taxes, grand totals, cashier attribution, and payments.", "Today", "PHP 18,420.00", nameof(SalesPage)),
            new AdminProcessItem("Employee Time", "Clock-in records, clock-out records, breaks, hourly rates, and payroll linkage.", "Active shifts", "4 staff", nameof(EmployeeTimePage)),
            new AdminProcessItem("Payroll", "Payroll runs, regular hours, overtime hours, gross pay, status, and processing notes.", "Current period", "Ready", nameof(PayrollPage)),
            new AdminProcessItem("Users and Roles", "Employee numbers, cashier/barista/manager/admin access, PIN setup, and active status.", "Accounts", "8 users", nameof(UsersRolesPage))
        ];

        WorkflowItems =
        [
            new AdminProcessItem("Opening", "Confirm active menus, count opening cash, sync stock levels, and start employee time entries.", "Before sales", "4 tasks"),
            new AdminProcessItem("Operations", "Monitor orders, sales, payments, inventory deductions, and low ingredient alerts.", "Live", "POS linked"),
            new AdminProcessItem("Closing", "Reconcile sales, review stock movements, close open time entries, and prepare payroll data.", "End of day", "5 tasks")
        ];

        BackCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(".."));
        LogoutCommand = new AsyncRelayCommand(async () =>
        {
            mainPageViewModel.Logout();
            await Shell.Current.GoToAsync("..");
        });
        OpenProcessCommand = new RelayCommand(async parameter =>
        {
            if (parameter is AdminProcessItem process && !string.IsNullOrWhiteSpace(process.Route))
            {
                await Shell.Current.GoToAsync(process.Route);
            }
        });
    }

    public ObservableCollection<AdminProcessItem> ProcessItems { get; }

    public ObservableCollection<AdminProcessItem> WorkflowItems { get; }

    public ICommand BackCommand { get; }

    public ICommand LogoutCommand { get; }

    public ICommand OpenProcessCommand { get; }

    public string TodaySalesMetric
    {
        get => todaySalesMetric;
        private set => SetProperty(ref todaySalesMetric, value);
    }

    public string OpenOrdersMetric
    {
        get => openOrdersMetric;
        private set => SetProperty(ref openOrdersMetric, value);
    }

    public string LowStockMetric
    {
        get => lowStockMetric;
        private set => SetProperty(ref lowStockMetric, value);
    }

    public string ActiveStaffMetric
    {
        get => activeStaffMetric;
        private set => SetProperty(ref activeStaffMetric, value);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var menusTask = menuService.GetMenusAsync(cancellationToken);
            var ingredientsTask = ingredientService.GetIngredientsAsync(cancellationToken);
            var stocksTask = stockService.GetStocksAsync(cancellationToken);
            var ordersTask = orderService.GetOrdersAsync(cancellationToken);
            var salesTask = saleService.GetSalesAsync(cancellationToken);
            var timeEntriesTask = timeEntryService.GetTimeEntriesAsync(cancellationToken);
            var payrollRunsTask = timeEntryService.GetPayrollRunsAsync(cancellationToken);
            var usersTask = userService.GetUsersAsync(cancellationToken);

            await Task.WhenAll(
                menusTask,
                ingredientsTask,
                stocksTask,
                ordersTask,
                salesTask,
                timeEntriesTask,
                payrollRunsTask,
                usersTask);

            var menus = await menusTask;
            var ingredients = await ingredientsTask;
            var stocks = await stocksTask;
            var orders = await ordersTask;
            var sales = await salesTask;
            var timeEntries = await timeEntriesTask;
            var payrollRuns = await payrollRunsTask;
            var users = await usersTask;

            var todaySales = sales.Where(sale => sale.SoldAtUtc.Date == DateTime.Today).ToArray();

            TodaySalesMetric = $"PHP {todaySales.Sum(sale => sale.GrandTotal):N2}";
            OpenOrdersMetric = orders.Count(order => order.Status is 1 or 2 or 3).ToString();
            LowStockMetric = ingredients.Count(ingredient => ingredient.CurrentQuantity <= ingredient.ReorderLevel).ToString();
            ActiveStaffMetric = timeEntries.Count(entry => !entry.IsClosed).ToString();

            ProcessItems.Clear();
            ProcessItems.Add(new AdminProcessItem("Menu Management", "Menus, menu categories, pricing, SKU availability, and recipe ingredients.", $"{menus.Count(menu => menu.IsAvailable)} available", $"{menus.Count} items", nameof(MenuManagementPage)));
            ProcessItems.Add(new AdminProcessItem("Ingredients", "Ingredient master data, units of measure, reorder levels, target levels, and unit cost.", "Reorder watch", $"{ingredients.Count(ingredient => ingredient.CurrentQuantity <= ingredient.ReorderLevel)} low", nameof(IngredientsPage)));
            ProcessItems.Add(new AdminProcessItem("Stocks", "Batch receiving, supplier notes, expiration dates, stock movement review, and inventory adjustments.", $"{stocks.Count(stock => stock.Quantity > 0)} active", $"{stocks.Count} batches", nameof(StocksPage)));
            ProcessItems.Add(new AdminProcessItem("Orders", "Draft, due, paid, and fulfilled order headers with customer contact and payment state.", "Open queue", $"{orders.Count} orders", nameof(OrdersPage)));
            ProcessItems.Add(new AdminProcessItem("Sales", "Receipts from paid orders, discounts, taxes, grand totals, cashier attribution, and payments.", "Paid orders", $"PHP {sales.Sum(sale => sale.GrandTotal):N2}", nameof(SalesPage)));
            ProcessItems.Add(new AdminProcessItem("Employee Time", "Clock-in records, clock-out records, breaks, hourly rates, and payroll linkage.", "Active shifts", $"{timeEntries.Count(entry => !entry.IsClosed)} staff", nameof(EmployeeTimePage)));
            ProcessItems.Add(new AdminProcessItem("Payroll", "Payroll runs, regular hours, overtime hours, gross pay, status, and processing notes.", "Processed", $"{payrollRuns.Count} runs", nameof(PayrollPage)));
            ProcessItems.Add(new AdminProcessItem("Users and Roles", "Employee numbers, cashier/barista/manager/admin access, PIN setup, and active status.", "Accounts", $"{users.Count} users", nameof(UsersRolesPage)));
        }
        catch
        {
            TodaySalesMetric = "Offline";
            OpenOrdersMetric = "Offline";
            LowStockMetric = "Offline";
            ActiveStaffMetric = "Offline";
        }
    }
}
