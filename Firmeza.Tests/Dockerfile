# Dockerfile for FIRMEZA TESTS

# 1) Build Image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Firmeza.sln", "./"]
COPY ["Firmeza.Api/Firmeza.Api.csproj", "Firmeza.Api/"]
COPY ["Firmeza.Admin/Firmeza.Admin.csproj", "Firmeza.Admin/"]
COPY ["Firmeza.Infrastructure/Firmeza.Infrastructure.csproj", "Firmeza.Infrastructure/"]
COPY ["Firmeza.Tests/Firmeza.Tests.csproj", "Firmeza.Tests/"]

# Restore dependencies
RUN dotnet restore "Firmeza.Tests/Firmeza.Tests.csproj"

# Copy full source code
COPY . .

# Build
WORKDIR "/src/Firmeza.Tests"
RUN dotnet build "Firmeza.Tests.csproj" -c Release -o /app/build

# 2) Test Runner
# We use the SDK image because `dotnet test` requires the SDK, not just the Runtime.
FROM build AS testrunner
WORKDIR "/src/Firmeza.Tests"

# Run tests and fail if any test fails
ENTRYPOINT ["dotnet", "test", "Firmeza.Tests.csproj", "--configuration", "Release", "--logger", "trx;LogFileName=test_results.xml"]
