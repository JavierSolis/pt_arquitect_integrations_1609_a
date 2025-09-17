// src/IntegrationService.Application.Contracts/ICloudStorage.cs
using IntegrationService.Domain;
namespace IntegrationService.Application.Contracts;

public interface ICloudStorage
{
    Task<string> SaveFileAsync(string orderNumber, Evidence evidence);
}