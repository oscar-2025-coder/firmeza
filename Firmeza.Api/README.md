## Firmeza API – Project Documentation
## 1. Overview

Firmeza API is a RESTful backend developed in ASP.NET Core 8, designed to support the operational workflow of the Firmeza construction materials business.
This API serves as the central communication layer for internal modules such as the Razor Admin Panel and the upcoming Blazor Client Portal.

The system provides functionality for managing:

Products

Customers

Sales

User authentication and authorization with Identity + JWT

Email notifications via SMTP

The API is structured to be secure, scalable, and easy to integrate with external clients or front-end applications.

## 2. Key Features
   2.1 Authentication & Authorization

ASP.NET Identity Core

JWT Bearer tokens

Custom role management (Administrator, Customer)

Role-based access for protected endpoints

2.2 Domain Modules

Products: Full CRUD, DTO mapping, validation, search and filtering.

Customers: Registration flow, linked with Identity users, email notifications.

Sales: CRUD operations, automatic email notifications, support for future PDF generation.

2.3 Email Service

SMTP-based email delivery (Gmail by default)

Configurable via environment variables

Easily replaceable for enterprise SMTP servers without changes in core logic

2.4 Documentation

Integrated Swagger UI

JWT authentication support within Swagger

Auto-generated endpoint documentation

2.5 Unit Testing

Test project built with xUnit, FluentAssertions, Moq

Includes basic controller and domain tests

Support for InMemory EF Core provider for isolated database testing

## 3 Technologies Used
   Category	Technology
   Backend Framework	ASP.NET Core 8 (Web API)
   ORM	Entity Framework Core 8
   Database	PostgreSQL
   Authentication	Identity + JWT
   Mapping	AutoMapper
   Email	SMTP (Gmail by default)
   Documentation	Swagger / OpenAPI
   Testing	xUnit, Moq, FluentAssertions
## 4. Project Structure
``````
   Firmeza/
   │
   ├── Firmeza.Api/                 # Main Web API project
   │   ├── Controllers/
   │   ├── DTOs/
   │   ├── Mappings/
   │   ├── Program.cs
   │   └── ...
   │
   ├── Firmeza.Infrastructure/      # Persistence, Identity, Services
   │   ├── Data/
   │   ├── Identity/
   │   ├── Services/
   │   └── ...
   │
   ├── Firmeza.Domain/              # Core domain entities
   │   ├── Products/
   │   ├── Customers/
   │   ├── Sales/
   │   └── ...
   │
   ├── Firmeza.Admin/               # Razor Admin (existing)
   │
   └── Firmeza.Tests/               # Automated tests (xUnit)
   
   ``````

## 5. Environment Configuration

The API requires a .env file placed in the root of the solution:

# PostgreSQL Configuration
DB_CONNECTION=<your_postgres_connection_string>

# JWT Settings
JWT_KEY=<your_jwt_secret>
JWT_ISSUER=<your_issuer>
JWT_AUDIENCE=<your_audience>

# Email (SMTP)
EMAIL_HOST=<smtp_server>
EMAIL_PORT=<smtp_port>
EMAIL_USERNAME=<smtp_user>
EMAIL_PASSWORD=<smtp_password>
EMAIL_FROM=<sender_email>


No sensitive information should be committed to version control.

## 6. Local Installation & Execution
   Prerequisites

.NET 8 SDK

PostgreSQL database

Optional: Rider, Visual Studio, or VS Code

Steps to run locally

Clone the repository:

git clone <repo-url>


Create a .env file using the configuration section above.

Restore dependencies:

dotnet restore


Apply migrations (if needed):

dotnet ef database update --project Firmeza.Infrastructure --startup-project Firmeza.Api


Run the API:

dotnet run --project Firmeza.Api


Navigate to Swagger UI:

https://localhost:<port>/swagger

## 7. Docker Support

A basic Docker structure should be included in the root of the solution:

Dockerfile (Draft)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ./Firmeza.Api/Firmeza.Api.csproj
RUN dotnet publish ./Firmeza.Api/Firmeza.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Api.dll"]

docker-compose.yml (Draft)
version: '3.8'

services:
firmeza-api:
build: ./Firmeza.Api
container_name: firmeza_api
ports:
- "8080:8080"
environment:
DB_CONNECTION: ${DB_CONNECTION}
JWT_KEY: ${JWT_KEY}
JWT_ISSUER: ${JWT_ISSUER}
JWT_AUDIENCE: ${JWT_AUDIENCE}
depends_on:
- db

db:
image: postgres:16
container_name: firmeza_db
environment:
POSTGRES_PASSWORD: postgres
POSTGRES_USER: postgres
POSTGRES_DB: firmeza
ports:
- "5432:5432"

## 8. Testing

Run all unit tests:

dotnet test


Tests cover:

Product controller behavior

Sales logic (basic)

Domain model integrity

This provides a foundation for expanding test coverage over time.

## 9. API Documentation

Once running, Swagger provides full API documentation at:

https://localhost:<port>/swagger/index.html


This includes:

All endpoints

Request and response models

JWT authentication field

Try-out requests

## 10. Future Improvements

Full PDF generation for sales invoices

Complete test suite for all controllers and services

CI/CD integration

Production-ready Docker and Kubernetes setups

Logging and application monitoring

## 11. License and Authors

This project is developed for the Firmeza business system as part of a modular architecture including Razor Admin and Blazor Client applications.