namespace GroundControlApp.Main.Services;

public sealed class AppStartupLoader
{
    public async Task LoadAsync(IProgress<StartupLoadProgress> progress, CancellationToken cancellationToken = default)
    {
        progress.Report(new StartupLoadProgress("Preparing workspace", 0.15));
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        progress.Report(new StartupLoadProgress("Loading theme resources", 0.35));
        LoadApplicationResources();
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        progress.Report(new StartupLoadProgress("Loading packaged assets", 0.65));
        await LoadPackagedAssetsAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        progress.Report(new StartupLoadProgress("Starting point of sale", 0.9));
        await Task.Delay(150, cancellationToken);

        progress.Report(new StartupLoadProgress("Ready", 1));
    }

    private static void LoadApplicationResources()
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
        {
            return;
        }

        _ = resources.TryGetValue("PosTileStyle", out _);
        _ = resources.TryGetValue("PosKeypadButtonStyle", out _);
        _ = resources.TryGetValue("Primary", out _);
        _ = resources.TryGetValue("Gray950", out _);
    }

    private static async Task LoadPackagedAssetsAsync(CancellationToken cancellationToken)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync("AboutAssets.txt");
        using var reader = new StreamReader(stream);
        _ = await reader.ReadToEndAsync(cancellationToken);
    }
}

public sealed record StartupLoadProgress(string Message, double Progress);
