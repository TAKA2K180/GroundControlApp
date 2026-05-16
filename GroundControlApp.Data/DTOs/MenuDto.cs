namespace GroundControlApp.Data.DTOs;

public sealed record MenuDto(
    Guid Id,
    string Name,
    string? Description,
    int Category,
    string Sku,
    decimal Price,
    bool IsAvailable,
    IReadOnlyCollection<MenuIngredientDto>? Ingredients);
