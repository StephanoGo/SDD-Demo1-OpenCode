using PedidosPriority.Data;
using PedidosPriority.Models;

namespace PedidosPriority.Tests.Helpers;

public sealed class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _queue = new();
    private int _nextId = 1;

    public IReadOnlyList<Order> InsertedOrders => _queue;

    public void Seed(params Order[] orders)
    {
        _queue.AddRange(orders);
    }

    public int Insert(Order order)
    {
        order.OrderId = _nextId++;
        _queue.Add(order);
        return order.OrderId;
    }

    public IEnumerable<Order> GetPackingQueue() => _queue.ToList();
}