using PedidosPriority.Models;

namespace PedidosPriority.Data;

public interface IOrderRepository
{
    int Insert(Order order);
    IEnumerable<Order> GetPackingQueue();
}