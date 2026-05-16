using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class MenuManagementPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel =
        BackOfficeProcessViewModel.CreateMenuManagement(AppServices.GetRequiredService<IMenuService>());
    private bool hasLoaded;

    public MenuManagementPage()
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
