```shell
# --- 1. CREACIÓN DE CARPETAS Y SOLUCIÓN ---

# Crea la carpeta principal para el proyecto y entra en ella
mkdir IntegrationService
cd IntegrationService

# Crea el archivo de la solución (.sln) que agrupará todos nuestros proyectos
dotnet new sln -n IntegrationService

# Crea una carpeta 'src' para mantener el código fuente organizado
mkdir src


# --- 2. CREACIÓN DE LOS 5 PROYECTOS DENTRO DE 'src' ---

# Proyecto Domain: Contendrá nuestras entidades (clases puras)
dotnet new classlib -n IntegrationService.Domain -o src/IntegrationService.Domain

# Proyecto Application.Contracts: Contendrá nuestras interfaces y DTOs
dotnet new classlib -n IntegrationService.Application.Contracts -o src/IntegrationService.Application.Contracts

# Proyecto Api: Nuestro punto de entrada (ASP.NET Core Web API)
dotnet new webapi -n IntegrationService.Api -o src/IntegrationService.Api

# Proyecto EventProcessing: Contendrá nuestros workers de fondo
dotnet new classlib -n IntegrationService.EventProcessing -o src/IntegrationService.EventProcessing

# Proyecto Infrastructure: Contendrá nuestras implementaciones "falsas" (mocks)
dotnet new classlib -n IntegrationService.Infrastructure -o src/IntegrationService.Infrastructure


# --- 3. AÑADIR LOS PROYECTOS A LA SOLUCIÓN ---

dotnet sln add src/IntegrationService.Domain/IntegrationService.Domain.csproj
dotnet sln add src/IntegrationService.Application.Contracts/IntegrationService.Application.Contracts.csproj
dotnet sln add src/IntegrationService.Api/IntegrationService.Api.csproj
dotnet sln add src/IntegrationService.EventProcessing/IntegrationService.EventProcessing.csproj
dotnet sln add src/IntegrationService.Infrastructure/IntegrationService.Infrastructure.csproj


# --- 4. ESTABLECER LAS REFERENCIAS ENTRE PROYECTOS ---

# Application.Contracts necesita conocer los modelos de Domain
dotnet add src/IntegrationService.Application.Contracts/IntegrationService.Application.Contracts.csproj reference src/IntegrationService.Domain/IntegrationService.Domain.csproj

# Infrastructure necesita implementar los contratos y conocer los modelos de Domain
dotnet add src/IntegrationService.Infrastructure/IntegrationService.Infrastructure.csproj reference src/IntegrationService.Application.Contracts/IntegrationService.Application.Contracts.csproj
dotnet add src/IntegrationService.Infrastructure/IntegrationService.Infrastructure.csproj reference src/IntegrationService.Domain/IntegrationService.Domain.csproj

# EventProcessing necesita los contratos, los modelos y la infraestructura (para registrar sus propias dependencias si fuera necesario)
dotnet add src/IntegrationService.EventProcessing/IntegrationService.EventProcessing.csproj reference src/IntegrationService.Application.Contracts/IntegrationService.Application.Contracts.csproj
dotnet add src/IntegrationService.EventProcessing/IntegrationService.EventProcessing.csproj reference src/IntegrationService.Domain/IntegrationService.Domain.csproj

# Api (el host) necesita referenciar todo para poder "ensamblar" la aplicación
dotnet add src/IntegrationService.Api/IntegrationService.Api.csproj reference src/IntegrationService.Application.Contracts/IntegrationService.Application.Contracts.csproj
dotnet add src/IntegrationService.Api/IntegrationService.Api.csproj reference src/IntegrationService.EventProcessing/IntegrationService.EventProcessing.csproj
dotnet add src/IntegrationService.Api/IntegrationService.Api.csproj reference src/IntegrationService.Infrastructure/IntegrationService.Infrastructure.csproj
```
