// src/IntegrationService.Infrastructure/InMemoryQueues.cs
using System.Collections.Concurrent;
using IntegrationService.Application.Contracts;
using IntegrationService.Domain;

namespace IntegrationService.Infrastructure;

// --- Cola Principal ---
public class InMemoryMainEventQueue : IMainEventQueue
{
    private readonly BlockingCollection<TmsEvent> _queue = new();

    public void Enqueue(TmsEvent tmsEvent) => _queue.Add(tmsEvent);
    
    public TmsEvent Dequeue(CancellationToken cancellationToken) => _queue.Take(cancellationToken);
}

// --- Cola de Evidencias ---
public class InMemoryEvidenceQueue : IEvidenceQueue
{
    private readonly BlockingCollection<EvidenceProcessingRequest> _queue = new();
    
    public void Enqueue(EvidenceProcessingRequest request) => _queue.Add(request);

    public EvidenceProcessingRequest Dequeue(CancellationToken cancellationToken) => _queue.Take(cancellationToken);
}

// --- Cola de Notificaciones ---
public class InMemoryNotificationQueue : INotificationQueue
{
    private readonly BlockingCollection<NotificationPayload> _queue = new();
    
    public void Enqueue(NotificationPayload payload) => _queue.Add(payload);

    public NotificationPayload Dequeue(CancellationToken cancellationToken) => _queue.Take(cancellationToken);
}