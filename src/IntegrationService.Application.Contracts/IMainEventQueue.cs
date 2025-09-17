// src/IntegrationService.Application.Contracts/IMainEventQueue.cs
using IntegrationService.Domain;

namespace IntegrationService.Application.Contracts;

// Usaremos interfaces específicas para cada cola por claridad en la Inyección de Dependencias
public interface IMainEventQueue
{
    void Enqueue(TmsEvent tmsEvent);
    TmsEvent Dequeue(CancellationToken cancellationToken);
}