# Warehouse Management System API

A RESTful Warehouse Management System built with **ASP.NET Core 8** following **Clean Architecture** principles. The project demonstrates scalable backend development practices including CQRS, repository pattern, validation, logging, exception handling, pagination, filtering, sorting, and unit testing.

---

## Overview

This API allows users to manage warehouse products while maintaining a clean and maintainable architecture. It was developed as a portfolio project to demonstrate modern .NET backend development practices and software engineering principles.

---

## Features

* Product management (CRUD)
* Soft delete
* SKU uniqueness validation
* Pagination
* Filtering by product name and SKU
* Sorting (Name, SKU, Price)
* Global exception handling
* Structured logging with Serilog
* Swagger / OpenAPI documentation
* XML API documentation
* FluentValidation
* Standard API response wrapper
* Unit testing with xUnit v3 and Moq

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

### Products

| Method | Endpoint             | Description         |
| ------ | -------------------- | ------------------- |
| GET    | `/api/products`      | Get all products    |
| GET    | `/api/products/{id}` | Get product by id   |
| POST   | `/api/products`      | Create product      |
| PUT    | `/api/products/{id}` | Update product      |
| DELETE | `/api/products/{id}` | Soft delete product |

---

## Query Parameters

Example:

```
GET /api/products?pageNumber=1&pageSize=10&name=laptop&sortBy=price&descending=true
```

Supported parameters:

* pageNumber
* pageSize
* name
* sku
* sortBy
* descending

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

### Clone the repository

```bash
git clone https://github.com/yourusername/WarehouseManagementSystem.git
```

### Navigate to the solution

```bash
cd WarehouseManagementSystem
```

### Update the connection string

Modify the `DefaultConnection` value in `appsettings.json`.

### Apply migrations

```bash
dotnet ef database update
```

### Run the API

```bash
dotnet run --project WarehouseManagementSystem.Api
```

Swagger will be available at:

```
https://localhost:xxxx/swagger
```

---

## Running Unit Tests

```bash
dotnet test
```

---

## Future Improvements

* JWT Authentication
* Role-based Authorization
* Docker support
* GitHub Actions CI/CD
* Integration Tests
* Health Checks
* API Versioning
* Response Caching

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

## License

This project is available for educational and portfolio purposes.
