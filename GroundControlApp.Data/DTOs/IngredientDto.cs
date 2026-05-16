namespace GroundControlApp.Data.DTOs;

public sealed record IngredientDto(
    Guid Id,
    string Name,
    string UnitOfMeasure,
    decimal ReorderLevel,
    decimal TargetLevel,
    decimal CurrentQuantity,
    decimal UnitCost,
    bool IsActive);
