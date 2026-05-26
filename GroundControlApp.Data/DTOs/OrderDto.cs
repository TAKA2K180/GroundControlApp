namespace GroundControlApp.Data.DTOs;

public sealed record CreateOrderDto(
    string? InvoiceNumber,
    DateTime? DueAtUtc,
    string? CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string? Notes,
    Guid? UserId,
    List<CreateOrderDetailDto> Details);

public sealed record CreateOrderDetailDto(
    Guid? MenuId,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal LineTotal,
    string? Description,
    string? Notes,
    List<CreateOrderDetailAddOnDto>? AddOns);

public sealed record CreateOrderDetailAddOnDto(Guid? AddOnId, string Name, decimal Price);

public sealed record PayOrderDto(decimal AmountPaid, Guid? UserId);

public sealed record OrderDto(
    Guid Id,
    string InvoiceNumber,
    int Status,
    DateTime OrderedAtUtc,
    DateTime? DueAtUtc,
    string? CustomerName,
    decimal GrandTotal,
    decimal AmountPaid,
    DateTime? InventoryDeductedAtUtc,
    IReadOnlyCollection<OrderDetailDto> Details);

public sealed record OrderDetailDto(
    Guid Id,
    Guid? MenuId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    IReadOnlyCollection<OrderDetailAddOnDto> AddOns);

public sealed record OrderDetailAddOnDto(Guid Id, Guid? AddOnId, string Name, decimal Price);
