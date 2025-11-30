# Dockerfile para ejecutar pruebas automatizadas
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar archivos de solución y proyectos
COPY ["Firmeza.sln", "./"]
COPY ["Firmeza.Tests/Firmeza.Tests.csproj", "Firmeza.Tests/"]
COPY ["Firmeza.Api/Firmeza.Api.csproj", "Firmeza.Api/"]
COPY ["Firmeza.Infrastructure/Firmeza.Infrastructure.csproj", "Firmeza.Infrastructure/"]
COPY ["Firmeza.Admin/Firmeza.Admin.csproj", "Firmeza.Admin/"]

# Restaurar dependencias
RUN dotnet restore "Firmeza.Tests/Firmeza.Tests.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar el proyecto de pruebas
WORKDIR "/src/Firmeza.Tests"
RUN dotnet build "Firmeza.Tests.csproj" -c Release -o /app/build

# Ejecutar las pruebas
# Si alguna prueba falla, el contenedor terminará con código de salida != 0
CMD ["dotnet", "test", "Firmeza.Tests.csproj", "--configuration", "Release", "--verbosity", "normal"]
