using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class SalesPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel =
        BackOfficeProcessViewModel.CreateSales();
    private bool hasLoaded;

    public SalesPage()
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
