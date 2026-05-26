using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
        : this(AppServices.GetRequiredService<HomePageViewModel>())
    {
    }

    public HomePage(HomePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
