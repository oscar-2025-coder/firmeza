# Imagen base para producción (ASP.NET Core runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen para build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos para aprovechar caché de capas
COPY ["Firmeza.sln", "./"]
COPY ["Firmeza.Admin/Firmeza.Admin.csproj", "Firmeza.Admin/"]
COPY ["Firmeza.Api/Firmeza.Api.csproj", "Firmeza.Api/"]
COPY ["Firmeza.Infrastructure/Firmeza.Infrastructure.csproj", "Firmeza.Infrastructure/"]
COPY ["Firmeza.Tests/Firmeza.Tests.csproj", "Firmeza.Tests/"]

# Restaurar dependencias
RUN dotnet restore "Firmeza.Admin/Firmeza.Admin.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar
WORKDIR "/src/Firmeza.Admin"
RUN dotnet build "Firmeza.Admin.csproj" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "Firmeza.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final (runtime)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Admin.dll"]
