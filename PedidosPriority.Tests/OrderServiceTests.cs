using PedidosPriority.Models;
using PedidosPriority.Services;
using PedidosPriority.Tests.Helpers;

namespace PedidosPriority.Tests;

public sealed class OrderServiceTests
{
    private readonly FakeOrderRepository _repository;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _repository = new FakeOrderRepository();
        _service = new OrderService(_repository);
    }

    private static OrderDetailRequest Item(int productId, int quantity, decimal unitPrice)
    {
        return new OrderDetailRequest { ProductId = productId, Quantity = quantity, UnitPrice = unitPrice };
    }

    [Fact]
    public void CreateOrder_ConEnvioExtra_AsignaPrioridadAlta()
    {
        var orderId = _service.CreateOrder(1, 15.50m, new[] { Item(1, 1, 100.00m) });

        var created = _repository.InsertedOrders.Single(o => o.OrderId == orderId);

        Assert.Equal(1, created.PriorityLevel);
    }

    [Fact]
    public void CreateOrder_ConEnvioGratis_AsignaPrioridadBaja()
    {
        var orderId = _service.CreateOrder(1, 0m, new[] { Item(1, 1, 100.00m) });

        var created = _repository.InsertedOrders.Single(o => o.OrderId == orderId);

        Assert.Equal(2, created.PriorityLevel);
    }

    [Fact]
    public void CreateOrder_ConCostoDeEnvioNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(1, -1m, new[] { Item(1, 1, 100m) }));
    }

    [Fact]
    public void CreateOrder_SinItems_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(1, 10m, System.Array.Empty<OrderDetailRequest>()));
    }

    [Fact]
    public void CreateOrder_ConCantidadInvalida_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(1, 10m, new[] { Item(1, 0, 100m) }));
    }

    [Fact]
    public void CreateOrder_ConPrecioNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(1, 10m, new[] { Item(1, 1, -5m) }));
    }

    [Fact]
    public void CreateOrder_ConClienteInvalido_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(0, 10m, new[] { Item(1, 1, 100m) }));
    }

    [Fact]
    public void CreateOrder_CalculaTotalCorrecto()
    {
        var orderId = _service.CreateOrder(1, 15.50m, new[] { Item(1, 1, 100.00m) });

        var created = _repository.InsertedOrders.Single(o => o.OrderId == orderId);

        Assert.Equal(115.50m, created.Total);
    }

    [Fact]
    public void CreateOrder_CalculaSubtotalComoSumaDeLineas()
    {
        var orderId = _service.CreateOrder(1, 5.00m, new[]
        {
            Item(1, 2, 10.50m),
            Item(2, 3, 5.00m)
        });

        var created = _repository.InsertedOrders.Single(o => o.OrderId == orderId);

        Assert.Equal(36.00m, created.Subtotal);
        Assert.Equal(41.00m, created.Total);
        Assert.Equal(2, _repository.InsertedItems.Count);
    }

    [Fact]
    public void CreateOrder_ConSubtotalDeLineasCero_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(1, 10m, new[] { Item(1, 1, 0m) }));
    }

    [Fact]
    public void GetPackingQueue_PrioridadAltaAntesQueBaja()
    {
        _repository.Seed(
            new Order { OrderId = 3, PriorityLevel = 2, RegistrationDate = new DateTime(2026, 9, 24, 10, 0, 0) },
            new Order { OrderId = 1, PriorityLevel = 2, RegistrationDate = new DateTime(2026, 9, 24, 8, 0, 0) },
            new Order { OrderId = 2, PriorityLevel = 1, RegistrationDate = new DateTime(2026, 9, 24, 9, 0, 0) });

        var queue = _service.GetPackingQueue().ToList();

        Assert.Equal(3, queue.Count);
        Assert.Equal(2, queue[0].OrderId);
        Assert.Equal(1, queue[1].OrderId);
        Assert.Equal(3, queue[2].OrderId);
    }

    [Fact]
    public void GetPackingQueue_MismaPrioridadOrdenaPorFechaDeRegistro()
    {
        _repository.Seed(
            new Order { OrderId = 2, PriorityLevel = 1, RegistrationDate = new DateTime(2026, 9, 24, 10, 0, 0) },
            new Order { OrderId = 1, PriorityLevel = 1, RegistrationDate = new DateTime(2026, 9, 24, 8, 0, 0) });

        var queue = _service.GetPackingQueue().ToList();

        Assert.Equal(2, queue.Count);
        Assert.Equal(1, queue[0].OrderId);
        Assert.Equal(2, queue[1].OrderId);
    }

    [Fact]
    public void GetOrderDetails_DevuelveDatosDelRepositorio()
    {
        var expected = new OrderDetailsViewModel
        {
            OrderId = 7,
            CustomerFullName = "Cliente Prueba",
            PriorityLevel = 1,
            Lines = { new OrderDetailItemDto { ProductName = "Producto 1", Quantity = 2, UnitPrice = 10.00m, LineTotal = 20.00m } }
        };

        _repository.SeedDetails(expected);

        var result = _service.GetOrderDetails(7);

        Assert.Equal(7, result.OrderId);
        Assert.Equal("Cliente Prueba", result.CustomerFullName);
        Assert.Single(result.Lines);
        Assert.Equal(20.00m, result.Lines[0].LineTotal);
    }
}