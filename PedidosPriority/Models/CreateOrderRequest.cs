using System.ComponentModel.DataAnnotations;

namespace PedidosPriority.Models;

public sealed class CreateOrderRequest
{
    [Required(ErrorMessage = "El cliente es obligatorio.")]
    [Display(Name = "Cliente (ID)")]
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del cliente es inválido.")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "El subtotal es obligatorio.")]
    [Display(Name = "Subtotal")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor a cero.")]
    public decimal Subtotal { get; set; }

    [Required(ErrorMessage = "El costo de envío es obligatorio.")]
    [Display(Name = "Costo de envío")]
    [Range(0, double.MaxValue, ErrorMessage = "El costo de envío no puede ser negativo.")]
    public decimal ShippingCost { get; set; }
}