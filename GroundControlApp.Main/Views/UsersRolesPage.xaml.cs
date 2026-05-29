using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class UsersRolesPage : ContentPage
{
    private readonly UserManagementViewModel viewModel =
        new(AppServices.GetRequiredService<IUserService>());

    public UsersRolesPage()
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
