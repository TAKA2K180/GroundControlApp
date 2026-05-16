using System.Collections.ObjectModel;
using System.Windows.Input;
using GroundControlApp.Data.Services;
using GroundControlApp.Main.Models;

namespace GroundControlApp.Main.ViewModels;

public sealed class BackOfficeProcessViewModel : ObservableObject
{
    private readonly IGroundControlApiClient apiClient;
    private readonly string processKey;
    private string primaryMetric;
    private string secondaryMetric;
    private string tertiaryMetric;

    private BackOfficeProcessViewModel(
        string title,
        string subtitle,
        string primaryMetric,
        string secondaryMetric,
        string tertiaryMetric,
        string processKey,
        IGroundControlApiClient apiClient,
        IEnumerable<AdminTaskItem> tasks,
        IEnumerable<AdminRecordItem> records)
    {
        Title = title;
        Subtitle = subtitle;
        this.primaryMetric = primaryMetric;
        this.secondaryMetric = secondaryMetric;
        this.tertiaryMetric = tertiaryMetric;
        this.processKey = processKey;
        this.apiClient = apiClient;
        Tasks = new ObservableCollection<AdminTaskItem>(tasks);
        Records = new ObservableCollection<AdminRecordItem>(records);
        BackCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(".."));
    }

    public string Title { get; }

    public string Subtitle { get; }

    public string PrimaryMetric
    {
        get => primaryMetric;
        private set => SetProperty(ref primaryMetric, value);
    }

    public string SecondaryMetric
    {
        get => secondaryMetric;
        private set => SetProperty(ref secondaryMetric, value);
    }

    public string TertiaryMetric
    {
        get => tertiaryMetric;
        private set => SetProperty(ref tertiaryMetric, value);
    }

    public ObservableCollection<AdminTaskItem> Tasks { get; }

    public ObservableCollection<AdminRecordItem> Records { get; }

    public ICommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            switch (processKey)
            {
                case "menus":
                    await LoadMenusAsync(cancellationToken);
                    break;
                case "ingredients":
                    await LoadIngredientsAsync(cancellationToken);
                    break;
                case "stocks":
                    await LoadStocksAsync(cancellationToken);
                    break;
                case "employee-time":
                    await LoadTimeEntriesAsync(cancellationToken);
                    break;
                default:
                    Records.Clear();
                    Records.Add(new AdminRecordItem("Endpoint not available", $"No list endpoint exists yet for {Title}.", "API", "Pending"));
                    TertiaryMetric = "Needs API";
                    break;
            }
        }
        catch (Exception ex)
        {
            Records.Clear();
            Records.Add(new AdminRecordItem("API unavailable", ex.Message, "Retry", "Error"));
            TertiaryMetric = "Offline";
        }
    }

    private async Task LoadMenusAsync(CancellationToken cancellationToken)
    {
        var menus = await apiClient.GetMenusAsync(cancellationToken);
        Records.Clear();

        foreach (var menu in menus)
        {
            Records.Add(new AdminRecordItem(
                menu.Name,
                $"{GetMenuCategoryName(menu.Category)} - SKU {menu.Sku}",
                $"PHP {menu.Price:N2}",
                menu.IsAvailable ? "Available" : "Unavailable"));
        }

        PrimaryMetric = $"{menus.Count} menu items";
        SecondaryMetric = $"{menus.Count(menu => menu.IsAvailable)} available";
        TertiaryMetric = $"{menus.Count(menu => !menu.IsAvailable)} unavailable";
    }

    private async Task LoadIngredientsAsync(CancellationToken cancellationToken)
    {
        var ingredients = await apiClient.GetIngredientsAsync(cancellationToken);
        Records.Clear();

        foreach (var ingredient in ingredients)
        {
            Records.Add(new AdminRecordItem(
                ingredient.Name,
                $"{ingredient.UnitOfMeasure} - reorder {ingredient.ReorderLevel:N2} - target {ingredient.TargetLevel:N2}",
                $"{ingredient.CurrentQuantity:N2}",
                ingredient.IsActive ? "Active" : "Inactive"));
        }

        PrimaryMetric = $"{ingredients.Count} ingredients";
        SecondaryMetric = $"{ingredients.Count(item => item.CurrentQuantity <= item.ReorderLevel)} low stock";
        TertiaryMetric = $"PHP {ingredients.Sum(item => item.UnitCost):N2}";
    }

    private async Task LoadStocksAsync(CancellationToken cancellationToken)
    {
        var stocks = await apiClient.GetStocksAsync(cancellationToken);
        Records.Clear();

        foreach (var stock in stocks)
        {
            Records.Add(new AdminRecordItem(
                stock.BatchNumber,
                $"{stock.IngredientName ?? "Ingredient"} - {stock.SupplierName ?? "No supplier"}",
                $"{stock.Quantity:N2}",
                stock.ExpirationDate is null ? "No expiry" : stock.ExpirationDate.Value.ToString("MMM d, yyyy")));
        }

        PrimaryMetric = $"{stocks.Count} batches";
        SecondaryMetric = $"{stocks.Select(stock => stock.SupplierName).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().Count()} suppliers";
        TertiaryMetric = $"PHP {stocks.Sum(stock => stock.UnitCost):N2}";
    }

    private async Task LoadTimeEntriesAsync(CancellationToken cancellationToken)
    {
        var timeEntries = await apiClient.GetTimeEntriesAsync(cancellationToken);
        Records.Clear();

        foreach (var timeEntry in timeEntries)
        {
            Records.Add(new AdminRecordItem(
                timeEntry.EmployeeName ?? timeEntry.UserId.ToString(),
                $"Clock in {timeEntry.ClockInAtUtc:g} - break {timeEntry.BreakMinutes} min",
                $"PHP {timeEntry.HourlyRate:N2}/hr",
                timeEntry.IsClosed ? "Closed" : "Open"));
        }

        PrimaryMetric = $"{timeEntries.Count(entry => !entry.IsClosed)} active";
        SecondaryMetric = $"{timeEntries.Sum(entry => entry.HoursWorked):N2} hours";
        TertiaryMetric = $"{timeEntries.Count(entry => entry.IsPaid)} paid";
    }

    public static BackOfficeProcessViewModel CreateMenuManagement(IGroundControlApiClient apiClient)
    {
        return new(
            "Menu Management",
            "Create menu items, set coffee categories, manage SKUs, pricing, availability, and recipe ingredient mappings.",
            "9 menu items",
            "7 categories",
            "2 unavailable",
            "menus",
            apiClient,
            [
                new AdminTaskItem("Add menu item", "Create a coffee, pastry, food, add-on, or merchandise item.", "New Item"),
                new AdminTaskItem("Update recipe", "Attach menu ingredients so inventory can be deducted when sales close.", "Recipe"),
                new AdminTaskItem("Set availability", "Temporarily disable out-of-stock items from the POS catalog.", "Availability")
            ],
            [
                new AdminRecordItem("Benguet Arabica", "Coffee - SKU GC-COF-001", "PHP 280.00", "Available"),
                new AdminRecordItem("Cold Brew Blend", "Coffee - SKU GC-COF-008", "PHP 320.00", "Available"),
                new AdminRecordItem("Blueberry Muffin", "Pastry - SKU GC-PAS-004", "PHP 145.00", "Unavailable")
            ]);
    }

    public static BackOfficeProcessViewModel CreateIngredients(IGroundControlApiClient apiClient)
    {
        return new(
            "Ingredients",
            "Maintain ingredient master data, units, current quantity, reorder level, target level, and unit cost.",
            "18 ingredients",
            "3 low stock",
            "PHP 42 avg cost",
            "ingredients",
            apiClient,
            [
                new AdminTaskItem("Create ingredient", "Add beans, milk, syrups, cups, lids, pastry inputs, and other consumables.", "New Ingredient"),
                new AdminTaskItem("Set reorder level", "Tune low-stock triggers per unit of measure.", "Reorder"),
                new AdminTaskItem("Review unit cost", "Update costs used by stock batches and recipe costing.", "Costing")
            ],
            [
                new AdminRecordItem("Arabica Beans", "kg - target 25", "12.5 kg", "Low"),
                new AdminRecordItem("Oat Milk", "liter - target 18", "21 L", "Healthy"),
                new AdminRecordItem("12 oz Cups", "pieces - target 500", "220 pcs", "Watch")
            ]);
    }

    public static BackOfficeProcessViewModel CreateStocks(IGroundControlApiClient apiClient)
    {
        return new(
            "Stocks",
            "Receive stock batches, track suppliers, expiration dates, notes, and inventory movement history.",
            "12 batches",
            "4 suppliers",
            "3 adjustments",
            "stocks",
            apiClient,
            [
                new AdminTaskItem("Receive stock", "Record batch number, quantity, unit cost, supplier, and expiration date.", "Receive"),
                new AdminTaskItem("Adjust inventory", "Create stock movements for waste, count corrections, and transfers.", "Adjust"),
                new AdminTaskItem("Audit movements", "Review movement type, ingredient, user, quantity, and notes.", "Audit")
            ],
            [
                new AdminRecordItem("BATCH-AR-0526", "Arabica Beans from Baguio Roasters", "10 kg", "Received"),
                new AdminRecordItem("BATCH-OM-0526", "Oat Milk from Daily Supply", "24 L", "Received"),
                new AdminRecordItem("ADJ-CUP-001", "12 oz Cups count correction", "-30 pcs", "Adjusted")
            ]);
    }

    public static BackOfficeProcessViewModel CreateOrders(IGroundControlApiClient apiClient)
    {
        return new(
            "Orders",
            "Manage order headers, details, status, due dates, customer contacts, payments, and inventory deduction state.",
            "6 open",
            "2 due today",
            "PHP 7,840.00",
            "orders",
            apiClient,
            [
                new AdminTaskItem("Review drafts", "Complete pending order headers before fulfillment.", "Drafts"),
                new AdminTaskItem("Update payment", "Record payment idempotency, amount paid, and order status.", "Payment"),
                new AdminTaskItem("Fulfill order", "Confirm details and mark inventory deducted when completed.", "Fulfill")
            ],
            [
                new AdminRecordItem("INV-000126", "Mika Santos - pickup 3:00 PM", "PHP 1,240.00", "Paid"),
                new AdminRecordItem("INV-000127", "Walk-in customer", "PHP 680.00", "Draft"),
                new AdminRecordItem("INV-000128", "R. Cruz - event beans", "PHP 5,920.00", "Due")
            ]);
    }

    public static BackOfficeProcessViewModel CreateSales(IGroundControlApiClient apiClient)
    {
        return new(
            "Sales",
            "Review receipts, cashier attribution, sale status, discounts, taxes, grand totals, and payment methods.",
            "PHP 18,420.00",
            "42 receipts",
            "PHP 438 avg",
            "sales",
            apiClient,
            [
                new AdminTaskItem("Sales report", "Review completed, voided, and refunded sale status totals.", "Report"),
                new AdminTaskItem("Payment review", "Check card, cash, and split payment records.", "Payments"),
                new AdminTaskItem("Discount audit", "Review manual discount totals and sale notes.", "Audit")
            ],
            [
                new AdminRecordItem("RCPT-000421", "Cashier: Aya - Card", "PHP 540.00", "Completed"),
                new AdminRecordItem("RCPT-000422", "Cashier: Ben - Cash", "PHP 320.00", "Completed"),
                new AdminRecordItem("RCPT-000423", "Cashier: Aya - Split", "PHP 1,180.00", "Completed")
            ]);
    }

    public static BackOfficeProcessViewModel CreateEmployeeTime(IGroundControlApiClient apiClient)
    {
        return new(
            "Employee Time",
            "Track clock-in, clock-out, break minutes, hourly rates, notes, and payroll linkage per user.",
            "4 active",
            "28.5 hours",
            "1 open break",
            "employee-time",
            apiClient,
            [
                new AdminTaskItem("Open shifts", "Review staff who are currently clocked in.", "Open"),
                new AdminTaskItem("Edit time entry", "Correct clock-out time, break minutes, notes, or hourly rate.", "Edit"),
                new AdminTaskItem("Prepare payroll", "Close unclosed entries before processing payroll runs.", "Prepare")
            ],
            [
                new AdminRecordItem("Aya Reyes", "Clocked in 8:02 AM - Barista", "PHP 95/hr", "Open"),
                new AdminRecordItem("Ben Lim", "Clocked in 9:14 AM - Cashier", "PHP 90/hr", "Open"),
                new AdminRecordItem("Mika Santos", "Closed 7.75 hrs", "PHP 110/hr", "Closed")
            ]);
    }

    public static BackOfficeProcessViewModel CreatePayroll(IGroundControlApiClient apiClient)
    {
        return new(
            "Payroll",
            "Process payroll runs, regular hours, overtime hours, gross pay, status, and run notes.",
            "Ready",
            "128.5 regular",
            "6 overtime",
            "payroll",
            apiClient,
            [
                new AdminTaskItem("Create payroll run", "Select period start/end and collect closed time entries.", "New Run"),
                new AdminTaskItem("Review gross pay", "Validate regular hours, overtime, hourly rates, and gross pay.", "Review"),
                new AdminTaskItem("Finalize payroll", "Mark payroll run processed after approval.", "Finalize")
            ],
            [
                new AdminRecordItem("May 1 - May 15", "Processed payroll run", "PHP 14,920.00", "Processed"),
                new AdminRecordItem("May 16 - May 31", "Current payroll period", "PHP 0.00", "Draft"),
                new AdminRecordItem("Overtime Review", "6 hours pending approval", "PHP 855.00", "Pending")
            ]);
    }

    public static BackOfficeProcessViewModel CreateUsersRoles(IGroundControlApiClient apiClient)
    {
        return new(
            "Users and Roles",
            "Manage employee numbers, names, email, password/PIN setup, hourly rate, active status, and access roles.",
            "8 users",
            "4 roles",
            "1 inactive",
            "users",
            apiClient,
            [
                new AdminTaskItem("Create user", "Add employee profile, email, role, hourly rate, and active status.", "New User"),
                new AdminTaskItem("Set access", "Assign Admin, Manager, Cashier, or Barista permissions.", "Roles"),
                new AdminTaskItem("Reset PIN", "Prepare PIN reset flow for quick POS sign-in.", "Reset")
            ],
            [
                new AdminRecordItem("Admin User", "admin@groundcontrol.local - Admin", "PHP 150/hr", "Active"),
                new AdminRecordItem("Aya Reyes", "aya@groundcontrol.local - Barista", "PHP 95/hr", "Active"),
                new AdminRecordItem("Ben Lim", "ben@groundcontrol.local - Cashier", "PHP 90/hr", "Active")
            ]);
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
}
