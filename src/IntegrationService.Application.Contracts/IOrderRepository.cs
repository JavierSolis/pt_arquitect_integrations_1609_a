// src/IntegrationService.Application.Contracts/IOrderRepository.cs
using IntegrationService.Domain;

namespace IntegrationService.Application.Contracts;

public interface IOrderRepository
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task UpdateAsync(Order order);
}