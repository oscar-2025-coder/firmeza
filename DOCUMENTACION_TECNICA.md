# Resumen Técnico del Proyecto Firmeza

Este documento sirve como guía puntual para la sustentación técnica de la solución.

## 1. Arquitectura General
El sistema sigue una **Arquitectura en Capas (N-Tier)** con principios de **Clean Architecture**, diseñada para ser modular, escalable y fácil de mantener.
- **Separación de Responsabilidades:** El Frontend (Cliente) está desacoplado del Backend (API).
- **Comunicación:** Se realiza vía **API REST** utilizando JSON.

## 2. Estructura de la Solución (Backend .NET)

### 🟢 Firmeza.Api (Capa de Presentación)
- **Rol:** Es la puerta de entrada del Backend.
- **Contenido:** Controladores (`Controllers`) que reciben peticiones HTTP y DTOs (Data Transfer Objects) para validar datos de entrada/salida.
- **Tecnología:** ASP.NET Core Web API.

### 🔵 Firmeza.Infrastructure (Capa de Dominio e Infraestructura)
- **Rol:** El "cerebro" y el acceso a datos.
- **Contenido:**
    - **Entities:** Modelos que representan las tablas de la base de datos.
    - **Data:** Configuración de Entity Framework y el DbContext.
    - **Services:** Lógica de negocio (validaciones, cálculos, envío de correos).
- **Ventaja:** Centraliza la lógica para que no esté dispersa en los controladores.

### 🟠 Firmeza.Admin (Panel Administrativo)
- **Rol:** Interfaz para gestión interna (Backoffice).
- **Tecnología:** ASP.NET Core MVC / Razor Pages (Renderizado en servidor).
- **Uso:** Permite a los administradores gestionar usuarios y configuraciones sin usar la API directamente.

### 🟣 Firmeza.Tests (Aseguramiento de Calidad)
- **Rol:** Garantizar que el código funcione correctamente antes de desplegar.
- **Contenido:** Pruebas unitarias y de integración (xUnit) que validan la lógica crítica del sistema.

## 3. Cliente Web (Frontend)

### ⚛️ firmeza-client
- **Rol:** Interfaz de usuario para el cliente final.
- **Tecnología:** **React** (Biblioteca de UI) + **Vite** (Build tool rápido).
- **Características:** Es una **SPA (Single Page Application)**, lo que ofrece una experiencia fluida sin recargas de página constantes.

## 4. Infraestructura y Despliegue (Docker)

El proyecto utiliza **Docker Compose** para orquestar todos los servicios (`api`, `admin`, `client`, `db`).

- **Entorno de Producción (Docker):**
    - El `firmeza-client` se compila (`npm run build`) a archivos estáticos optimizados y se sirve con un servidor ligero. Esto simula el entorno real de entrega.
- **Entorno de Desarrollo (Local):**
    - Se usa `npm run dev` fuera de Docker para aprovechar el **Hot Reload** (recarga instantánea al editar código), agilizando la programación.
