using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class OrdersPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel = BackOfficeProcessViewModel.CreateOrders();
    private bool hasLoaded;

    public OrdersPage()
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (hasLoaded) return;
        hasLoaded = true;
        await viewModel.LoadAsync();
    }
}
