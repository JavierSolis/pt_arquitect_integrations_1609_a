// src/IntegrationService.Infrastructure/FakeCloudStorage.cs
using IntegrationService.Application.Contracts;
using IntegrationService.Domain;
using Microsoft.Extensions.Logging;

namespace IntegrationService.Infrastructure;

public class FakeCloudStorage : ICloudStorage
{
    private readonly ILogger<FakeCloudStorage> _logger;

    public FakeCloudStorage(ILogger<FakeCloudStorage> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(string orderNumber, Evidence evidence)
    {
        // Simulamos la descarga y subida con un pequeño retraso
        _logger.LogInformation("Descargando archivo '{FileName}'...", evidence.FileName);
        await Task.Delay(250); // Simula I/O de red

        var cloudPath = $"cloud-storage/{orderNumber}/{evidence.FileName}";
        _logger.LogInformation("Archivo '{FileName}' guardado en '{CloudPath}'.", evidence.FileName, cloudPath);

        return cloudPath;
    }
}