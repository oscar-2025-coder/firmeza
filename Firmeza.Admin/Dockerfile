# Imagen base para producción (ASP.NET Core runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen para build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 📌 Primero copiamos SOLO el .csproj del proyecto (evita errores de rutas)
COPY Firmeza.Admin/Firmeza.Admin.csproj ./Firmeza.Admin/

# Restauramos dependencias
RUN dotnet restore "./Firmeza.Admin/Firmeza.Admin.csproj"

# 📌 Ahora copiamos todo el código fuente completo
COPY . .

# Compilamos
WORKDIR "/src/Firmeza.Admin"
RUN dotnet build "Firmeza.Admin.csproj" -c Release -o /app/build

# Publicamos
FROM build AS publish
RUN dotnet publish "Firmeza.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final (runtime)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Admin.dll"]
