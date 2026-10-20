using NotificationService.API.Models;

namespace NotificationService.API.Services;

public interface IEmailNotificationService
{
    Task SendOrderConfirmationAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
