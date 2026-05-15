using System.Collections.ObjectModel;
using System.Windows.Input;
using GroundControlApp.Main.Models;
using GroundControlApp.Main.Views;

namespace GroundControlApp.Main.ViewModels;

public sealed class AdminPanelViewModel : ObservableObject
{
    public AdminPanelViewModel()
    {
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

    public ICommand OpenProcessCommand { get; }
}
