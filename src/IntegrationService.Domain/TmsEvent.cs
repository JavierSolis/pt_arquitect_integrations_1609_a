// src/IntegrationService.Domain/TmsEvent.cs
namespace IntegrationService.Domain;

public class TmsEvent
{
    public string ServiceType { get; set; } = string.Empty;
    public string DispatchType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SubStatus { get; set; } = string.Empty;
    public string VehicleCode { get; set; } = string.Empty;
    public string CourierName { get; set; } = string.Empty;
    public Details Details { get; set; } = new();
    public List<Evidence> Evidences { get; set; } = [];
    public DateTime EventDate { get; set; }
}

public class Details
{
    public string OrderNumber { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string ClientCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}

public class Evidence
{
    public string Label { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}