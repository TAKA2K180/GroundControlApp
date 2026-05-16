using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IStockService
{
    Task<IReadOnlyCollection<StockDto>> GetStocksAsync(CancellationToken cancellationToken = default);
}
