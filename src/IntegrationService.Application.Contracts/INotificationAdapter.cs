// src/IntegrationService.Application.Contracts/INotificationAdapter.cs
namespace IntegrationService.Application.Contracts;

// Representa los datos que necesita cualquier notificador
public record NotificationPayload(string OrderNumber, string ClientName, string NewStatus);

public interface INotificationAdapter
{
    // Una propiedad para identificar qué adaptador es
    string ClientType { get; }
    Task SendAsync(NotificationPayload payload);
}