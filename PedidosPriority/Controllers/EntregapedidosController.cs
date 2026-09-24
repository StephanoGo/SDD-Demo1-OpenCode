using Microsoft.AspNetCore.Mvc;
using PedidosPriority.Models;
using PedidosPriority.Services;

namespace PedidosPriority.Controllers;

public sealed class EntregapedidosController : Controller
{
    private readonly IOrderService _orderService;

    public EntregapedidosController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        var queue = _orderService.GetPackingQueue();

        return View(queue);
    }
}