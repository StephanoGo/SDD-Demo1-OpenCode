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

    [Fact]
    public void CreateOrder_ConEnvioExtra_AsignaPrioridadAlta()
    {
        var orderId = _service.CreateOrder(1, 100.00m, 15.50m);

        var created = _repository.InsertedOrders.Single(o => o.OrderId == orderId);

        Assert.Equal(1, created.PriorityLevel);
    }

    [Fact]
    public void CreateOrder_ConEnvioGratis_AsignaPrioridadBaja()
    {
        var orderId = _service.CreateOrder(1, 100.00m, 0m);

        var created = _repository.InsertedOrders.Single(o => o.OrderId == orderId);

        Assert.Equal(2, created.PriorityLevel);
    }

    [Fact]
    public void CreateOrder_ConCostoDeEnvioNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(1, 100.00m, -1m));
    }

    [Fact]
    public void CreateOrder_ConSubtotalCero_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(1, 0m, 10m));
    }

    [Fact]
    public void CreateOrder_ConClienteInvalido_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateOrder(0, 100m, 10m));
    }

    [Fact]
    public void CreateOrder_CalculaTotalCorrecto()
    {
        var orderId = _service.CreateOrder(1, 100.00m, 15.50m);

        var created = _repository.InsertedOrders.Single(o => o.OrderId == orderId);

        Assert.Equal(115.50m, created.Total);
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
}