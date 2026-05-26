namespace GroundControlApp.Data.DTOs;

public sealed record AddOnDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    IReadOnlyCollection<Guid> MenuIds);
