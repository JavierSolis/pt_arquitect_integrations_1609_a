// src/IntegrationService.Application.Contracts/IEventHistoryRepository.cs
using IntegrationService.Domain;
namespace IntegrationService.Application.Contracts;

public interface IEventHistoryRepository
{
    Task AddAsync(TmsEvent tmsEvent);
}