# Firmeza - E-Commerce Platform

Sistema de comercio electrónico para venta de materiales de construcción y alquiler de maquinaria pesada.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-19.2-blue.svg)

---

## 📋 Índice

- [Características](#-características)
- [Tecnologías](#-tecnologías)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Ejecución](#-instalación-y-ejecución)
- [Configuración SMTP](#-configuración-smtp)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [API Endpoints](#-api-endpoints)
- [Contribución](#-contribución)
- [Licencia](#-licencia)

---

## ✨ Características

- **Autenticación JWT**: Registro e inicio de sesión seguro para clientes
- **Catálogo de Productos**: Navegación y búsqueda de productos
- **Carrito de Compras**: Gestión de productos con cálculo automático de totales (IVA 19%)
- **Checkout**: Procesamiento de órdenes con confirmación por correo electrónico
- **Notificaciones Email**: Envío automático de comprobantes de compra vía SMTP
- **UI/UX Moderna**: Interfaz responsive con Tailwind CSS

---

## 🛠 Tecnologías

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM
- **PostgreSQL** - Base de datos (Neon.tech)
- **AutoMapper** - Mapeo objeto-objeto
- **JWT** - Autenticación
- **SMTP (Gmail)** - Servicio de correo

### Frontend
- **React 19.2** - Librería UI
- **Vite 7.2** - Build tool
- **Tailwind CSS 3.4** - Framework CSS
- **React Router 7.9** - Navegación
- **Axios 1.13** - Cliente HTTP
- **jwt-decode** - Decodificación de tokens

---

## 📦 Requisitos Previos

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker](https://www.docker.com/) y [Docker Compose](https://docs.docker.com/compose/)
- Cuenta de base de datos PostgreSQL (se recomienda [Neon.tech](https://neon.tech/))
- Cuenta de Gmail para SMTP (o servidor SMTP alternativo)

---

## 🚀 Instalación y Ejecución

### 1. Clonar el Repositorio

```bash
git clone https://github.com/tu-usuario/firmeza.git
cd firmeza
```

### 2. Configurar Variables de Entorno

Crear un archivo `.env` en la raíz del proyecto:

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
SMTP_EMAIL=tu-email@gmail.com
SMTP_PASSWORD=tu-app-password
SMTP_ENABLE_SSL=true
```

> ⚠️ **Nota**: Para Gmail, debes generar una "Contraseña de aplicación" en tu cuenta de Google. No uses tu contraseña principal.

### 3. Ejecutar Backend (API)

#### Opción A: Con Docker (Recomendado)

```bash
docker compose up -d firmeza-api
```

La API estará disponible en:
- **API**: http://localhost:8081
- **Swagger**: http://localhost:8081/swagger

#### Opción B: Sin Docker (Desarrollo Local)

```bash
cd Firmeza.Api
dotnet restore
dotnet run
```

### 4. Ejecutar Frontend

```bash
cd firmeza-client
npm install
npm run dev
```

El frontend estará disponible en: **http://localhost:5173**

---

## 📧 Configuración SMTP

El sistema utiliza SMTP para enviar correos electrónicos de confirmación de compra.

### Gmail

1. Habilita la verificación en dos pasos en tu cuenta de Google
2. Genera una "Contraseña de aplicación":
   - Ve a https://myaccount.google.com/security
   - Busca "Contraseñas de aplicaciones"
   - Crea una nueva para "Correo"
3. Usa esta contraseña en `SMTP_PASSWORD`

### Otro Proveedor SMTP

Para usar otro servicio (ej. SendGrid, Mailgun, servidor corporativo):

1. Actualiza las variables de entorno en `.env`:
   ```env
   SMTP_HOST=smtp.tu-servidor.com
   SMTP_PORT=587
   SMTP_EMAIL=tu-email@dominio.com
   SMTP_PASSWORD=tu-password
   SMTP_ENABLE_SSL=true
   ```

2. No se requiere ningún cambio en el código. El sistema está diseñado para ser agnóstico del proveedor SMTP.

---

## 📁 Estructura del Proyecto

```
firmeza/
├── Firmeza.Api/              # API REST (ASP.NET Core)
│   ├── Controllers/          # Endpoints de la API
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Program.cs            # Configuración de la aplicación
│   └── Properties/
├── Firmeza.Infrastructure/   # Capa de infraestructura
│   ├── Data/                 # DbContext y configuración EF
│   ├── Entities/             # Entidades del dominio
│   ├── Identity/             # Configuración de Identity
│   └── Services/             # Servicios (Email, etc.)
├── Firmeza.Admin/            # Panel administrativo (Razor Pages)
├── Firmeza.Tests/            # Pruebas unitarias
├── firmeza-client/           # Frontend (React + Vite)
│   ├── src/
│   │   ├── api/              # Servicios de API
│   │   ├── components/       # Componentes reutilizables
│   │   ├── context/          # Contextos (Auth, Cart)
│   │   ├── pages/            # Páginas de la aplicación
│   │   └── main.jsx          # Punto de entrada
│   ├── public/
│   └── package.json
├── docker-compose.yml        # Configuración de Docker
└── .env                      # Variables de entorno (no versionado)
```

---

## 🔌 API Endpoints

### Autenticación

- `POST /api/Auth/register` - Registrar nuevo cliente
- `POST /api/Auth/login` - Iniciar sesión

### Productos

- `GET /api/Products` - Listar todos los productos
- `GET /api/Products/{id}` - Obtener producto por ID
- `GET /api/Products/search` - Buscar productos

### Ventas

- `POST /api/Sales` - Crear nueva venta (requiere autenticación)
- `GET /api/Sales/{id}` - Obtener venta por ID

> 📖 Documentación completa disponible en Swagger: **http://localhost:8081/swagger**

---

## 🔐 Usuarios de Prueba

### Cliente

Para crear un cliente, usa el endpoint `/api/Auth/register` o la página de registro del frontend.

---

## 🐳 Despliegue con Docker

### Construir y Ejecutar Todo el Stack

```bash
docker compose up -d
```

Esto levantará:
- **firmeza-api** en puerto 8081
- **firmeza-admin** en puerto 8080

### Reconstruir imágenes

```bash
docker compose up -d --build
```

### Ver logs

```bash
docker logs firmeza-api
docker logs firmeza-admin
```

### Detener servicios

```bash
docker compose down
```

---

## 🧪 Pruebas

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

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

---

## 👥 Autores

- **Equipo Firmeza** - Desarrollo inicial

---

## 📞 Soporte

Para reportar bugs o solicitar nuevas funcionalidades, abre un [issue](https://github.com/tu-usuario/firmeza/issues).

---

**¡Hecho con ❤️ por el equipo Firmeza!**
