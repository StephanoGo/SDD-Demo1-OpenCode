namespace PedidosPriority.Models;

public sealed class OrderDetailsViewModel
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Email { get; set; }
    public DateTime RegistrationDate { get; set; }
    public byte PriorityLevel { get; set; }
    public string? PriorityName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public bool HasHeader { get; set; }
    public List<OrderDetailItemDto> Lines { get; set; } = new();
}