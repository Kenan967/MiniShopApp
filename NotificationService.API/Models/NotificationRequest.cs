using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Models;

public class NotificationRequest
{
    public int OrderId { get; set; }

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}
