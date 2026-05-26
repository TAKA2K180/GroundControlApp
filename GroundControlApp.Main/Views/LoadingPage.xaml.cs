using GroundControlApp.Main.ViewModels;

namespace GroundControlApp.Main.Views;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingViewModel viewModel = new();
    private bool hasInitialized;

    public LoadingPage()
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (hasInitialized)
        {
            return;
        }

        hasInitialized = true;
        var minimumDisplayTime = Task.Delay(1400);
        await viewModel.InitializeAsync();
        await minimumDisplayTime;
        await Shell.Current.GoToAsync("//HomePage");
    }
}
