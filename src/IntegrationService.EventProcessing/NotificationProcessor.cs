// src/IntegrationService.EventProcessing/NotificationProcessor.cs
using IntegrationService.Application.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationService.EventProcessing;

public class NotificationProcessor : BackgroundService
{
    private readonly ILogger<NotificationProcessor> _logger;
    private readonly INotificationQueue _notificationQueue;
    private readonly Dictionary<string, INotificationAdapter> _adapters;

    public NotificationProcessor(
        ILogger<NotificationProcessor> logger,
        INotificationQueue notificationQueue,
        IEnumerable<INotificationAdapter> adapters)
    {
        _logger = logger;
        _notificationQueue = notificationQueue;
        _adapters = adapters.ToDictionary(a => a.ClientType);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Processor está iniciando.");
        
        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var payload = _notificationQueue.Dequeue(stoppingToken);
                    _logger.LogInformation(
                        "<= Consumiendo evento de notificación para el cliente '{ClientName}' y la orden '{OrderNumber}'.",
                        payload.ClientName,
                        payload.OrderNumber);

                    // Patrón Adaptador: Seleccionamos la implementación correcta.
                    if (_adapters.TryGetValue(payload.ClientName, out var adapter))
                    {
                        await adapter.SendAsync(payload);
                    }
                    else if (_adapters.TryGetValue("GENERIC_EMAIL", out var genericAdapter))
                    {
                        _logger.LogInformation("No se encontró un adaptador específico para el cliente '{ClientName}'. Usando el adaptador genérico.", payload.ClientName);
                        await genericAdapter.SendAsync(payload);
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró ningún adaptador para el cliente '{ClientName}' ni un adaptador genérico.", payload.ClientName);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando un evento de la cola de notificaciones.");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }, stoppingToken);

        _logger.LogInformation("Notification Processor se ha detenido.");
    }
}