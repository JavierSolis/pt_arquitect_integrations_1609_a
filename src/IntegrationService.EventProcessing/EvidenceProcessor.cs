// src/IntegrationService.EventProcessing/EvidenceProcessor.cs
using IntegrationService.Application.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationService.EventProcessing;

public class EvidenceProcessor : BackgroundService
{
    private readonly ILogger<EvidenceProcessor> _logger;
    private readonly IEvidenceQueue _evidenceQueue;
    private readonly ICloudStorage _cloudStorage;

    public EvidenceProcessor(
        ILogger<EvidenceProcessor> logger,
        IEvidenceQueue evidenceQueue,
        ICloudStorage cloudStorage)
    {
        _logger = logger;
        _evidenceQueue = evidenceQueue;
        _cloudStorage = cloudStorage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Evidence Processor está iniciando.");

        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = _evidenceQueue.Dequeue(stoppingToken);
                    
                    _logger.LogInformation(
                        "<= Consumiendo evento de evidencia para la orden '{OrderNumber}', archivo '{FileName}'.",
                        request.OrderNumber,
                        request.Evidence.FileName);

                    // La única responsabilidad de este worker: llamar al servicio de almacenamiento
                    var savedPath = await _cloudStorage.SaveFileAsync(request.OrderNumber, request.Evidence);

                    _logger.LogInformation("Evidencia para la orden '{OrderNumber}' procesada y guardada en '{Path}'.",
                        request.OrderNumber,
                        savedPath);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando un evento de la cola de evidencias.");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }, stoppingToken);

         _logger.LogInformation("Evidence Processor se ha detenido.");
    }
}