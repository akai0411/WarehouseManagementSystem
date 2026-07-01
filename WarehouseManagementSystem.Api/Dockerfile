# syntax=docker/dockerfile:1

# ---- Build stage ----
# Uses the full SDK image (includes the compiler/tooling) only to build;
# it never ships in the final image, keeping it small.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the .csproj files first so Docker can cache "dotnet restore"
# as its own layer. Restore only reruns when a .csproj actually changes,
# not on every code edit.
COPY WarehouseManagementSystem.Api/*.csproj WarehouseManagementSystem.Api/
COPY WarehouseManagementSystem.Application/*.csproj WarehouseManagementSystem.Application/
COPY WarehouseManagementSystem.Domain/*.csproj WarehouseManagementSystem.Domain/
COPY WarehouseManagementSystem.Infrastructure/*.csproj WarehouseManagementSystem.Infrastructure/

RUN dotnet restore WarehouseManagementSystem.Api/WarehouseManagementSystem.Api.csproj

# Now copy the rest of the source and publish a self-contained release build.
COPY WarehouseManagementSystem.Api/ WarehouseManagementSystem.Api/
COPY WarehouseManagementSystem.Application/ WarehouseManagementSystem.Application/
COPY WarehouseManagementSystem.Domain/ WarehouseManagementSystem.Domain/
COPY WarehouseManagementSystem.Infrastructure/ WarehouseManagementSystem.Infrastructure/

RUN dotnet publish WarehouseManagementSystem.Api/WarehouseManagementSystem.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
# Uses the lightweight ASP.NET runtime image (no SDK/compiler) to keep the
# final image small and reduce attack surface.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WarehouseManagementSystem.Api.dll"]
