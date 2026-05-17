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
