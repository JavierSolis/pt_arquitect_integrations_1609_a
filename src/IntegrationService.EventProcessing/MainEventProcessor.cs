// src/IntegrationService.EventProcessing/MainEventProcessor.cs
using IntegrationService.Application.Contracts;
using IntegrationService.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationService.EventProcessing;

public class MainEventProcessor : BackgroundService
{
    private readonly ILogger<MainEventProcessor> _logger;
    private readonly IMainEventQueue _mainEventQueue;
    private readonly IOrderRepository _orderRepository;
    private readonly IEventHistoryRepository _eventHistoryRepository;
    private readonly IEvidenceQueue _evidenceQueue;
    private readonly INotificationQueue _notificationQueue;

    public MainEventProcessor(
        ILogger<MainEventProcessor> logger,
        IMainEventQueue mainEventQueue,
        IOrderRepository orderRepository,
        IEventHistoryRepository eventHistoryRepository,
        IEvidenceQueue evidenceQueue,
        INotificationQueue notificationQueue)
    {
        _logger = logger;
        _mainEventQueue = mainEventQueue;
        _orderRepository = orderRepository;
        _eventHistoryRepository = eventHistoryRepository;
        _evidenceQueue = evidenceQueue;
        _notificationQueue = notificationQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Main Event Processor está iniciando.");

        // Registra un callback que se ejecutará cuando se solicite la cancelación.
        stoppingToken.Register(() => 
            _logger.LogInformation("Main Event Processor está deteniéndose (señal de cancelación recibida)."));

        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var tmsEvent = _mainEventQueue.Dequeue(stoppingToken);
                    
                    _logger.LogInformation(
                        "<= Consumiendo evento '{Status}' de la cola principal para la orden '{OrderNumber}'.",
                        tmsEvent.Status,
                        tmsEvent.Details.OrderNumber);

                    await ProcessEventAsync(tmsEvent, stoppingToken); // Pasa el token por si acaso
                }
                catch (OperationCanceledException)
                {
                    // Esto es normal, salimos del bucle.
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando un evento de la cola principal.");
                    // Espera un poco antes de intentar con el siguiente mensaje para evitar bucles de error rápidos.
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }, stoppingToken);

        _logger.LogInformation("Main Event Processor se ha detenido completamente.");
    }

    private async Task ProcessEventAsync(TmsEvent tmsEvent, CancellationToken cancellationToken)
    {

        var orderNumber = tmsEvent.Details.OrderNumber;

        // 1. Registrar el evento en el historial (Requisito #5)
        await _eventHistoryRepository.AddAsync(tmsEvent);

        // 2. Buscar el pedido en nuestro "OMS"
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        if (order is null)
        {
            _logger.LogWarning("No se encontró la orden '{OrderNumber}'. El evento será ignorado.", orderNumber);
            return;
        }

        // 3. Validar que el pedido no tenga un estado final (Requisito #2)
        if (!order.TryUpdateStatus(tmsEvent.Status))
        {
            _logger.LogWarning(
                "La orden '{OrderNumber}' ya está en un estado final ('{Status}'). No se realizarán más actualizaciones.",
                orderNumber, order.Status);
            return;
        }
        _logger.LogInformation("Lógica de negocio: Estado de la orden '{OrderNumber}' actualizado a '{NewStatus}'.", orderNumber, tmsEvent.Status);
        // 4. Actualizar contador de visitas si aplica (Requisito #3)

        if (tmsEvent.Status is "DELIVERED" or "NOT DELIVERED")
        {
            order.IncrementVisitCount();
            _logger.LogInformation("Lógica de negocio: Contador de visitas para la orden '{OrderNumber}' actualizado a {VisitCount}.", orderNumber, order.VisitCount);
        }

        // 5. Lógica de Devolución si se alcanzan 3 visitas (Requisito #4)
        if (tmsEvent.Status is "NOT DELIVERED" && order.VisitCount >= 3)
        {
            _logger.LogWarning("LÓGICA CRÍTICA: La orden '{OrderNumber}' ha alcanzado {VisitCount} intentos. Generando evento TO BE RETURN.", orderNumber, order.VisitCount);
            order.TryUpdateStatus("TO BE RETURN");
            var returnEvent = new TmsEvent { Status = "TO BE RETURN", Details = new Details { OrderNumber = orderNumber, ClientName = tmsEvent.Details.ClientName } };
            await _eventHistoryRepository.AddAsync(returnEvent);
        }

        await _orderRepository.UpdateAsync(order);

        if (tmsEvent.Evidences.Any())
        {
            foreach (var evidence in tmsEvent.Evidences)
            {
                var request = new EvidenceProcessingRequest(orderNumber, evidence);
                _evidenceQueue.Enqueue(request);
                _logger.LogInformation("=> Publicando tarea de evidencia ('{FileName}') en la cola de evidencias.", evidence.FileName);
            }
        }
        
        var notificationPayload = new NotificationPayload(orderNumber, tmsEvent.Details.ClientName, order.Status);
        _notificationQueue.Enqueue(notificationPayload);
        _logger.LogInformation("=> Publicando tarea de notificación para el cliente '{ClientName}' en la cola de notificaciones.", tmsEvent.Details.ClientName);
    }

    private async Task ProcessEventAsync_backup(TmsEvent tmsEvent, CancellationToken cancellationToken)
    {
        var orderNumber = tmsEvent.Details.OrderNumber;

        // 1. Registrar el evento en el historial (Requisito #5)
        await _eventHistoryRepository.AddAsync(tmsEvent);

        // 2. Buscar el pedido en nuestro "OMS"
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        if (order is null)
        {
            _logger.LogWarning("No se encontró la orden '{OrderNumber}'. El evento será ignorado.", orderNumber);
            return;
        }

        // 3. Validar que el pedido no tenga un estado final (Requisito #2)
        if (!order.TryUpdateStatus(tmsEvent.Status))
        {
            _logger.LogWarning(
                "La orden '{OrderNumber}' ya está en un estado final ('{Status}'). No se realizarán más actualizaciones.",
                orderNumber, order.Status);
            return;
        }
        _logger.LogInformation("Estado de la orden '{OrderNumber}' actualizado a '{NewStatus}'.", orderNumber, tmsEvent.Status);


        // 4. Actualizar contador de visitas si aplica (Requisito #3)
        if (tmsEvent.Status is "DELIVERED" or "NOT DELIVERED")
        {
            order.IncrementVisitCount();
            _logger.LogInformation("Contador de visitas para la orden '{OrderNumber}' actualizado a {VisitCount}.", orderNumber, order.VisitCount);
        }

        // 5. Lógica de Devolución si se alcanzan 3 visitas (Requisito #4)
        if (tmsEvent.Status is "NOT DELIVERED" && order.VisitCount >= 3)
        {
            _logger.LogWarning("La orden '{OrderNumber}' ha alcanzado {VisitCount} intentos de entrega. Generando evento TO BE RETURN.", orderNumber, order.VisitCount);
            order.TryUpdateStatus("TO BE RETURN"); // Actualizamos el estado final a "TO BE RETURN"

            // Creamos un nuevo evento para ser procesado. Esto podría encolarse o manejarse directamente.
            // Para la simulación, lo registraremos y actualizaremos el estado.
            var returnEvent = new TmsEvent { Status = "TO BE RETURN", Details = new Details { OrderNumber = orderNumber } };
            await _eventHistoryRepository.AddAsync(returnEvent);
        }

        // 6. Guardar el estado actualizado del pedido en nuestro "OMS"
        await _orderRepository.UpdateAsync(order);
        _logger.LogInformation("Orden '{OrderNumber}' guardada en el OMS.", orderNumber);

        // 7. Encolar tareas de evidencia si existen (Requisito #6)
        if (tmsEvent.Evidences.Any())
        {
            foreach (var evidence in tmsEvent.Evidences)
            {
                var request = new EvidenceProcessingRequest(orderNumber, evidence);
                _evidenceQueue.Enqueue(request);
                _logger.LogInformation("=> Publicando evento de evidencia ('{FileName}') en la cola de evidencias.", evidence.FileName);
            }
        }

        // 8. Encolar tarea de notificación al cliente (Requisito #7)
        var notificationPayload = new NotificationPayload(orderNumber, tmsEvent.Details.ClientName, order.Status);
        _notificationQueue.Enqueue(notificationPayload);
        _logger.LogInformation("=> Publicando evento de notificación para el cliente '{ClientName}' en la cola de notificaciones.", tmsEvent.Details.ClientName);
    }
}