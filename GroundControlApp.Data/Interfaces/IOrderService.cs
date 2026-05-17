using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyCollection<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default);

    Task<OrderDto> CreateOrderAsync(
        CreateOrderDto order,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<OrderDto> PayOrderAsync(
        Guid orderId,
        PayOrderDto payment,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
