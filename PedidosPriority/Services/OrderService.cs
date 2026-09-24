using PedidosPriority.Data;
using PedidosPriority.Models;

namespace PedidosPriority.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public int CreateOrder(int customerId, decimal subtotal, decimal shippingCost)
    {
        if (customerId <= 0)
        {
            throw new ArgumentException("El identificador del cliente es inválido.", nameof(customerId));
        }

        if (subtotal <= 0)
        {
            throw new ArgumentException("El subtotal debe ser mayor a cero.", nameof(subtotal));
        }

        if (shippingCost < 0)
        {
            throw new ArgumentException("El costo de envío no puede ser negativo.", nameof(shippingCost));
        }

        var order = new Order
        {
            CustomerId = customerId,
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            Total = subtotal + shippingCost,
            PriorityLevel = shippingCost > 0 ? (byte)1 : (byte)2
        };

        return _repository.Insert(order);
    }

    public IEnumerable<Order> GetPackingQueue()
    {
        return _repository.GetPackingQueue()
            .OrderBy(o => o.PriorityLevel)
            .ThenBy(o => o.RegistrationDate)
            .ThenBy(o => o.OrderId);
    }
}