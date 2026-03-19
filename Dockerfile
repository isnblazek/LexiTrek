# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY LexiTrek.slnx .
COPY src/LexiTrek.Domain/LexiTrek.Domain.csproj src/LexiTrek.Domain/
COPY src/LexiTrek.Shared/LexiTrek.Shared.csproj src/LexiTrek.Shared/
COPY src/LexiTrek.Application/LexiTrek.Application.csproj src/LexiTrek.Application/
COPY src/LexiTrek.Infrastructure/LexiTrek.Infrastructure.csproj src/LexiTrek.Infrastructure/
COPY src/LexiTrek.Web/LexiTrek.Web.csproj src/LexiTrek.Web/
COPY src/LexiTrek.Api/LexiTrek.Api.csproj src/LexiTrek.Api/
COPY tests/LexiTrek.Tests/LexiTrek.Tests.csproj tests/LexiTrek.Tests/

RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish src/LexiTrek.Api/LexiTrek.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LexiTrek.Api.dll"]
