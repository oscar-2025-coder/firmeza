## Firmeza.Admin – Administrative Panel for the Firmeza System

Firmeza.Admin is an administrative panel built with ASP.NET Core 8 (Razor Pages + MVC), using Identity, Entity Framework Core, PostgreSQL, EPPlus, and QuestPDF.

This panel provides full management of products, customers, sales, and bulk data import, serving as the official administrative module of the Firmeza ecosystem.

## Main Features
## Authentication & Security

Administrator registration via Identity.

Login, roles, and authorization.

Administrator role with full access to the panel.

Customer users cannot access the Admin panel.

## Dashboard

Total number of products.

Total number of customers.

Total number of sales.

Revenue from the last 30 days.

Modern and responsive design.

## Product Management

Complete CRUD with validations.

Unique SKU enforcement.

Enable/disable products.

Export to Excel.

Generate PDF product reports.

## Customer Management

Full CRUD with validations:

Unique document number

Unique email

Valid age

Linked to sales.

Export to Excel and PDF.

Safe deletion rules when customers have associated sales.

## Sales Management

Fast and intuitive sale creation.

Dynamic customer and product selection.

Automatic calculation of:

Subtotal

Tax

Total

Multiple sale items support.

Automatic PDF receipt generation.

Receipts available for download.

## Bulk Import (Excel)

Upload .xlsx files.

Automatic data normalization.

Detects errors before inserting.

Supports massive insertion of:

Products

Customers

Sales

In-memory validation before saving.

## Project Structure

``````
Firmeza.Admin/
│
├── Controllers/
│   ├── AdminController.cs
│   ├── ProductsController.cs
│   ├── CustomersController.cs
│   ├── SalesController.cs
│   └── DataImportController.cs
│
├── Areas/
│   └── Identity/...
│
├── Data/
│   ├── ApplicationDbContext.cs
│   └── IdentitySeeder.cs
│
├── Models/
│   ├── Product.cs
│   ├── Customer.cs
│   ├── Sale.cs
│   └── SaleItem.cs
│
├── Services/
│   └── Pdf/ReceiptPdfService.cs
│
├── ViewModels/
├── Views/
└── wwwroot/

## Entity–Relationship Model (ERD)
+------------------+          +------------------+
|     Products     |          |    Customers     |
+------------------+          +------------------+
| Id (PK)          |          | Id (PK)          |
| Name             |          | FullName         |
| Sku              |          | DocumentNumber   |
| UnitPrice        |          | Email            |
| Stock            |          | PhoneNumber      |
| IsActive         |          | Age              |
+------------------+          +------------------+
|                          |
| 1                        | 1
|                          |
v                          v
+--------------------------------------+
|                Sales                 |
+--------------------------------------+
| Id (PK)                              |
| CustomerId (FK → Customers)          |
| Date                                  |
| Subtotal                              |
| Tax                                   |
| Total                                 |
| ReceiptFileName                       |
| Notes                                 |
+--------------------------------------+
|
| 1
|
v
+-------------------------------+
|           SaleItems           |
+-------------------------------+
| Id (PK)                       |
| SaleId (FK → Sales)           |
| ProductId (FK → Products)     |
| Quantity                       |
| UnitPrice                      |
| Amount                         |
+-------------------------------+

## Simplified Class Diagram
Product
└── Guid Id
└── string Name
└── string Sku
└── decimal UnitPrice
└── int Stock
└── bool IsActive

Customer
└── Guid Id
└── string FullName
└── string DocumentNumber
└── string Email
└── string PhoneNumber
└── int Age
└── bool IsActive

Sale
└── Guid Id
└── Guid CustomerId
└── DateTimeOffset Date
└── decimal Subtotal
└── decimal Tax
└── decimal Total
└── string ReceiptFileName
└── ICollection<SaleItem> Items

SaleItem
└── Guid Id
└── Guid SaleId
└── Guid ProductId
└── int Quantity
└── decimal UnitPrice
└── decimal Amount

## Local Installation
1. Clone the project
   git clone https://github.com/your-repo/Firmeza.Admin.git
   cd Firmeza.Admin

2. Configure appsettings.json
   "ConnectionStrings": {
   "DefaultConnection": "Host=localhost;Port=5432;Database=firmeza;Username=postgres;Password=yourpassword"
   }

3. Apply migrations
   dotnet ef database update

4. Run the project
   dotnet run


Access the admin panel at:
➡ http://localhost:5148

## Installation with Docker
1. Create docker-compose.yml
   version: '3.9'

services:
db:
image: postgres:15
environment:
POSTGRES_USER: postgres
POSTGRES_PASSWORD: 1234
POSTGRES_DB: firmeza
ports:
- "5432:5432"
volumes:
- pg_data:/var/lib/postgresql/data

admin:
build: .
ports:
- "5148:80"
environment:
ConnectionStrings__DefaultConnection: "Host=db;Database=firmeza;Username=postgres;Password=1234"
depends_on:
- db

volumes:
pg_data:

## Run containers
   docker compose up --build

## License

MIT License.

## Author

Oscar Leonardo Ochoa Pérez
Firmeza Project — Administrative Module