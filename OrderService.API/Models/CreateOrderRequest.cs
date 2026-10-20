using System.ComponentModel.DataAnnotations;

namespace OrderService.API.Models;

public class CreateOrderRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Product ID must be greater than zero.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
}
