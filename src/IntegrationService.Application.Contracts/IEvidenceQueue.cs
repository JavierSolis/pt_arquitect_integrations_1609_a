// src/IntegrationService.Application.Contracts/IEvidenceQueue.cs
using IntegrationService.Domain;
namespace IntegrationService.Application.Contracts;

// Usaremos un record simple para pasar solo la info necesaria a la cola de evidencias
public record EvidenceProcessingRequest(string OrderNumber, Evidence Evidence);

public interface IEvidenceQueue
{
    void Enqueue(EvidenceProcessingRequest request);
    EvidenceProcessingRequest Dequeue(CancellationToken cancellationToken);
}