using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Models;
using NotificationService.API.Services;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IEmailNotificationService emailService,
        ILogger<NotificationsController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Send(
        [FromBody] NotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0)
            return BadRequest("Invalid order ID.");

        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        try
        {
            await _emailService.SendOrderConfirmationAsync(request, cancellationToken);

            return Ok(new
            {
                message = "Order confirmation email sent successfully.",
                orderId = request.OrderId,
                email = request.Email
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Email service is not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "Email service is not configured. Check NotificationService settings.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not send confirmation email for order {OrderId}.", request.OrderId);
            return StatusCode(StatusCodes.Status502BadGateway,
                "Order was created, but the confirmation email could not be sent.");
        }
    }
}
