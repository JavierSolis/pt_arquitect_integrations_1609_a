// src/IntegrationService.Api/Controllers/WebhookController.cs
using IntegrationService.Application.Contracts;
using IntegrationService.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationService.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IMainEventQueue _mainEventQueue;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IMainEventQueue mainEventQueue, ILogger<WebhookController> logger)
    {
        _mainEventQueue = mainEventQueue;
        _logger = logger;
    }

    [HttpPost("tms-events")]
    public IActionResult PostTmsEvent([FromBody] TmsEvent tmsEvent)
    {
        // ... (el resto del código es idéntico al de la respuesta anterior)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var orderNumber = tmsEvent.Details?.OrderNumber ?? "UNKNOWN";

        _logger.LogInformation(
            "=> Webhook recibido! Evento '{Status}' para la orden '{OrderNumber}'. Encolando...",
            tmsEvent.Status,
            orderNumber);

        try
        {
            _mainEventQueue.Enqueue(tmsEvent);

            _logger.LogInformation(
                "Evento para la orden '{OrderNumber}' encolado exitosamente en la cola principal. [Respondiendo 202 Accepted]",
                orderNumber);
            
            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al encolar el evento para la orden '{OrderNumber}'.", orderNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, "No se pudo encolar el evento.");
        }
    }
}