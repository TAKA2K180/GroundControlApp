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

    public Task<UserDto> CreateUserAsync(SaveUserDto user, CancellationToken cancellationToken = default)
    {
        return PostAsync<SaveUserDto, UserDto>("api/v1/users", user, null, cancellationToken);
    }

    public Task<UserDto> UpdateUserAsync(Guid id, SaveUserDto user, CancellationToken cancellationToken = default)
    {
        return PutAsync<SaveUserDto, UserDto>($"api/v1/users/{id}", user, cancellationToken);
    }

    public Task<UserDto> ResetPinAsync(Guid id, ResetUserPinDto request, CancellationToken cancellationToken = default)
    {
        return PutAsync<ResetUserPinDto, UserDto>($"api/v1/users/{id}/pin", request, cancellationToken);
    }

    public Task<UserDto> AuthenticateAsync(AuthenticateUserDto request, CancellationToken cancellationToken = default)
    {
        return PostAsync<AuthenticateUserDto, UserDto>("api/v1/users/authenticate", request, null, cancellationToken);
    }
}
