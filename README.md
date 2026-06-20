# SINPE POS System

A Point-of-Sale (POS) system for processing **SINPE Móvil** payments in Costa Rica. The system receives SMS notifications from SINPE Móvil, validates payments, matches them against pending orders, and maintains a complete audit trail.

## Architecture

```
┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐
│   Android App       │     │   Backend API       │     │   Frontend Web      │
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

## Tech Stack

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Web API framework |
| Entity Framework Core | 10.0.5 | ORM / Database access |
| SQL Server | - | Relational database |
| Swashbuckle | 10.1.7 | Swagger/OpenAPI documentation |

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| React | 19.2.4 | UI library |
| TypeScript | 6.0.2 | Type-safe JavaScript |
| Vite | 8.0.4 | Build tool / Dev server |
| React Router | 7.15.1 | Client-side routing |

### Mobile

| Technology | Purpose |
|------------|---------|
| Kotlin | Android SMS receiver app |

## Project Structure

```
ProyectoIngenieria2026/
├── ProyectoIngenieriaBACKEND_POS/    # .NET Backend API
│   ├── Controllers/                  # API endpoints
│   │   ├── SmsController.cs          # SMS reception endpoint
│   │   ├── OrdersController.cs       # Order management
│   │   ├── PaymentsController.cs     # Payment queries & review
│   │   └── AuditLogController.cs     # Audit log queries
│   ├── Services/                     # Business logic
│   │   ├── SmsReceiverService.cs     # SMS parsing & payment processing
│   │   ├── OrderService.cs           # Order CRUD operations
│   │   ├── PaymentService.cs         # Payment queries & review
│   │   ├── AuditLogService.cs        # Audit event logging
│   │   └── Interfaces/               # Service contracts
│   ├── Models/
│   │   ├── Entities/                 # Database entities
│   │   ├── Dtos/                     # Data Transfer Objects
│   │   └── Enums/                    # enumerations
│   ├── Data/                         # DbContext configuration
│   └── Program.cs                    # Application entry point
│
├── ProyectoFrontend_POS/             # React Frontend
│   └── src/
│       ├── features/                 # Feature modules
│       ├── components/               # Reusable UI components
│       ├── services/                 # API client services
│       └── hooks/                    # Custom React hooks
│
├── BNSinpeSimulatorAPP/              # Kotlin Android App
│   └── app/                          # Android source code
│
└── Proyecto-Kotlin/                  # Additional Kotlin modules
```

## Features

### Core Functionality
- **SMS Parsing** — Automatic extraction of payment data from SINPE Móvil SMS using regex
- **Payment Validation** — Duplicate reference detection and 15-minute expiration window
- **Order Matching** — Automatic matching of incoming payments with pending orders
- **Manual Review** — Admin interface for reviewing ambiguous payments

### Order States
| State | Description |
|-------|-------------|
| `PENDIENTE` | Order created, awaiting payment |
| `PAGADA` | Payment matched and confirmed |
| `PAGO_PARCIAL` | Partial payment received (amount mismatch) |
| `PAGADA_REVISADA` | Payment approved after manual review |

### Payment States
| State | Description |
|-------|-------------|
| `Pending` | Payment received, not yet processed |
| `Valid` | Payment confirmed and matched to order |
| `Rejected` | Payment rejected (duplicate, expired, etc.) |
| `PendingReview` | Requires manual admin review |

### Audit System
- Immutable audit log for all system events
- Event types: `PaymentConfirmed`, `DuplicateReference`, `PaymentExpired`, `OrderExpired`, etc.
- Risk levels: `Low`, `Medium`, `High`, `Critical`
- Filterable by event type and risk level

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) or a cloud instance

### Backend Setup

```bash
cd ProyectoIngenieriaBACKEND_POS

# Restore dependencies
dotnet restore

# Update connection string in appsettings.json
# Set your SQL Server connection string

# Apply migrations
dotnet ef database update

# Run the API
dotnet run
```

The API will be available at `https://localhost:5000` with Swagger UI at `/swagger`.

### Frontend Setup

```bash
cd ProyectoFrontend_POS

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at `http://localhost:5173`.

## API Endpoints

### SMS

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/sms/receive` | Receive and process incoming SMS |

### Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/orders` | Get all orders |
| `POST` | `/api/orders` | Create a new order |
| `GET` | `/api/orders/{id}/status` | Get order status |

### Payments

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/payments/list` | Get all payments with client info |
| `GET` | `/api/payments/pending-review` | Get payments awaiting review |
| `PUT` | `/api/payments/{id}/review` | Approve or reject a payment |

### Audit Logs

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/auditlog` | Get all audit logs |
| `GET` | `/api/auditlog/byevent?eventType=` | Filter by event type |
| `GET` | `/api/auditlog/byrisk?riskLevel=` | Filter by risk level |

## Database Schema

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

## License

Academic project - Universidad de Costa Rica
