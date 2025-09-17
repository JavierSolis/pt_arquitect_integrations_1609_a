// src/IntegrationService.Infrastructure/NotificationAdapters.cs
using IntegrationService.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace IntegrationService.Infrastructure;

public class ClientAWebhookAdapter : INotificationAdapter
{
    private readonly ILogger<ClientAWebhookAdapter> _logger;
    public string ClientType => "TIENDAS PERUANAS S.A."; // Coincide con el clientName del JSON de ejemplo

    public ClientAWebhookAdapter(ILogger<ClientAWebhookAdapter> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(NotificationPayload payload)
    {
        _logger.LogInformation(
            "[ADAPTADOR CLIENTE A - Webhook] Enviando notificación para la orden {OrderNumber}. Nuevo estado: {NewStatus}",
            payload.OrderNumber,
            payload.NewStatus);
        // Aquí iría la lógica real con HttpClient para llamar al webhook del cliente
        return Task.CompletedTask;
    }
}

public class GenericEmailAdapter : INotificationAdapter
{
    private readonly ILogger<GenericEmailAdapter> _logger;
    public string ClientType => "GENERIC_EMAIL"; // Un tipo para clientes que no tienen un adaptador específico

    public GenericEmailAdapter(ILogger<GenericEmailAdapter> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(NotificationPayload payload)
    {
        _logger.LogInformation(
            "[ADAPTADOR GENÉRICO - Email] Enviando email de notificación para la orden {OrderNumber} del cliente {ClientName}. Nuevo estado: {NewStatus}",
            payload.OrderNumber,
            payload.ClientName,
            payload.NewStatus);
        // Aquí iría la lógica real para enviar un email
        return Task.CompletedTask;
    }
}