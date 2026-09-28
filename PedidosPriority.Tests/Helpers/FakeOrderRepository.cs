using PedidosPriority.Data;
using PedidosPriority.Models;

namespace PedidosPriority.Tests.Helpers;

public sealed class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _queue = new();
    private int _nextId = 1;
    private OrderDetailsViewModel? _details;

    public IReadOnlyList<Order> InsertedOrders => _queue;
    public IReadOnlyList<OrderDetailRequest> InsertedItems { get; } = new List<OrderDetailRequest>();

    public void Seed(params Order[] orders)
    {
        _queue.AddRange(orders);
    }

    public void SeedDetails(OrderDetailsViewModel details)
    {
        _details = details;
    }

    public int InsertWithDetails(Order order, IEnumerable<OrderDetailRequest> items)
    {
        order.OrderId = _nextId++;
        _queue.Add(order);
        ((List<OrderDetailRequest>)InsertedItems).AddRange(items);
        return order.OrderId;
    }

    public OrderDetailsViewModel GetOrderDetails(int orderId)
    {
        return _details ?? new OrderDetailsViewModel { OrderId = orderId };
    }

    public Task<bool> DispatchOrderAsync(int orderId)
    {
        var order = _queue.FirstOrDefault(o => o.OrderId == orderId);

        if (order is null)
        {
            return Task.FromResult(false);
        }

        _queue.Remove(order);
        return Task.FromResult(true);
    }

    public IEnumerable<Order> GetPackingQueue() => _queue.ToList();
}