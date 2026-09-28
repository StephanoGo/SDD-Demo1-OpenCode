namespace PedidosPriority.Models;

public sealed class ActiveCustomerDto
{
    public int CustomerId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Email { get; set; }
}