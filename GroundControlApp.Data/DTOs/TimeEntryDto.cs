namespace GroundControlApp.Data.DTOs;

public sealed record TimeEntryDto(
    Guid Id,
    Guid UserId,
    string? EmployeeNumber,
    string? EmployeeName,
    DateTime ClockInAtUtc,
    DateTime? ClockOutAtUtc,
    int BreakMinutes,
    decimal HourlyRate,
    decimal HoursWorked,
    bool IsClosed,
    bool IsPaid,
    string? Notes);
