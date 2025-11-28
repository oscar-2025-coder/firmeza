## Firmeza.Admin – Administrative Panel

Firmeza.Admin is the administrative module of the Firmeza ecosystem, built using ASP.NET Core 8 (Razor Pages + MVC), Identity, EF Core, PostgreSQL, EPPlus and QuestPDF.

This system allows administrators to manage products, customers, sales, and bulk operations.

## 1. Features Overview
   1.1 Authentication & Security

Identity-based login

Role-based access control

Admin-only access to the panel

Customer users cannot access Razor

## 2. Dashboard

Total products

Total customers

Total sales

Last 30 days revenue

Clean and responsive UI

## 3. Product Management

CRUD with full validation

Unique SKU enforcement

Activate/deactivate products

Search and filtering

Export to Excel

Generate PDF product reports

## 4. Customer Management

Full CRUD with validation

Unique email / document / phone

Linked sales history

Excel and PDF export

Safe deletion rules

## 5. Sales Management

Complete sale creation workflow

Customer & product selection

Auto subtotal, tax & total calculation

Multiple items per sale

Automatic PDF receipt generation

Files stored in wwwroot/receipts

## 6. Bulk Import (Excel)

Upload .xlsx files

Normalizes mixed/unorganized rows

Validates before saving

Errors detected and logged

Mass import for:

Products

Customers

Sales

## 7. Project Structure
``````
   Firmeza.Admin/
   │
   ├── Controllers/
   │── Areas/Identity/
   │── Data/
   │── Models/
   │── Services/
   │── ViewModels/
   │── Views/
   └── wwwroot/
``````
## 8. Entity–Relationship (ER) Overview
   Products ──< SaleItems >── Sales ──> Customers


(Full diagram removed to keep README clean.)

## 9. Class Models (Simplified)
   9.1 Product

Id

Name

Sku

UnitPrice

Stock

IsActive

9.2 Customer

Id

FullName

DocumentNumber

Email

PhoneNumber

Age

IsActive

9.3 Sale

Id

CustomerId

Date

Subtotal / Tax / Total

ReceiptFileName

Items

9.4 SaleItem

Id

SaleId

ProductId

Quantity

UnitPrice

Amount

## 10. Local Installation
    10.1 Navigate to project
    cd Firmeza.Admin

10.2 Update appsettings.json
"DefaultConnection": "Host=localhost;Database=firmeza;Username=postgres;Password=yourpassword"

10.3 Apply migrations
dotnet ef database update

10.4 Run the project
dotnet run


Admin panel available at:

👉 http://localhost:5148

## 11. Docker Setup (Optional)
    11.1 Example docker-compose.yml

(Credentials removed)

version: '3.9'

services:
db:
image: postgres:15
environment:
POSTGRES_USER: postgres
POSTGRES_PASSWORD: yourpassword
POSTGRES_DB: firmeza
ports:
- "5432:5432"

admin:
build: .
ports:
- "5148:80"
environment:
ConnectionStrings__DefaultConnection: "Host=db;Database=firmeza;Username=postgres;Password=yourpassword"
depends_on:
- db

11.2 Run containers
docker compose up --build

## 12. License

MIT License.

## 13. Author

Oscar Leonardo Ochoa Pérez
Firmeza Project — Administrative Module