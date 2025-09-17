// src/IntegrationService.Infrastructure/InMemoryEventHistoryRepository.cs
using System.Collections.Concurrent;
using IntegrationService.Application.Contracts;
using IntegrationService.Domain;
using Microsoft.Extensions.Logging;

namespace IntegrationService.Infrastructure;

public class InMemoryEventHistoryRepository : IEventHistoryRepository
{
    private readonly ConcurrentBag<TmsEvent> _eventLog = new();
    private readonly ILogger<InMemoryEventHistoryRepository> _logger;

    public InMemoryEventHistoryRepository(ILogger<InMemoryEventHistoryRepository> logger)
    {
        _logger = logger;
    }

    public Task AddAsync(TmsEvent tmsEvent)
    {
        _eventLog.Add(tmsEvent);
        _logger.LogInformation("Evento '{Status}' para la orden '{OrderNumber}' registrado en el historial.", 
            tmsEvent.Status, 
            tmsEvent.Details.OrderNumber);
        return Task.CompletedTask;
    }
}