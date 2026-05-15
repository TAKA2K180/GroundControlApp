using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class PayrollPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel = BackOfficeProcessViewModel.CreatePayroll();
    private bool hasLoaded;

    public PayrollPage()
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
