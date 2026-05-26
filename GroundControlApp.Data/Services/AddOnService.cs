using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class AddOnService : GroundControlApiServiceBase, IAddOnService
{
    public AddOnService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<AddOnDto>> GetAddOnsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<AddOnDto>("api/v1/add-ons", cancellationToken);
    }
}
