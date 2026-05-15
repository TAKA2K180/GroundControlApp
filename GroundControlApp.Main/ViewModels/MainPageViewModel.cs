using System.Collections.ObjectModel;
using System.Windows.Input;
using GroundControlApp.Main.Models;
using GroundControlApp.Main.Services;

namespace GroundControlApp.Main.ViewModels;

public sealed class MainPageViewModel : ObservableObject
{
    private const string TestAccountEmail = "admin@groundcontrol.local";
    private const string TestAccountSecret = "1234";

    private readonly GroundControlApiClient apiClient = new();
    private string searchText = string.Empty;
    private string selectedTender = "Card";
    private string shiftStatus = "Point of Sale";
    private string syncStatus = "Ready for API-backed inventory.";
    private string ticketStatus = "Dine in - Table 4";
    private string signInEmail = TestAccountEmail;
    private string signInSecret = string.Empty;
    private string signInMessage = string.Empty;
    private bool isSignInVisible;
    private bool isSignedIn;

    public MainPageViewModel()
    {
        Categories =
        [
            new PosCategory("All", true),
            new PosCategory("Arabica"),
            new PosCategory("Robusta"),
            new PosCategory("Liberica"),
            new PosCategory("Excelsa")
        ];

        Products = [];

        CartItems =
        [
            new CartItem("Benguet Arabica", "250g whole beans", 2, 280.00m),
            new CartItem("Barako Liberica", "fine grind", 1, 260.00m),
            new CartItem("Espresso Crema Blend", "500g whole beans", 1, 350.00m)
        ];

        KeypadValues = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "CLR", "0", "."];

        SelectCategoryCommand = new RelayCommand(_ => { });
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

    public decimal Subtotal => CartItems.Sum(item => item.Total);

    public decimal Tax => decimal.Round(Subtotal * 0.0825m, 2);

    public decimal Total => Subtotal + Tax;

    public string SubtotalDisplay => $"PHP {Subtotal:N2}";

    public string TaxDisplay => $"PHP {Tax:N2}";

    public string TotalDisplay => $"PHP {Total:N2}";

    public string ChargeText => $"Charge {TotalDisplay}";

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            SyncStatus = "Loading menu catalog from API.";
            var menus = await apiClient.GetMenusAsync(cancellationToken);
            Products.Clear();

            foreach (var menu in menus.Where(menu => menu.IsAvailable))
            {
                Products.Add(new PosProduct(menu.Id, menu.Name, GetMenuCategoryName(menu.Category), menu.Price));
            }

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
        CartItems.Add(new CartItem(product.Name, string.Empty, 1, Math.Max(product.Price, 0)));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(SubtotalDisplay));
        OnPropertyChanged(nameof(TaxDisplay));
        OnPropertyChanged(nameof(TotalDisplay));
        OnPropertyChanged(nameof(ChargeText));
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
