# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all csproj files first for layer caching
COPY ServiceCity/ServiceCity.csproj ./ServiceCity/
COPY ServiceCity.Core/ServiceCity.Core.csproj ./ServiceCity.Core/
COPY ServiceCity.Data/ServiceCity.Data.csproj ./ServiceCity.Data/
COPY ServiceCity.Services/ServiceCity.Services.csproj ./ServiceCity.Services/

# Restore with explicit project reference resolution
RUN dotnet restore ServiceCity/ServiceCity.csproj

# Copy all source code
COPY ServiceCity/ ./ServiceCity/
COPY ServiceCity.Core/ ./ServiceCity.Core/
COPY ServiceCity.Data/ ./ServiceCity.Data/
COPY ServiceCity.Services/ ./ServiceCity.Services/

# Publish
RUN dotnet publish ServiceCity/ServiceCity.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

USER app

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "ServiceCity.dll"]
