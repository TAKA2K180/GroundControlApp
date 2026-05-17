namespace GroundControlApp.Data.DTOs;

public sealed record SaleDto(
    Guid Id,
    string ReceiptNumber,
    DateTime SoldAtUtc,
    int Status,
    decimal GrandTotal,
    string? CustomerName,
    string? CashierName,
    IReadOnlyCollection<string> PaymentMethods);
