using PedidosPriority.Models;

namespace PedidosPriority.Services;

public interface IOrderService
{
    int CreateOrder(int customerId, decimal shippingCost, IEnumerable<OrderDetailRequest> items);
    IEnumerable<Order> GetPackingQueue();
    OrderDetailsViewModel GetOrderDetails(int orderId);
}