namespace PedidosPriority.Models;

public sealed class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public byte PriorityLevel { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? PriorityName { get; set; }
}