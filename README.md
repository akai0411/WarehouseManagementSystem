# Warehouse Management System API

A RESTful Warehouse Management System built with **ASP.NET Core 8** following **Clean Architecture** principles. The project demonstrates scalable backend development practices including CQRS, repository pattern, validation, logging, exception handling, pagination, filtering, sorting, and unit testing.

---

## Overview

This API allows users to manage warehouse products while maintaining a clean and maintainable architecture. It was developed as a portfolio project to demonstrate modern .NET backend development practices and software engineering principles.

---

## Features

* Product management (CRUD)
* Warehouse / Zone / Location management (CRUD, hierarchical: Warehouse → Zone → Location)
* Inventory management (assign a product to a location, unassign, query)
* Stock movement ledger (Inbound / Outbound / Adjustment) with automatic quantity updates
* JWT authentication with role-based authorization (Admin, RegionalManager, WarehouseManager, Operator)
* Role-based data scoping (e.g. a WarehouseManager only sees/manages their own warehouse; a RegionalManager is scoped by country)
* Optimistic concurrency control (RowVersion) on Product and Inventory updates
* Soft delete, with referential guards (e.g. a warehouse can't be deleted while it still has active zones)
* SKU uniqueness validation
* Pagination, filtering and sorting on list endpoints
* Global exception handling
* Structured logging with Serilog
* Swagger / OpenAPI documentation with JWT authorization support
* XML API documentation
* FluentValidation
* Standard API response wrapper
* Unit testing with xUnit v3 and Moq
* Docker support (API + SQL Server via docker-compose), with migrations and seed data applied automatically on startup

---

## Technologies

* ASP.NET Core 8
* Entity Framework Core
* SQL Server
* Clean Architecture
* CQRS
* Repository Pattern
* FluentValidation
* Serilog
* Swagger / Swashbuckle
* xUnit v3
* Moq
* Docker / Docker Compose

---

## Project Structure

```
WarehouseManagementSystem
│
├── WarehouseManagementSystem.Api
│
├── WarehouseManagementSystem.Application
│
├── WarehouseManagementSystem.Domain
│
├── WarehouseManagementSystem.Infrastructure
│
└── WarehouseManagementSystem.UnitTests
```

---

## Architecture

```
API
 │
 ▼
Application (CQRS Handlers)
 │
 ▼
Repository Interfaces
 │
 ▼
Infrastructure (EF Core)
 │
 ▼
SQL Server
```

The project follows Clean Architecture principles where:

* API handles HTTP requests.
* Application contains business logic.
* Domain contains entities.
* Infrastructure handles persistence.
* Dependencies always point inward.

---

## API Endpoints

### Auth

| Method | Endpoint                | Description                                | Roles                  |
| ------ | ------------------------ | ------------------------------------------- | ----------------------- |
| POST   | `/api/auth/register`     | Register a new user                         | Public                  |
| POST   | `/api/auth/login`        | Authenticate and receive a JWT              | Public                  |
| PUT    | `/api/auth/assign-role`  | Assign a role/scope to a user               | Admin, WarehouseManager |

### Products

| Method | Endpoint             | Description          | Roles |
| ------ | -------------------- | --------------------- | ----- |
| GET    | `/api/products`      | Get all products      | Authenticated |
| GET    | `/api/products/{id}` | Get product by id      | Authenticated |
| POST   | `/api/products`      | Create product         | Admin |
| PUT    | `/api/products/{id}` | Update product         | Admin |
| DELETE | `/api/products/{id}` | Soft delete product    | Admin |

### Warehouses

| Method | Endpoint                | Description                | Roles                              |
| ------ | ------------------------ | ---------------------------- | ------------------------------------ |
| GET    | `/api/warehouses`       | Get all warehouses          | Admin, RegionalManager               |
| GET    | `/api/warehouses/{id}`  | Get warehouse by id         | Admin, RegionalManager, WarehouseManager |
| POST   | `/api/warehouses`       | Create warehouse            | Admin |
| PUT    | `/api/warehouses/{id}`  | Update warehouse            | Admin |
| DELETE | `/api/warehouses/{id}`  | Soft delete warehouse (blocked if it still has active zones) | Admin |

### Zones

Nested under a warehouse.

| Method | Endpoint                                         | Description             | Roles                                          |
| ------ | -------------------------------------------------- | -------------------------- | ------------------------------------------------- |
| GET    | `/api/warehouses/{warehouseId}/zones`             | Get all zones in a warehouse | Admin, RegionalManager, WarehouseManager, Operator |
| GET    | `/api/warehouses/{warehouseId}/zones/{zoneId}`    | Get zone by id             | Admin, RegionalManager, WarehouseManager, Operator |
| POST   | `/api/warehouses/{warehouseId}/zones`             | Create zone                | Admin |
| PUT    | `/api/warehouses/{warehouseId}/zones/{zoneId}`    | Update zone                | Admin |
| DELETE | `/api/warehouses/{warehouseId}/zones/{zoneId}`    | Soft delete zone (blocked if it still has active locations) | Admin |

### Locations

Nested under a zone.

| Method | Endpoint                                       | Description                 | Roles                                          |
| ------ | ------------------------------------------------ | ------------------------------ | ------------------------------------------------- |
| GET    | `/api/zones/{zoneId}/locations`                 | Get all locations in a zone   | Admin, RegionalManager, WarehouseManager, Operator |
| GET    | `/api/zones/{zoneId}/locations/{locationId}`    | Get location by id            | Admin, RegionalManager, WarehouseManager, Operator |
| POST   | `/api/zones/{zoneId}/locations`                 | Create location               | Admin |
| PUT    | `/api/zones/{zoneId}/locations/{locationId}`    | Update location                | Admin |
| DELETE | `/api/zones/{zoneId}/locations/{locationId}`    | Soft delete location (blocked if it still has inventory assigned) | Admin |

### Inventory

| Method | Endpoint             | Description                                      | Roles                    |
| ------ | --------------------- | --------------------------------------------------- | -------------------------- |
| GET    | `/api/inventory`      | Get inventory (results scoped by role)             | Authenticated              |
| GET    | `/api/inventory/{id}` | Get inventory record by id                          | Authenticated              |
| POST   | `/api/inventory`      | Assign a product to a location                      | Admin, WarehouseManager    |
| DELETE | `/api/inventory/{id}` | Unassign a product from a location (only if quantity is 0) | Admin |

### Stock Movements

An append-only ledger — movements can be created and read, never updated or deleted. Creating one automatically adjusts the related inventory's quantity.

| Method | Endpoint                    | Description                            | Roles                              |
| ------ | ----------------------------- | ----------------------------------------- | ------------------------------------- |
| GET    | `/api/stockmovements`        | Get stock movements (results scoped by role) | Authenticated |
| GET    | `/api/stockmovements/{id}`   | Get stock movement by id                | Authenticated |
| POST   | `/api/stockmovements`        | Register a movement (Inbound / Outbound / Adjustment) | Admin, WarehouseManager, Operator (Adjustment: Admin, WarehouseManager only) |

---

## Query Parameters

All list endpoints (Products, Warehouses, Zones, Locations, Inventory, Stock Movements) support pagination, and most support filtering/sorting relevant to that resource.

Example:

```
GET /api/products?pageNumber=1&pageSize=10&name=laptop&sortBy=price&descending=true
```

Common parameters:

* pageNumber
* pageSize
* sortBy
* descending

Resource-specific filters include `name` / `sku` (Products), `city` / `country` (Warehouses), `type` (Zones), and role-based scoping filters on Inventory / Stock Movements (applied automatically based on the caller's role, not passed by the client).

---

## API Response

Successful responses follow a consistent structure:

```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": {},
  "errors": null,
  "timestamp": "2026-06-23T12:00:00Z"
}
```

---

## Logging

The application uses Serilog to provide structured logging.

Logs include:

* Request information
* Exceptions
* Stack traces
* Correlation / Trace IDs

---

## Validation

Validation is handled using FluentValidation.

Examples:

* Required fields
* Price must be greater than zero
* Stock cannot be negative
* Duplicate SKU prevention

---

## Running the Project

### Option A: Docker (recommended)

No local SQL Server or .NET SDK needed — this spins up the API and a SQL Server container together, and applies EF Core migrations automatically on startup.

```bash
git clone https://github.com/akai0411/WarehouseManagementSystem.git
cd WarehouseManagementSystem
docker compose up --build
```

Swagger will be available at:

```
http://localhost:8080/swagger
```

The database is seeded automatically on first run (sample warehouses, zones, locations, products and users — see `WarehouseManagementSystem.Api/Data/DataSeeder.cs`).

### Option B: Local .NET SDK

```bash
git clone https://github.com/akai0411/WarehouseManagementSystem.git
cd WarehouseManagementSystem
```

Update the `DefaultConnection` value in `appsettings.json` to point at your local SQL Server instance, then:

```bash
dotnet ef database update --project WarehouseManagementSystem.Infrastructure --startup-project WarehouseManagementSystem.Api
dotnet run --project WarehouseManagementSystem.Api
```

---

## Running Unit Tests

```bash
dotnet test
```

---

## Future Improvements

* GitHub Actions CI/CD
* Integration tests
* Health checks
* API versioning
* Response caching

---

## Learning Objectives

This project was built to strengthen knowledge in:

* Clean Architecture
* SOLID Principles
* Repository Pattern
* CQRS
* REST API Design
* Entity Framework Core
* Validation
* Logging
* Testing
* Software maintainability

---

## Author

Jorge Margolles

[LinkedIn](https://www.linkedin.com/in/jorge-developer-programmer/)

---

## License

This project is available for educational and portfolio purposes.
