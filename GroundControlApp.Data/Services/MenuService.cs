using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class MenuService : GroundControlApiServiceBase, IMenuService
{
    public MenuService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<MenuDto>> GetMenusAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<MenuDto>("api/v1/menus", cancellationToken);
    }
}
