using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class OrderService : GroundControlApiServiceBase, IOrderService
{
    public OrderService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<OrderDto>("api/v1/orders", cancellationToken);
    }

    public Task<OrderDto> CreateOrderAsync(
        CreateOrderDto order,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateOrderDto, OrderDto>(
            "api/v1/orders",
            order,
            CreateIdempotencyHeaders(idempotencyKey),
            cancellationToken);
    }

    public Task<OrderDto> PayOrderAsync(
        Guid orderId,
        PayOrderDto payment,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<PayOrderDto, OrderDto>(
            $"api/v1/orders/{orderId}/pay",
            payment,
            CreateIdempotencyHeaders(idempotencyKey),
            cancellationToken);
    }

    private static IReadOnlyDictionary<string, string> CreateIdempotencyHeaders(string idempotencyKey)
    {
        return new Dictionary<string, string>
        {
            ["Idempotency-Key"] = idempotencyKey
        };
    }
}
