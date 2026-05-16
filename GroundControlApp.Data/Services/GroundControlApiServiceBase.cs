using System.Net.Http.Json;
using System.Text.Json;

namespace GroundControlApp.Data.Services;

public abstract class GroundControlApiServiceBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;

    protected GroundControlApiServiceBase(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    protected async Task<IReadOnlyCollection<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<T>>(JsonOptions, cancellationToken)
            ?? [];
    }
}
