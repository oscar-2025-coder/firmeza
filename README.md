# Firmeza - E-Commerce Platform

E-commerce system for selling construction materials and heavy machinery rental.

**🔗 GitHub Repository**: [https://github.com/oscar-2025-coder/firmeza](https://github.com/oscar-2025-coder/firmeza)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-19.2-blue.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon.tech-blue.svg)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED.svg)

---

## 📋 Table of Contents

- [Features](#-features)
- [Technologies](#-technologies)
- [Prerequisites](#-prerequisites)
- [Quick Start (Full Stack with Docker)](#-quick-start-full-stack-with-docker)
- [Detailed Installation and Setup](#-detailed-installation-and-setup)
- [SMTP Configuration](#-smtp-configuration)
- [Project Structure](#-project-structure)
- [API Endpoints](#-api-endpoints)
- [Docker Commands](#-docker-commands)
- [Testing](#-testing)
- [Contributing](#-contributing)
- [License](#-license)
- [Authors](#-authors)
- [Support](#-support)

---

## ✨ Features

- **JWT Authentication**: Secure registration and login for customers
- **Product Catalog**: Browse and search products
- **Shopping Cart**: Product management with automatic total calculation (19% VAT)
- **Checkout**: Order processing with email confirmation
- **Email Notifications**: Automatic purchase receipt delivery via SMTP
- **Modern UI/UX**: Responsive interface with Tailwind CSS
- **Admin Panel**: Product and sales management for administrators

---

## 🛠 Technologies

### Backend

- **ASP.NET Core 8.0** – Web API
- **Entity Framework Core** – ORM
- **PostgreSQL (Neon.tech)** – Cloud database
- **AutoMapper** – Object-to-object mapping
- **JWT** – Authentication
- **SMTP (Gmail)** – Email service

### Frontend

- **React 19.2** – UI library
- **Vite 7.2** – Build tool
- **Tailwind CSS 3.4** – CSS framework
- **React Router 7.9** – Navigation
- **Axios 1.13** – HTTP client
- **jwt-decode** – Token decoding

---

## 📦 Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/)
- PostgreSQL database account (recommended: [Neon.tech](https://neon.tech/))
- Gmail account for SMTP (or alternative SMTP server)

---

## 🚀 Quick Start (Full Stack with Docker)

> ✅ **Modo recomendado para calificación / demo**: levanta TODO con un solo comando usando Docker y la base de datos en Neon.

### 1. Clone the Repositor
git clone https://github.com/oscar-2025-coder/firmeza.git
cd firmeza
2. Create .env File
In the project root, create a .env file:

env
Copiar código
# Database (Neon.tech)
DB_CONNECTION=Host=your-host.neon.tech;Port=5432;Database=neondb;Username=user;Password=pass;SSL Mode=Require;Trust Server Certificate=true;

# JWT Configuration
JWT_KEY=supersecretkey_firmeza_api_2025_verystrong!
JWT_ISSUER=FirmezaAPI
JWT_AUDIENCE=FirmezaClient

# SMTP Configuration (Gmail)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_EMAIL=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_ENABLE_SSL=true
⚠️ Important:

Do not commit this file to Git.

For Gmail, you must generate an App Password (not your real password).

3. Run the Entire Stack with Docker
bash
Copiar código
docker compose up --build -d
This will start:

🛍 Store (Client): http://localhost:3000

⚙️ Admin Panel: http://localhost:8080

📚 API Swagger: http://localhost:8081/swagger

These URLs quedan fijos en el proyecto, listos para que los prueben al calificar.

## Detailed Installation and Setup
Option A: Full Stack with Docker (Recommended)
If you already created .env as above:

bash
Copiar código
docker compose up --build -d
Access:

Store (Client): http://localhost:3000

Admin Panel: http://localhost:8080

API Swagger: http://localhost:8081/swagger

Option B: Backend with Docker + Frontend in Local Dev Mode
Sometimes for development you may want React in dev mode (HMR):

Terminal 1 – Backend (Docker):

bash
Copiar código
docker compose up --build firmeza-tests firmeza-api firmeza-admin
Terminal 2 – Frontend (Local Dev):

bash
Copiar código
cd firmeza-client
npm install
npm run dev
Access:

Frontend (Vite dev server): http://localhost:5173

API: http://localhost:8081

Swagger: http://localhost:8081/swagger

Admin Panel: http://localhost:8080

## SMTP Configuration
The system uses SMTP to send purchase confirmation emails.

Gmail
Enable two-step verification in your Google account.

Generate an App Password:

Go to https://myaccount.google.com/security

Search for “App passwords”

Create a new one for Mail

Use that password in SMTP_PASSWORD in your .env.

Other SMTP Providers
To use another service (SendGrid, Mailgun, corporate server):

env
Copiar código
SMTP_HOST=smtp.your-server.com
SMTP_PORT=587
SMTP_EMAIL=your-email@domain.com
SMTP_PASSWORD=your-password
SMTP_ENABLE_SSL=true
The system is SMTP provider-agnostic: no code changes required.

## Project Structure
txt
Copiar código
```
firmeza/
├── Firmeza.Api/              # REST API (ASP.NET Core)
│   ├── Controllers/          # API endpoints
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Program.cs            # Application configuration
│   └── Properties/
├── Firmeza.Infrastructure/   # Infrastructure layer
│   ├── Data/                 # DbContext and EF configuration
│   ├── Entities/             # Domain entities
│   ├── Identity/             # Identity configuration
│   └── Services/             # Services (Email, etc.)
├── Firmeza.Admin/            # Admin panel (Razor Pages)
├── Firmeza.Tests/            # Unit tests (xUnit)
├── firmeza-client/           # Frontend (React + Vite)
│   ├── src/
│   │   ├── api/              # API services
│   │   ├── components/       # Reusable components
│   │   ├── context/          # Contexts (Auth, Cart)
│   │   ├── pages/            # Application pages
│   │   └── main.jsx          # Entry point
│   ├── public/
│   └── package.json
├── docker-compose.yml        # Docker configuration
└── .env                      # Environment variables (not versioned)
```
## API Endpoints
Authentication
POST /api/Auth/register – Register new customer

POST /api/Auth/login – Login

Products
GET /api/Products – List all products

GET /api/Products/{id} – Get product by ID

GET /api/Products/search – Search products

Sales
POST /api/Sales – Create new sale (requires authentication)

GET /api/Sales/{id} – Get sale by ID

## Complete documentation available at Swagger:
http://localhost:8081/swagger

## Docker Commands
Build and Run the Full Stack
bash
Copiar código
docker compose up -d
Starts:

firmeza-api on port 8081

firmeza-admin on port 8080

firmeza-client on port 3000

Rebuild Images
bash
Copiar código
docker compose up --build -d
View Logs
bash
Copiar código
docker logs firmeza-api
docker logs firmeza-admin
docker logs firmeza-client
Stop Services
bash
Copiar código
docker compose down
## Testing
Backend Tests
bash
Copiar código
cd Firmeza.Tests
dotnet test
Frontend Tests
bash
Copiar código
cd firmeza-client
npm run test
## Contributing
Fork the project

Create a feature branch:

bash
Copiar código
git checkout -b feature/AmazingFeature
Commit your changes:

bash
Copiar código
git commit -m "Add some AmazingFeature"
Push to the branch:

bash
Copiar código
git push origin feature/AmazingFeature
Open a Pull Request

## License
This project is licensed under the MIT License.
See the LICENSE file for details.

## Authors
Oscar Leonardo Ochoa Perez – Full Stack Developer

📞 Support
To report bugs or request new features, open an issue here:

👉 https://github.com/oscar-2025-coder/firmeza/issues

Made with ❤️ by the Firmeza team!