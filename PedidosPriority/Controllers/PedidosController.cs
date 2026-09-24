using Microsoft.AspNetCore.Mvc;
using PedidosPriority.Models;
using PedidosPriority.Services;

namespace PedidosPriority.Controllers;

public sealed class PedidosController : Controller
{
    private readonly IOrderService _orderService;

    public PedidosController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        return View(new CreateOrderRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", request);
        }

        try
        {
            _orderService.CreateOrder(request.CustomerId, request.Subtotal, request.ShippingCost);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", request);
        }

        return RedirectToAction("Index", "Entregapedidos");
    }
}