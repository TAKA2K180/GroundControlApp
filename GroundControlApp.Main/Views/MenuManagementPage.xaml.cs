using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class MenuManagementPage : ContentPage
{
    private readonly MenuManagementViewModel viewModel =
        new(
            AppServices.GetRequiredService<IMenuService>(),
            AppServices.GetRequiredService<IIngredientService>());

    public MenuManagementPage()
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadAsync();
    }
}
