# ============================
# 1) Runtime Image
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# ============================
# 2) Build Image
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiamos SOLO los csproj necesarios
COPY ["Firmeza.Api/Firmeza.Api.csproj", "Firmeza.Api/"]
COPY ["Firmeza.Infrastructure/Firmeza.Infrastructure.csproj", "Firmeza.Infrastructure/"]

# Restauramos dependencias
RUN dotnet restore "Firmeza.Api/Firmeza.Api.csproj"

# Copiamos SOLO el código fuente de API e Infrastructure
COPY Firmeza.Api/. Firmeza.Api/
COPY Firmeza.Infrastructure/. Firmeza.Infrastructure/

# Compilamos
WORKDIR "/src/Firmeza.Api"
RUN dotnet build "Firmeza.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ============================
# 3) Publish Image
# ============================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Firmeza.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ============================
# 4) Final Runtime Image
# ============================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Api.dll"]
