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

    [HttpPost]
    [Route("api/pedidos/despachar/{id}")]
    public async Task<IActionResult> Despachar(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { success = false, orderId = id });
        }

        var dispatched = await _orderService.DispatchOrderAsync(id);

        if (!dispatched)
        {
            return BadRequest(new { success = false, orderId = id });
        }

        return Ok(new { success = true, orderId = id });
    }
}