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

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        IReadOnlyDictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        using var response = await httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("API returned an empty response.");
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PutAsJsonAsync(path, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("API returned an empty response.");
    }
}
