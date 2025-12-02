# Firmeza - E-Commerce Platform

E-commerce system for selling construction materials and heavy machinery rental.

**🔗 GitHub Repository**: [https://github.com/oscar-2025-coder/firmeza](https://github.com/oscar-2025-coder/firmeza)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-19.2-blue.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-8.0-blue.svg)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED.svg)

---

## 📋 Table of Contents

- [Features](#-features)
- [Technologies](#-technologies)
- [Prerequisites](#-prerequisites)
- [Installation and Setup](#-installation-and-setup)
- [SMTP Configuration](#-smtp-configuration)
- [Project Structure](#-project-structure)
- [API Endpoints](#-api-endpoints)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

- **JWT Authentication**: Secure registration and login for customers
- **Product Catalog**: Browse and search products
- **Shopping Cart**: Product management with automatic total calculation (19% VAT)
- **Checkout**: Order processing with email confirmation
- **Email Notifications**: Automatic purchase receipt delivery via SMTP
- **Modern UI/UX**: Responsive interface with Tailwind CSS

---

## 🛠 Technologies

### Backend
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database (Neon.tech)
- **AutoMapper** - Object-to-object mapping
- **JWT** - Authentication
- **SMTP (Gmail)** - Email service

### Frontend
- **React 19.2** - UI library
- **Vite 7.2** - Build tool
- **Tailwind CSS 3.4** - CSS framework
- **React Router 7.9** - Navigation
- **Axios 1.13** - HTTP client
- **jwt-decode** - Token decoding

---

## 📦 Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/)
- PostgreSQL database account (recommended: [Neon.tech](https://neon.tech/))
- Gmail account for SMTP (or alternative SMTP server)

---

## 🚀 Quick Start

> 💡 **Hybrid Deployment**: This project uses a **hybrid approach** - the backend (API + Admin) runs in Docker, while the frontend must be run locally with `npm run dev` due to Docker packaging limitations.

### Prerequisites
- Docker and Docker Compose installed
- Node.js 18+ installed
- `.env` file configured (see below)

### Run the Project

**Terminal 1 - Backend (Docker):**
```bash
docker compose up --build firmeza-tests firmeza-api firmeza-admin
```

**Terminal 2 - Frontend (Local):**
```bash
cd firmeza-client
npm install
npm run dev
```

**Access:**
- Frontend: http://localhost:5173
- API: http://localhost:8081
- Swagger: http://localhost:8081/swagger
- Admin Panel: http://localhost:8080

---

## 🔧 Detailed Installation and Setup

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/firmeza.git
cd firmeza
```

### 2. Configure Environment Variables

Create a `.env` file in the project root:

```env
# Database
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
```

> ⚠️ **Note**: For Gmail, you must generate an "App Password" in your Google account. Do not use your main password.

### 3. Run Backend (API)

#### Option A: With Docker (Recommended)

```bash
docker compose up -d firmeza-api
```

The API will be available at:
- **API**: http://localhost:8081
- **Swagger**: http://localhost:8081/swagger

#### Option B: Without Docker (Local Development)

```bash
cd Firmeza.Api
dotnet restore
dotnet run
```

### 4. Run Frontend

> ⚠️ **Important**: The frontend must be run locally with `npm run dev` due to Docker packaging issues with Node.js base images. It cannot be containerized at this time.

```bash
cd firmeza-client
npm install
npm run dev
```

The frontend will be available at: **http://localhost:5173**

---

## 📧 SMTP Configuration

The system uses SMTP to send purchase confirmation emails.

### Gmail

1. Enable two-step verification in your Google account
2. Generate an "App Password":
   - Go to https://myaccount.google.com/security
   - Search for "App passwords"
   - Create a new one for "Mail"
3. Use this password in `SMTP_PASSWORD`

### Other SMTP Provider

To use another service (e.g., SendGrid, Mailgun, corporate server):

1. Update environment variables in `.env`:
   ```env
   SMTP_HOST=smtp.your-server.com
   SMTP_PORT=587
   SMTP_EMAIL=your-email@domain.com
   SMTP_PASSWORD=your-password
   SMTP_ENABLE_SSL=true
   ```

2. No code changes required. The system is designed to be SMTP provider-agnostic.

---

## 📁 Project Structure

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
├── Firmeza.Tests/            # Unit tests
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

---

## 🔌 API Endpoints

### Authentication

- `POST /api/Auth/register` - Register new customer
- `POST /api/Auth/login` - Login

### Products

- `GET /api/Products` - List all products
- `GET /api/Products/{id}` - Get product by ID
- `GET /api/Products/search` - Search products

### Sales

- `POST /api/Sales` - Create new sale (requires authentication)
- `GET /api/Sales/{id}` - Get sale by ID

> 📖 Complete documentation available at Swagger: **http://localhost:8081/swagger**

---

## 🔐 Test Users

### Customer

To create a customer, use the `/api/Auth/register` endpoint or the frontend registration page.

---

## 🐳 Docker Deployment

### Build and Run the Full Stack

```bash
docker compose up -d
```

This will start:
- **firmeza-api** on port 8081
- **firmeza-admin** on port 8080

### Rebuild Images

```bash
docker compose up -d --build
```

### View Logs

```bash
docker logs firmeza-api
docker logs firmeza-admin
```

### Stop Services

```bash
docker compose down
```

---

## 🧪 Testing

### Backend

```bash
cd Firmeza.Tests
dotnet test
```

### Frontend

```bash
cd firmeza-client
npm run test
```

---

## 🤝 Contributing

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License. See the `LICENSE` file for details.

---

## 👥 Authors

- **Oscar Leonardo Ochoa Perez** - Full Stack Developer

---

## 📞 Support

To report bugs or request new features, open an [issue](https://github.com/your-username/firmeza/issues).

---

**Made with ❤️ by the Firmeza team!**
