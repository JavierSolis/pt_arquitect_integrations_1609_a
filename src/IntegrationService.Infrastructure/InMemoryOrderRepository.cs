// src/IntegrationService.Infrastructure/InMemoryOrderRepository.cs
using System.Collections.Concurrent;
using IntegrationService.Application.Contracts;
using IntegrationService.Domain;
using Microsoft.Extensions.Logging;

namespace IntegrationService.Infrastructure;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ILogger<InMemoryOrderRepository> _logger;
    private readonly ConcurrentDictionary<string, Order> _orders = new();

    public InMemoryOrderRepository(ILogger<InMemoryOrderRepository> logger)
    {
        _logger = logger;
        // Pre-poblamos con datos de prueba
        var order1 = new Order("2500000006-01", "PLANNING");
        _orders.TryAdd(order1.Id, order1);
        
        var order2 = new Order("3500000001-01", "STARTED", 2);
        _orders.TryAdd(order2.Id, order2);

        var order3 = new Order("4500000008-01", "DELIVERED", 1);
        _orders.TryAdd(order3.Id, order3);
    }

    public Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        _logger.LogInformation("[OMS SIM] Buscando orden '{OrderNumber}'...", orderNumber);
        _orders.TryGetValue(orderNumber, out var order);
        if (order is null)
        {
            _logger.LogWarning("[OMS SIM] Orden '{OrderNumber}' no encontrada.", orderNumber);
        }
        else
        {
            _logger.LogInformation("[OMS SIM] Orden '{OrderNumber}' encontrada con estado '{Status}'.", orderNumber, order.Status);
        }
        return Task.FromResult(order);
    }

    public Task UpdateAsync(Order order)
    {
        _logger.LogInformation("[OMS SIM] Actualizando orden '{OrderNumber}' con estado '{Status}' y {VisitCount} visitas.", order.Id, order.Status, order.VisitCount);
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}