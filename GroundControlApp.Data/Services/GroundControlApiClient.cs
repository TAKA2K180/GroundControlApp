using System.Net.Http.Json;
using System.Text.Json;

namespace GroundControlApp.Data.Services;

public sealed class GroundControlApiClient : IGroundControlApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;

    public GroundControlApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<MenuDto>> GetMenusAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<MenuDto>("api/v1/menus", cancellationToken);
    }

    public async Task<IReadOnlyCollection<IngredientDto>> GetIngredientsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<IngredientDto>("api/v1/ingredients", cancellationToken);
    }

    public async Task<IReadOnlyCollection<StockDto>> GetStocksAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<StockDto>("api/v1/stocks", cancellationToken);
    }

    public async Task<IReadOnlyCollection<TimeEntryDto>> GetTimeEntriesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<TimeEntryDto>("api/v1/employee-time/time-entries", cancellationToken);
    }

    private async Task<IReadOnlyCollection<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<T>>(JsonOptions, cancellationToken)
            ?? [];
    }
}

public sealed record MenuDto(
    Guid Id,
    string Name,
    string? Description,
    int Category,
    string Sku,
    decimal Price,
    bool IsAvailable,
    IReadOnlyCollection<MenuIngredientDto>? Ingredients);

public sealed record MenuIngredientDto(
    Guid IngredientId,
    string? IngredientName,
    string? UnitOfMeasure,
    decimal Quantity);

public sealed record IngredientDto(
    Guid Id,
    string Name,
    string UnitOfMeasure,
    decimal ReorderLevel,
    decimal TargetLevel,
    decimal CurrentQuantity,
    decimal UnitCost,
    bool IsActive);

public sealed record StockDto(
    Guid Id,
    Guid IngredientId,
    string? IngredientName,
    string BatchNumber,
    decimal Quantity,
    decimal UnitCost,
    DateOnly? ExpirationDate,
    DateTime ReceivedAtUtc,
    string? SupplierName);

public sealed record TimeEntryDto(
    Guid Id,
    Guid UserId,
    string? EmployeeNumber,
    string? EmployeeName,
    DateTime ClockInAtUtc,
    DateTime? ClockOutAtUtc,
    int BreakMinutes,
    decimal HourlyRate,
    decimal HoursWorked,
    bool IsClosed,
    bool IsPaid,
    string? Notes);
