namespace GroundControlApp.Data.DTOs;

public sealed record MenuIngredientDto(
    Guid IngredientId,
    string? IngredientName,
    string? UnitOfMeasure,
    decimal Quantity);
