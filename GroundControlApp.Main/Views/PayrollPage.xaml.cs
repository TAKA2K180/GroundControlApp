using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class PayrollPage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel =
        BackOfficeProcessViewModel.CreatePayroll(AppServices.GetRequiredService<ITimeEntryService>());

    public PayrollPage()
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
