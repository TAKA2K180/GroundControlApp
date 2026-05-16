using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class StocksPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel =
        BackOfficeProcessViewModel.CreateStocks(AppServices.GetRequiredService<IStockService>());
    private bool hasLoaded;

    public StocksPage()
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
