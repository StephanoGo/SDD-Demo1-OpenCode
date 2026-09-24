using PedidosPriority.Models;

namespace PedidosPriority.Services;

public interface IOrderService
{
    int CreateOrder(int customerId, decimal subtotal, decimal shippingCost);
    IEnumerable<Order> GetPackingQueue();
}