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

    public int CreateOrder(int customerId, decimal shippingCost, IEnumerable<OrderDetailRequest> items)
    {
        if (customerId <= 0)
        {
            throw new ArgumentException("El identificador del cliente es inválido.", nameof(customerId));
        }

        if (shippingCost < 0)
        {
            throw new ArgumentException("El costo de envío no puede ser negativo.", nameof(shippingCost));
        }

        var itemList = items?.ToList() ?? new List<OrderDetailRequest>();

        if (itemList.Count == 0)
        {
            throw new ArgumentException("Debe agregarse al menos un producto al pedido.", nameof(items));
        }

        foreach (var item in itemList)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(items));
            }

            if (item.UnitPrice < 0)
            {
                throw new ArgumentException("El precio unitario no puede ser negativo.", nameof(items));
            }
        }

        var subtotal = itemList.Sum(i => i.Quantity * i.UnitPrice);

        if (subtotal <= 0)
        {
            throw new ArgumentException("El subtotal debe ser mayor a cero.", nameof(items));
        }

        var order = new Order
        {
            CustomerId = customerId,
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            Total = subtotal + shippingCost,
            PriorityLevel = shippingCost > 0 ? (byte)1 : (byte)2
        };

        return _repository.InsertWithDetails(order, itemList);
    }

    public Task<bool> DispatchOrderAsync(int orderId)
    {
        return _repository.DispatchOrderAsync(orderId);
    }

    public OrderDetailsViewModel GetOrderDetails(int orderId)
    {
        return _repository.GetOrderDetails(orderId);
    }

    public IEnumerable<Order> GetPackingQueue()
    {
        return _repository.GetPackingQueue()
            .OrderBy(o => o.PriorityLevel)
            .ThenBy(o => o.RegistrationDate)
            .ThenBy(o => o.OrderId);
    }
}