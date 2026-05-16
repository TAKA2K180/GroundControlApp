using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IMenuService
{
    Task<IReadOnlyCollection<MenuDto>> GetMenusAsync(CancellationToken cancellationToken = default);
}
