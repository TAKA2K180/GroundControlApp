using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GroundControlApp.Main.Models;
using GroundControlApp.Main.Services;
using GroundControlApp.Main.Views;

namespace GroundControlApp.Main.ViewModels;

public sealed class HomePageViewModel : ObservableObject
{
    private readonly AppIdentitySession identitySession;
    private readonly IReadOnlyCollection<HomeNavigationItem> navigationItems =
    [
        new("POS", "Open the register, build tickets, queue orders, and finish payments.", "Register", "Live", "//MainPage", AppRole.Guest, "#12B76A", "▣"),
        new("Orders", "Review pending, paid, and fulfilled customer orders.", "Queue", "Orders", nameof(OrdersPage), AppRole.Cashier, "#6941C6", "☷"),
        new("Employee Time", "Track shifts, active staff, time entries, and payroll readiness.", "People", "Shifts", nameof(EmployeeTimePage), AppRole.Barista, "#0E9384", "◷"),
        new("Inventory", "Receive stock batches, monitor ingredients, and act on low-stock alerts.", "Stockroom", "Levels", nameof(StocksPage), AppRole.Manager, "#B54708", "▦"),
        new("Menu", "Maintain menu items, categories, pricing, availability, and recipe ingredients.", "Catalog", "Items", nameof(MenuManagementPage), AppRole.Manager, "#087443", "☕"),
        new("Sales", "Audit receipts, totals, tender activity, and cashier attribution.", "Reporting", "PHP", nameof(SalesPage), AppRole.Manager, "#C11574", "₱"),
        new("Back Office", "Review operational metrics and manage every admin process from one workspace.", "Management", "Admin", nameof(AdminPanelPage), AppRole.Manager, "#175CD3", "◇"),
        new("Users", "Manage employees, account access, roles, and PIN reset workflows.", "Identity", "Roles", nameof(UsersRolesPage), AppRole.Admin, "#B42318", "♙")
    ];

    private readonly IReadOnlyCollection<string> footerTitles = ["POS", "Inventory", "Menu", "Sales", "Users"];

    public HomePageViewModel(AppIdentitySession identitySession)
    {
        this.identitySession = identitySession;
        identitySession.PropertyChanged += OnIdentitySessionChanged;

        NavigationItems = [];
        FooterItems = [];
        OpenNavigationItemCommand = new AsyncRelayCommand(OpenNavigationItemAsync);
        SignInCommand = new RelayCommand(() => identitySession.SignInTemporaryAdmin());
        SignOutCommand = new RelayCommand(() => identitySession.SignOut());
        RefreshNavigationItems();
    }

    public ObservableCollection<HomeNavigationItem> NavigationItems { get; }

    public ObservableCollection<HomeNavigationItem> FooterItems { get; }

    public ICommand OpenNavigationItemCommand { get; }

    public ICommand SignInCommand { get; }

    public ICommand SignOutCommand { get; }

    public string DisplayName => identitySession.DisplayName;

    public string RoleName => identitySession.RoleName;

    public bool IsSignedIn => identitySession.IsSignedIn;

    public bool IsGuestVisible => !identitySession.IsSignedIn;

    public bool IsSignedInVisible => identitySession.IsSignedIn;

    public HomeNavigationItem? FeaturedItem => NavigationItems.FirstOrDefault();

    public string FeaturedTitle => FeaturedItem?.Title ?? "POS";

    public string FeaturedMetric => identitySession.IsSignedIn ? "Ready" : "Guest";

    public string WelcomeText => identitySession.IsSignedIn
        ? $"Welcome back, {identitySession.DisplayName}"
        : "Start with POS or sign in for more tools";

    private async Task OpenNavigationItemAsync(object? parameter)
    {
        if (parameter is not HomeNavigationItem item ||
            !identitySession.CanAccess(item.RequiredRole))
        {
            return;
        }

        await Shell.Current.GoToAsync(item.Route);
    }

    private void RefreshNavigationItems()
    {
        NavigationItems.Clear();
        FooterItems.Clear();

        var availableItems = navigationItems
            .Where(item => identitySession.CanAccess(item.RequiredRole))
            .ToArray();

        foreach (var item in availableItems)
        {
            NavigationItems.Add(item);
        }

        foreach (var item in availableItems.Where(item => footerTitles.Contains(item.Title)))
        {
            FooterItems.Add(item);
        }

        OnPropertyChanged(nameof(FeaturedItem));
        OnPropertyChanged(nameof(FeaturedTitle));
        OnPropertyChanged(nameof(FeaturedMetric));
        OnPropertyChanged(nameof(WelcomeText));
    }

    private void OnIdentitySessionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppIdentitySession.DisplayName)
            or nameof(AppIdentitySession.Role)
            or nameof(AppIdentitySession.IsSignedIn))
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(RoleName));
            OnPropertyChanged(nameof(IsSignedIn));
            OnPropertyChanged(nameof(IsGuestVisible));
            OnPropertyChanged(nameof(IsSignedInVisible));
            OnPropertyChanged(nameof(FeaturedMetric));
            OnPropertyChanged(nameof(WelcomeText));
            RefreshNavigationItems();
        }
    }
}
