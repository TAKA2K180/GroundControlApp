using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class IngredientsPage : ContentPage
{
    private readonly IngredientsManagementViewModel viewModel =
        new(AppServices.GetRequiredService<IIngredientService>());

    public IngredientsPage()
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
