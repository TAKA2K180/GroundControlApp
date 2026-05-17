using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class UserService : GroundControlApiServiceBase, IUserService
{
    public UserService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<UserDto>("api/v1/users", cancellationToken);
    }
}
