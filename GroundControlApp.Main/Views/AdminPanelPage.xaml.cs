using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class AdminPanelPage : ContentPage
{
    public AdminPanelPage()
    {
        InitializeComponent();
        BindingContext = AppServices.GetRequiredService<AdminPanelViewModel>();
    }
}
