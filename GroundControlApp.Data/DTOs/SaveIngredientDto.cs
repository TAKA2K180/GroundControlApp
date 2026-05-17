namespace GroundControlApp.Data.DTOs;

public sealed record SaveIngredientDto(
    string Name,
    string UnitOfMeasure,
    decimal ReorderLevel,
    decimal TargetLevel,
    decimal CurrentQuantity,
    decimal UnitCost,
    bool IsActive);
