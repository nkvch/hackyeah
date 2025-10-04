FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Api/UknfPlatform.Api/UknfPlatform.Api.csproj", "Api/UknfPlatform.Api/"]
COPY ["Application/UknfPlatform.Application.Auth/UknfPlatform.Application.Auth.csproj", "Application/UknfPlatform.Application.Auth/"]
COPY ["Application/UknfPlatform.Application.Shared/UknfPlatform.Application.Shared.csproj", "Application/UknfPlatform.Application.Shared/"]
COPY ["Domain/UknfPlatform.Domain.Auth/UknfPlatform.Domain.Auth.csproj", "Domain/UknfPlatform.Domain.Auth/"]
COPY ["Domain/UknfPlatform.Domain.Shared/UknfPlatform.Domain.Shared.csproj", "Domain/UknfPlatform.Domain.Shared/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Persistence/UknfPlatform.Infrastructure.Persistence.csproj", "Infrastructure/UknfPlatform.Infrastructure.Persistence/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Identity/UknfPlatform.Infrastructure.Identity.csproj", "Infrastructure/UknfPlatform.Infrastructure.Identity/"]

# Restore dependencies
RUN dotnet restore "Api/UknfPlatform.Api/UknfPlatform.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/Api/UknfPlatform.Api"
RUN dotnet build "UknfPlatform.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UknfPlatform.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "UknfPlatform.Api.dll"]

