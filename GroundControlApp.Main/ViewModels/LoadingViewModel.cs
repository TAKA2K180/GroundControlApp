using GroundControlApp.Main.Services;

namespace GroundControlApp.Main.ViewModels;

public sealed class LoadingViewModel : ObservableObject
{
    private readonly AppStartupLoader startupLoader;
    private double progress;
    private string statusMessage = "Preparing workspace";
    private bool isBusy = true;

    public LoadingViewModel()
        : this(new AppStartupLoader())
    {
    }

    public LoadingViewModel(AppStartupLoader startupLoader)
    {
        this.startupLoader = startupLoader;
    }

    public double Progress
    {
        get => progress;
        private set => SetProperty(ref progress, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;

        var loadProgress = new Progress<StartupLoadProgress>(update =>
        {
            StatusMessage = update.Message;
            Progress = update.Progress;
        });

        await startupLoader.LoadAsync(loadProgress, cancellationToken);
        IsBusy = false;
    }
}
