using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IUserService
{
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
}
