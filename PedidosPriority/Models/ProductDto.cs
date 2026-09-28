namespace PedidosPriority.Models;

public sealed class ProductDto
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; }
}