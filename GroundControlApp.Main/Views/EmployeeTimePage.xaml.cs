using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class EmployeeTimePage : ContentPage
{
    private readonly BackOfficeProcessViewModel viewModel = BackOfficeProcessViewModel.CreateEmployeeTime();
    private bool hasLoaded;

    public EmployeeTimePage()
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
