// src/IntegrationService.Domain/Order.cs
namespace IntegrationService.Domain;

public class Order
{
    public string Id { get; init; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int VisitCount { get; private set; }

    // Constructor para crear un nuevo pedido
    public Order(string id, string status, int visitCount = 0)
    {
        Id = id;
        Status = status;
        VisitCount = visitCount;
    }

    // Lógica de Dominio: Actualiza el estado si es válido
    public bool TryUpdateStatus(string newStatus)
    {
        if (Status is "DELIVERED" or "RETURNED")
        {
            return false; // No se puede actualizar un estado final
        }
        Status = newStatus;
        return true;
    }

    // Lógica de Dominio: Incrementa el contador de visitas
    public void IncrementVisitCount()
    {
        VisitCount++;
    }
}