using GroundControlApp.Data.Services;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class UsersRolesPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel =
        BackOfficeProcessViewModel.CreateUsersRoles(AppServices.GetRequiredService<IGroundControlApiClient>());
    private bool hasLoaded;

    public UsersRolesPage()
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
