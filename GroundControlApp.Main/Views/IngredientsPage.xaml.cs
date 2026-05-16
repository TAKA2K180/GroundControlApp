using GroundControlApp.Data.Services;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class IngredientsPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel =
        BackOfficeProcessViewModel.CreateIngredients(AppServices.GetRequiredService<IGroundControlApiClient>());
    private bool hasLoaded;

    public IngredientsPage()
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
