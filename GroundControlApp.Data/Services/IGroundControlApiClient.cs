namespace GroundControlApp.Data.Services;

public interface IGroundControlApiClient
{
    Task<IReadOnlyCollection<MenuDto>> GetMenusAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IngredientDto>> GetIngredientsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockDto>> GetStocksAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TimeEntryDto>> GetTimeEntriesAsync(CancellationToken cancellationToken = default);
}
