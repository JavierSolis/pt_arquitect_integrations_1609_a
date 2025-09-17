// src/IntegrationService.Application.Contracts/INotificationQueue.cs
namespace IntegrationService.Application.Contracts;

public interface INotificationQueue
{
    void Enqueue(NotificationPayload payload);
    NotificationPayload Dequeue(CancellationToken cancellationToken);
}