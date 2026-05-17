namespace GroundControlApp.Data.DTOs;

public sealed record PayrollRunDto(
    Guid Id,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    DateTime ProcessedAtUtc,
    int Status,
    decimal TotalRegularHours,
    decimal TotalOvertimeHours,
    decimal TotalGrossPay,
    string? Notes,
    IReadOnlyCollection<PayrollRunItemDto> Items);

public sealed record PayrollRunItemDto(
    Guid Id,
    Guid UserId,
    string? EmployeeNumber,
    string? EmployeeName,
    decimal RegularHours,
    decimal OvertimeHours,
    decimal HourlyRate,
    decimal RegularPay,
    decimal OvertimePay,
    decimal GrossPay,
    IReadOnlyCollection<Guid> TimeEntryIds);
