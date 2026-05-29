using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IUserService
{
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<UserDto> CreateUserAsync(SaveUserDto user, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateUserAsync(Guid id, SaveUserDto user, CancellationToken cancellationToken = default);

    Task<UserDto> ResetPinAsync(Guid id, ResetUserPinDto request, CancellationToken cancellationToken = default);

    Task<UserDto> AuthenticateAsync(AuthenticateUserDto request, CancellationToken cancellationToken = default);
}
