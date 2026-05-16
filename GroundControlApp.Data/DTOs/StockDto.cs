namespace GroundControlApp.Data.DTOs;

public sealed record StockDto(
    Guid Id,
    Guid IngredientId,
    string? IngredientName,
    string BatchNumber,
    decimal Quantity,
    decimal UnitCost,
    DateOnly? ExpirationDate,
    DateTime ReceivedAtUtc,
    string? SupplierName);
