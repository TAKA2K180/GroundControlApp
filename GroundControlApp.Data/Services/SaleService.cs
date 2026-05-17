using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class SaleService : GroundControlApiServiceBase, ISaleService
{
    public SaleService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<SaleDto>> GetSalesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<SaleDto>("api/v1/sales", cancellationToken);
    }
}
