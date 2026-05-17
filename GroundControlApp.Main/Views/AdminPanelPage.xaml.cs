using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class AdminPanelPage : ContentPage
{
    private readonly AdminPanelViewModel viewModel;

    public AdminPanelPage()
    {
        InitializeComponent();
        viewModel = AppServices.GetRequiredService<AdminPanelViewModel>();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadAsync();
    }
}
