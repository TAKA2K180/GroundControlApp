using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IMenuService
{
    Task<IReadOnlyCollection<MenuDto>> GetMenusAsync(CancellationToken cancellationToken = default);

    Task<MenuDto> CreateMenuAsync(SaveMenuDto menu, CancellationToken cancellationToken = default);

    Task<MenuDto> UpdateMenuAsync(Guid id, SaveMenuDto menu, CancellationToken cancellationToken = default);
}
