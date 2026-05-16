using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class StockService : GroundControlApiServiceBase, IStockService
{
    public StockService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<StockDto>> GetStocksAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<StockDto>("api/v1/stocks", cancellationToken);
    }
}
