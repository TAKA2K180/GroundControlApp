namespace GroundControlApp.Data.DTOs;

public sealed record UserDto(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    int Role,
    decimal HourlyRate,
    bool IsActive);

public sealed record SaveUserDto(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    int Role,
    decimal HourlyRate,
    bool IsActive,
    string? Pin);

public sealed record ResetUserPinDto(string Pin);

public sealed record AuthenticateUserDto(string Identifier, string Pin);
