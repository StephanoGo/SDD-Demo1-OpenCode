using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PedidosPriority.Models;
using PedidosPriority.Services;

namespace PedidosPriority.Controllers;

public sealed class PedidosController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IOrderService _orderService;
    private readonly ICatalogService _catalogService;

    public PedidosController(IOrderService orderService, ICatalogService catalogService)
    {
        _orderService = orderService;
        _catalogService = catalogService;
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
            _orderService.CreateOrder(request.CustomerId, request.ShippingCost, request.Items);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", request);
        }

        return RedirectToAction("Index", "Entregapedidos");
    }

    [HttpGet]
    public IActionResult Detalle(int id)
    {
        if (id <= 0)
        {
            return View(new OrderDetailsViewModel());
        }

        var details = _orderService.GetOrderDetails(id);

        return View(details);
    }

    [HttpGet]
    public JsonResult Clientes()
    {
        return Json(_catalogService.GetActiveCustomers(), JsonOptions);
    }

    [HttpGet]
    public JsonResult Categorias()
    {
        return Json(_catalogService.GetCategories(), JsonOptions);
    }

    [HttpGet]
    public JsonResult Productos(int? categoriaId)
    {
        return Json(_catalogService.GetProductsByCategory(categoriaId), JsonOptions);
    }
}