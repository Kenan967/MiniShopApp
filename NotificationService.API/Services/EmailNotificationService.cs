using System.Net;
using System.Net.Mail;
using NotificationService.API.Models;

namespace NotificationService.API.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:SmtpHost"];
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var fromAddress = _configuration["Email:FromAddress"];
        var fromName = _configuration["Email:FromName"] ?? "MiniShop";
        var port = _configuration.GetValue<int?>("Email:SmtpPort") ?? 587;
        var enableSsl = _configuration.GetValue<bool?>("Email:EnableSsl") ?? true;

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fromAddress))
        {
            throw new InvalidOperationException(
                "Email SMTP settings are missing. Configure Email:SmtpHost, Email:Username, Email:Password and Email:FromAddress.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = $"MiniShop order confirmation - Order #{request.OrderId}",
            IsBodyHtml = false,
            Body =
                $"Hello,\n\n" +
                $"Your order has been created successfully.\n\n" +
                $"Order number: #{request.OrderId}\n" +
                $"Product: {request.ProductName}\n" +
                $"Quantity: {request.Quantity}\n" +
                $"Total: {request.TotalPrice:F2} AZN\n\n" +
                $"Thank you for shopping with MiniShop."
        };

        message.To.Add(new MailAddress(request.Email));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15000
        };

        // SmtpClient does not expose a CancellationToken overload in .NET 8.
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message);

        _logger.LogInformation(
            "Order confirmation email sent for order {OrderId} to {Email}.",
            request.OrderId,
            request.Email);
    }
}
