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

    public Task<MenuDto> CreateMenuAsync(SaveMenuDto menu, CancellationToken cancellationToken = default)
    {
        return PostAsync<SaveMenuDto, MenuDto>("api/v1/menus", menu, null, cancellationToken);
    }

    public Task<MenuDto> UpdateMenuAsync(Guid id, SaveMenuDto menu, CancellationToken cancellationToken = default)
    {
        return PutAsync<SaveMenuDto, MenuDto>($"api/v1/menus/{id}", menu, cancellationToken);
    }
}
