namespace GroundControlApp.Data.DTOs;

public sealed record SaveMenuDto(
    string Name,
    string? Description,
    int Category,
    string Sku,
    decimal Price,
    bool IsAvailable,
    List<SaveMenuIngredientDto> Ingredients);

public sealed record SaveMenuIngredientDto(Guid IngredientId, decimal Quantity);
