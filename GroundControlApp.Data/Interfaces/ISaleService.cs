using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface ISaleService
{
    Task<IReadOnlyCollection<SaleDto>> GetSalesAsync(CancellationToken cancellationToken = default);
}
