using PedidosPriority.Models;

namespace PedidosPriority.Data;

public interface IOrderRepository
{
    int InsertWithDetails(Order order, IEnumerable<OrderDetailRequest> items);
    IEnumerable<Order> GetPackingQueue();
    OrderDetailsViewModel GetOrderDetails(int orderId);
    Task<bool> DispatchOrderAsync(int orderId);
}