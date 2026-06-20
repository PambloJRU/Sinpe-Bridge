# Sistema POS SINPE

Un sistema de punto de venta (POS) para procesar pagos de **SINPE Móvil** en Costa Rica. El sistema recibe notificaciones SMS de SINPE Móvil, valida los pagos, los empareja con órdenes pendientes y mantiene un registro de auditoría completo.

## Arquitectura

```
┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐
│   App Android       │     │   API Backend       │     │   Frontend Web      │
│   (Kotlin)          │────▶│   (.NET 10)         │◀────│   (React + TS)      │
│                     │     │                     │     │                     │
│ BNSinpeSimulatorAPP │     │ REST API + Swagger  │     │ Vite + React 19     │
└─────────────────────┘     └─────────┬───────────┘     └─────────────────────┘
                                      │
                                      ▼
                             ┌─────────────────┐
                             │   SQL Server    │
                             │   (Somee)       │
                             └─────────────────┘
```

## Stack Tecnológico

### Backend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 10.0 | Framework de API web |
| Entity Framework Core | 10.0.5 | ORM / Acceso a base de datos |
| SQL Server | - | Base de datos relacional |
| Swashbuckle | 10.1.7 | Documentación Swagger/OpenAPI |

### Frontend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| React | 19.2.4 | Librería de UI |
| TypeScript | 6.0.2 | JavaScript con tipos |
| Vite | 8.0.4 | Herramienta de construcción / Dev server |
| React Router | 7.15.1 | Enrutamiento del lado del cliente |

### Móvil

| Tecnología | Propósito |
|------------|-----------|
| Kotlin | App Android receptora de SMS |

## Estructura del Proyecto

```
ProyectoIngenieria2026/
├── ProyectoIngenieriaBACKEND_POS/    # API Backend .NET
│   ├── Controllers/                  # Endpoints de la API
│   │   ├── SmsController.cs          # Endpoint de recepción de SMS
│   │   ├── OrdersController.cs       # Gestión de órdenes
│   │   ├── PaymentsController.cs     # Consultas y revisión de pagos
│   │   └── AuditLogController.cs     # Consultas de auditoría
│   ├── Services/                     # Lógica de negocio
│   │   ├── SmsReceiverService.cs     # Parsing de SMS y procesamiento
│   │   ├── OrderService.cs           # Operaciones CRUD de órdenes
│   │   ├── PaymentService.cs         # Consultas y revisión de pagos
│   │   ├── AuditLogService.cs        # Registro de eventos de auditoría
│   │   └── Interfaces/               # Contratos de servicios
│   ├── Models/
│   │   ├── Entities/                 # Entidades de la base de datos
│   │   ├── Dtos/                     # Objetos de Transferencia de Datos
│   │   └── Enums/                    # Enumeraciones
│   ├── Data/                         # Configuración del DbContext
│   └── Program.cs                    # Punto de entrada de la aplicación
│
├── ProyectoFrontend_POS/             # Frontend React
│   └── src/
│       ├── features/                 # Módulos de funcionalidad
│       ├── components/               # Componentes UI reutilizables
│       ├── services/                 # Servicios cliente de API
│       └── hooks/                    # Hooks personalizados de React
│
├── BNSinpeSimulatorAPP/              # App Android Kotlin
│   └── app/                          # Código fuente de Android
│
└── Proyecto-Kotlin/                  # Módulos Kotlin adicionales
```

## Funcionalidades

### Funcionalidad Principal
- **Parsing de SMS** — Extracción automática de datos de pago desde SMS de SINPE Móvil usando regex
- **Validación de Pagos** — Detección de referencias duplicadas y ventana de expiración de 15 minutos
- **Emparejamiento de Órdenes** — Coincidencia automática de pagos entrantes con órdenes pendientes
- **Revisión Manual** — Interfaz de administración para revisar pagos ambiguos

### Estados de Orden
| Estado | Descripción |
|--------|-------------|
| `PENDIENTE` | Orden creada, esperando pago |
| `PAGADA` | Pago emparejado y confirmado |
| `PAGO_PARCIAL` | Pago parcial recibido (monto no coincide) |
| `PAGADA_REVISADA` | Pago aprobado después de revisión manual |

### Estados de Pago
| Estado | Descripción |
|--------|-------------|
| `Pending` | Pago recibido, aún no procesado |
| `Valid` | Pago confirmado y emparejado con orden |
| `Rejected` | Pago rechazado (duplicado, expirado, etc.) |
| `PendingReview` | Requiere revisión manual del administrador |

### Sistema de Auditoría
- Registro de auditoría inmutable para todos los eventos del sistema
- Tipos de evento: `PaymentConfirmed`, `DuplicateReference`, `PaymentExpired`, `OrderExpired`, etc.
- Niveles de riesgo: `Low`, `Medium`, `High`, `Critical`
- Filtrable por tipo de evento y nivel de riesgo

## Inicio Rápido

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) o una instancia en la nube

### Configuración del Backend

```bash
cd ProyectoIngenieriaBACKEND_POS

# Restaurar dependencias
dotnet restore

# Actualizar la cadena de conexión en appsettings.json
# Configurar tu cadena de conexión de SQL Server

# Aplicar migraciones
dotnet ef database update

# Ejecutar la API
dotnet run
```

La API estará disponible en `https://localhost:5000` con Swagger UI en `/swagger`.

### Configuración del Frontend

```bash
cd ProyectoFrontend_POS

# Instalar dependencias
npm install

# Iniciar servidor de desarrollo
npm run dev
```

El frontend estará disponible en `http://localhost:5173`.

## Endpoints de la API

### SMS

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/sms/receive` | Recibir y procesar SMS entrante |

### Órdenes

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/orders` | Obtener todas las órdenes |
| `POST` | `/api/orders` | Crear una nueva orden |
| `GET` | `/api/orders/{id}/status` | Obtener estado de la orden |

### Pagos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/payments/list` | Obtener pagos con información del cliente |
| `GET` | `/api/payments/pending-review` | Obtener pagos pendientes de revisión |
| `PUT` | `/api/payments/{id}/review` | Aprobar o rechazar un pago |

### Logs de Auditoría

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/auditlog` | Obtener todos los logs de auditoría |
| `GET` | `/api/auditlog/byevent?eventType=` | Filtrar por tipo de evento |
| `GET` | `/api/auditlog/byrisk?riskLevel=` | Filtrar por nivel de riesgo |

## Esquema de Base de Datos

```
┌──────────────┐       ┌──────────────┐       ┌──────────────────────┐
│   Clients    │       │   Payments   │       │      Orders          │
├──────────────┤       ├──────────────┤       ├──────────────────────┤
│ Id (PK)      │──┐    │ Id (PK)      │──┐    │ Id (PK)              │
│ Name         │  └───▶│ ClientId (FK)│  └───▶│ PaymentId (FK)       │
│ Phone        │       │ Amount       │       │ Amount               │
└──────────────┘       │ Reference    │       │ Phone                │
                       │ ReceivedAt   │       │ State                │
                       │ Status       │       └──────────────────────┘
                       │ OriginalMsg  │
                       └──────────────┘

┌──────────────────────┐       ┌──────────────────────┐
│    AuditLogs         │       │ DuplecateReferences  │
├──────────────────────┤       ├──────────────────────┤
│ Id (PK)              │       │ Id (PK)              │
│ CreatedAt            │       │ Cellphone            │
│ EventType            │       │ IdClient (FK)        │
│ RiskLevel            │       └──────────────────────┘
│ Description          │
│ AdditionalData       │
│ PaymentId (FK)       │
│ OrderId (FK)         │
└──────────────────────┘
```

## Licencia

Proyecto académico - Universidad de Costa Rica
