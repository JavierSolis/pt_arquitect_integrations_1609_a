// src/IntegrationService.Api/Program.cs
using IntegrationService.Application.Contracts;
using IntegrationService.EventProcessing;
using IntegrationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTRO DE SERVICIOS EN EL CONTENEDOR DE DI ---

// --- TIEMPOS DE VIDA DE LOS SERVICIOS ---
// Singleton: Se crea UNA ÚNICA instancia para toda la vida de la aplicación.
//            Perfecto para servicios que gestionan estado compartido, como nuestras colas en memoria.
// Scoped: Se crea una nueva instancia por cada petición HTTP. No lo usaremos aquí.
// Transient: Se crea una nueva instancia cada vez que se solicita.

// --- Registro de las Colas en Memoria (Singleton) ---
// Necesitan ser Singleton para que el Controller (productor) y los Workers (consumidores)
// compartan la MISMA instancia de la cola.
builder.Services.AddSingleton<IMainEventQueue, InMemoryMainEventQueue>();
builder.Services.AddSingleton<IEvidenceQueue, InMemoryEvidenceQueue>();
builder.Services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();

// --- Registro de la Infraestructura Falsa (Singleton) ---
// También los registramos como Singleton porque simulan una base de datos o un servicio externo,
// que debe mantener su estado a lo largo de toda la aplicación.
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IEventHistoryRepository, InMemoryEventHistoryRepository>();
builder.Services.AddSingleton<ICloudStorage, FakeCloudStorage>();

// --- Registro de los Adaptadores de Notificación (Transient) ---
// Registramos todas las clases que implementan INotificationAdapter.
// El NotificationProcessor recibirá una colección de todos ellos.
builder.Services.AddTransient<INotificationAdapter, ClientAWebhookAdapter>();
builder.Services.AddTransient<INotificationAdapter, GenericEmailAdapter>();

// --- Registro de los Workers como Hosted Services ---
// AddHostedService registra nuestras clases BackgroundService para que el "Host" de .NET
// las inicie automáticamente cuando la aplicación arranque y las detenga de forma elegante.
builder.Services.AddHostedService<MainEventProcessor>();
builder.Services.AddHostedService<EvidenceProcessor>();
builder.Services.AddHostedService<NotificationProcessor>();

// --- Servicios estándar de la API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 

// --- 2. CONFIGURACIÓN DEL PIPELINE DE HTTP ---

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();