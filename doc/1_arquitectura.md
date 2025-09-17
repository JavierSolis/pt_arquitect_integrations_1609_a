# Arqutiectura propuesta

# Versión Inicial ( Idea )

Se diagrama la idea principal o base de la arquitectura, para tener claro los componentes principales, el principal motivo de mejorar esta arquitectura es poder soportar miles de enviós diarios.

```mermaid
---
title: Versión Inicial
---
graph TD
    subgraph "Sistema Externo"
        TMS(TMS Beetrack)
    end

    subgraph "Arquitectura de Integración"
        API_Component(a\ API, componente que recibe las llamadas por webhook)
        WORKER(b\ Worker principal, Logica según estados, reintentos y notificación)
    end

    subgraph "Sistemas Internos"
        OMS(OMS Interno)
        Storage(Almacenamiento de evidencias)
    end

    subgraph "Sistemas Externos"
        Client_A(Sistema Cliente A)
        Client_B(Sistema Cliente B)
    end

    TMS -- Webhook (JSON) --> API_Component
    API_Component -- Publica Evento --> WORKER
    WORKER -- Lee/Escribe --> OMS
    WORKER -- Escribe --> Storage
    WORKER -- Publica evento --> Client_A
    WORKER -- Publica evento --> Client_B

```

<br>
<br>

# Versión 2 ( según requerimientos )

Se detalla la arquitectura propuesta para la integración, considerando los requerimientos de escalabilidad, observabilidad y resiliencia.

## Diagrama de versión 2

```mermaid
---
title: Versión 2
---
graph TD
    subgraph "Sistema Externo"
        TMS(TMS Beetrack)
    end

    subgraph "Nuestra Arquitectura de Integración"
        API_Gateway(1\. API Gateway)
        Main_Queue("2\. Cola Principal de Eventos <br><i>(ej. SQS,  RabbitMQ ..)</i>")
        Processor("3\. Procesador Principal de Eventos <br><i>(Worker Service / Lambda, Azure Function)</i>")
        DLQ("4\. Cola de Mensajes Muertos - DLQ<br>Principal")


        Evidence_Queue(5\. Cola de Evidencias)
        Evidence_Handler("6\. Gestor de Evidencias <br><i>(Worker Service / Azure Function)</i>")
        Notification_Queue(7\. Cola de Notificaciones)

        subgraph "Worker"
            Notification_Dispatcher("8\. Despachador de Notificaciones <br><i>(Worker Service / Azure Function)</i>")
            Adapter_A("8.1\. Adaptador Cliente A<br><i>(Ej. Webhook)</i>")
            Adapter_B("8.2\. Adaptador Cliente B<br><i>(Ej. Email/SFTP)</i>")
        end


        DLQ2("9\.Cola de Mensajes Muertos - DLQ<br>Evidencias")
        DLQ3("10\.Cola de Mensajes Muertos - DLQ<br>Notificaciones")
    end

    subgraph "."
        subgraph "Sistemas Internos"
            OMS(OMS Interno)
            History_DB(BD de Histórico/Auditoría)
            Cloud_Storage("Almacenamiento en la Nube <br><i>(ej. S3, Azure Blob Storage)</i>")
        end

        subgraph "Sistemas Externos"
            Client_A(Sistema Cliente A)
            Client_B(Sistema Cliente B)
        end
    end

    %% Flujo Principal
    API_Gateway -- Publica Evento --> Main_Queue
    TMS -- Webhook (JSON) --> API_Gateway
    Processor -- Escribe --> History_DB
    Main_Queue -- Consume Evento<br> / Reintentos automáticos --> Processor
    Processor -- Lee/Escribe --> OMS
    Processor -- Publica Evento ''Evidencia a procesar''  --> Evidence_Queue
    Processor -- Publica Evento ''Notificar Cliente'' --> Notification_Queue

    %% Flujo de Reintentos y Errores
    Main_Queue -- En caso de fallo persistente --> DLQ


    %% Flujo de Evidencias (Asíncrono)
    Evidence_Queue -- Consume Evento --> Evidence_Handler
    Evidence_Queue -- En caso Falle --> DLQ2
    Evidence_Handler -- Actualiza ruta de evidencia en --> OMS
    Evidence_Handler -- Descarga y Guarda --> Cloud_Storage

    %% Flujo de Notificaciones (Asíncrono)
    Notification_Queue -- Consume Evento --> Notification_Dispatcher
    Notification_Queue -- En caso fallo --> DLQ3
    Notification_Dispatcher -- Envía a --> Adapter_A
    Notification_Dispatcher -- Envía a --> Adapter_B
    Adapter_A -- Notificación Push --> Client_A
    Adapter_B -- Notificación Push --> Client_B


```

<br>
<br>

## Diagrama de versión 2 como imagen con iconos de AWS

<div align="center">
    <img src="images/image_arq_eraser.png" alt="arqutectura en otro formato" />
</div>

<br>
<br>

## Descripción de componentes

1. Api Gateway:<br>
   Único punto de entrada público, su única responsabilidad es recibir el webhook del TMS, Existe componente eespcializado en AWS y Azure para este trabajo. (Requisito de recibir en tiempo real y confirma la recepción)

2. Cola Principal de Eventos:<br>
   Persiste los eventos, asegurando que no se pierdan si los sistemas posteriores fallan. Permite que el Procesador Principal consuma los eventos a su propio ritmo. Se configura con una política de reintentos (ej. reintentar 3 veces con esperas exponenciales). Cumpliendo el requisito de Lógica de Reintentos.
3. Procesador principal de eventos:<br>
   El worker que procesa segun las reglas de negocio, según el estado y CUS que hemos analizado, actualiza el OMA y el contador de visitas.

4. Cola de Mensajes Muertos de los principales (DLQ):
   Si un mensaje falla repetidamente en la cola principal se mueve aquí.

5. Cola de Evidencias:<br>
   Si el evento contiene evidencias, el worker publica un nuevo mensaje en la Cola de Evidencias, estos para procesar por separado la gestión de evidencias.

6. Gestor de Evidencias:<br>
   Worker que se encarga de descargar las evidencias adjuntas y las actualiza en el OMS
7. Cola de Notificaciones:<br>
   Cola dedicada para manejar el envío de notificaciones a clientes.
8. Despachador de Notificaciones y Adaptadores:
   El despachador consume de la Cola de Notificaciones. Lee el mensaje, identifica al cliente y, basándose e invoca al "Adaptador" específico para ese cliente.

   - 8.1. y 8.2. Son adaptadores a nivel del código, según el cliente sms, android notificactions, apple notificatiosn, email, etc.

9. Colsa de Mesajes muertos DLQ:<br>
   Cola de mensajes muertos para la cola de evidencias.
10. Colsa de Mesajes muertos DLQ:<br>
    Cola de mensajes muertos para la cola de notificaciones.

## Justificación de patrones de diseño

- Arquitectura Orientada a Eventos (EDA): Elegida porque el problema es inherentemente basado en eventos ("el courier llegó", "el pedido fue entregado"), patron enfocada en eventos donde lso componentes reacciones y de forma asincrona.( https://aws.amazon.com/es/what-is/eda/)
<br>
<div align="center">
    <img src="images/image_eda.png" alt="EDA" />
</div>
<br>

- Message Queue (Broker): Garantiza que no se pierdan datos (persistencia), maneja picos ( miles de notificaciones) https://www.enterpriseintegrationpatterns.com/patterns/messaging/MessageBroker.html

<br>
<div align="center">
    <img src="images/image_message_broker.png" />
</div>
<br>

- Dead-Letter Queue (DLQ): Es el patrón estándar para manejar errores irrecuperables de las colas, mas que guardar los mensajes el componente ofrece opciones de administración para auditarlos, notificar a soporte, reasignarlo a la cola principal una ves arreglado el problema. ( https://www.enterpriseintegrationpatterns.com/patterns/messaging/DeadLetterChannel.html )

<br>
<div align="center">
    <img src="images/image_DLQ.png" />
</div>
<br>

- Adapter Pattern: Mas para la implementación donde se tiene interfaces de clientes, y se van implementando segun cada cliente.
  https://refactoring.guru/es/design-patterns/adapter

<div align="center">
    <img src="images/image_adapter.png" alt="Adapter Pattern" />
</div>

- No es un patron, pero se uso el principcio, de "Separación de responsabilidades", en lugar de un gran servicio monolítico, dividimos las tareas/responsabilidades en componente, asi poder escalar y tolerar la carga de notificaciones

# Mejoras

- Multi AZ segun la zona geografica para catastrofes
- Optimizacion de costos con tipos de storage
- CDN si se desea acceder a las fotos de forma mas rapida y con baja latencia
- Redudancia de la base de datos para accesos solo de lectura(replicas de lectura)
- Monitoreo y alarmas para los servicios(Healtchecks)
