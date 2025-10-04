# **Section 11: Infrastructure and Deployment**

Let me define the deployment architecture, containerization strategy, and CI/CD approach for the hackathon demo.

---

## **Infrastructure as Code**

- **Tool:** Docker Compose 2.x
- **Location:** `infrastructure/docker-compose.yml`
- **Approach:** Container-based deployment with all services defined in compose files

---

## **Deployment Strategy**

- **Strategy:** Container orchestration with Docker Compose
- **CI/CD Platform:** GitHub Actions (optional for hackathon, but recommended for demonstration)
- **Pipeline Configuration:** `.github/workflows/backend-ci.yml` and `frontend-ci.yml`

---

## **Docker Compose Configuration**

```yaml
# infrastructure/docker-compose.yml
version: '3.9'

services:
  # ============================================
  # Database
  # ============================================
  postgres:
    image: postgres:16.3-alpine
    container_name: uknf-postgres
    environment:
      POSTGRES_DB: uknf_platform
      POSTGRES_USER: uknf_admin
      POSTGRES_PASSWORD: UknfP@ssw0rd2025
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U uknf_admin"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - uknf-network

  # ============================================
  # Redis Cache
  # ============================================
  redis:
    image: redis:7.2-alpine
    container_name: uknf-redis
    ports:
      - "6379:6379"
    command: redis-server --appendonly yes
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - uknf-network

  # ============================================
  # RabbitMQ Message Broker
  # ============================================
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: uknf-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: uknf
      RABBITMQ_DEFAULT_PASS: rabbitmq_pass
    ports:
      - "5672:5672"    # AMQP port
      - "15672:15672"  # Management UI
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - uknf-network

  # ============================================
  # MinIO Object Storage
  # ============================================
  minio:
    image: minio/minio:latest
    container_name: uknf-minio
    environment:
      MINIO_ROOT_USER: minio_admin
      MINIO_ROOT_PASSWORD: MinioP@ssw0rd2025
    ports:
      - "9000:9000"  # API
      - "9001:9001"  # Console
    volumes:
      - minio_data:/data
    command: server /data --console-address ":9001"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - uknf-network

  # ============================================
  # MailDev (Email Testing)
  # ============================================
  maildev:
    image: maildev/maildev:latest
    container_name: uknf-maildev
    ports:
      - "1080:1080"  # Web UI
      - "1025:1025"  # SMTP
    networks:
      - uknf-network

  # ============================================
  # Backend API
  # ============================================
  backend-api:
    build:
      context: ../src/Backend
      dockerfile: ../../infrastructure/docker/backend.Dockerfile
    container_name: uknf-backend-api
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5000
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=uknf_platform;Username=uknf_admin;Password=UknfP@ssw0rd2025"
      Redis__ConnectionString: "redis:6379"
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: uknf
      RabbitMQ__Password: rabbitmq_pass
      MinIO__Endpoint: minio:9000
      MinIO__AccessKey: minio_admin
      MinIO__SecretKey: MinioP@ssw0rd2025
      Email__SmtpHost: maildev
      Email__SmtpPort: 1025
      JWT__SecretKey: "SuperSecretKeyForJWTTokenGeneration2025!@#"
      JWT__Issuer: "UknfPlatform"
      JWT__Audience: "UknfPlatformUsers"
      JWT__ExpirationMinutes: 60
    ports:
      - "5000:5000"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      minio:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    networks:
      - uknf-network

  # ============================================
  # Background Workers
  # ============================================
  workers:
    build:
      context: ../src/Backend
      dockerfile: ../../infrastructure/docker/workers.Dockerfile
    container_name: uknf-workers
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=uknf_platform;Username=uknf_admin;Password=UknfP@ssw0rd2025"
      Redis__ConnectionString: "redis:6379"
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: uknf
      RabbitMQ__Password: rabbitmq_pass
      MinIO__Endpoint: minio:9000
      MinIO__AccessKey: minio_admin
      MinIO__SecretKey: MinioP@ssw0rd2025
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      backend-api:
        condition: service_healthy
    networks:
      - uknf-network

  # ============================================
  # Frontend (Angular)
  # ============================================
  frontend:
    build:
      context: ../src/Frontend/uknf-platform-ui
      dockerfile: ../../../infrastructure/docker/frontend.Dockerfile
    container_name: uknf-frontend
    environment:
      API_URL: http://backend-api:5000/api
    ports:
      - "4200:80"
    depends_on:
      - backend-api
    networks:
      - uknf-network

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:
  minio_data:

networks:
  uknf-network:
    driver: bridge
```

---

## **Dockerfiles**

### **Backend API Dockerfile**

```dockerfile
# infrastructure/docker/backend.Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["UknfPlatform.sln", "./"]
COPY ["Api/UknfPlatform.Api/UknfPlatform.Api.csproj", "Api/UknfPlatform.Api/"]
COPY ["Application/UknfPlatform.Application.Communication/UknfPlatform.Application.Communication.csproj", "Application/UknfPlatform.Application.Communication/"]
COPY ["Application/UknfPlatform.Application.Auth/UknfPlatform.Application.Auth.csproj", "Application/UknfPlatform.Application.Auth/"]
COPY ["Application/UknfPlatform.Application.Admin/UknfPlatform.Application.Admin.csproj", "Application/UknfPlatform.Application.Admin/"]
COPY ["Application/UknfPlatform.Application.Shared/UknfPlatform.Application.Shared.csproj", "Application/UknfPlatform.Application.Shared/"]
COPY ["Domain/UknfPlatform.Domain.Communication/UknfPlatform.Domain.Communication.csproj", "Domain/UknfPlatform.Domain.Communication/"]
COPY ["Domain/UknfPlatform.Domain.Auth/UknfPlatform.Domain.Auth.csproj", "Domain/UknfPlatform.Domain.Auth/"]
COPY ["Domain/UknfPlatform.Domain.Admin/UknfPlatform.Domain.Admin.csproj", "Domain/UknfPlatform.Domain.Admin/"]
COPY ["Domain/UknfPlatform.Domain.Shared/UknfPlatform.Domain.Shared.csproj", "Domain/UknfPlatform.Domain.Shared/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Persistence/UknfPlatform.Infrastructure.Persistence.csproj", "Infrastructure/UknfPlatform.Infrastructure.Persistence/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Identity/UknfPlatform.Infrastructure.Identity.csproj", "Infrastructure/UknfPlatform.Infrastructure.Identity/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.FileStorage/UknfPlatform.Infrastructure.FileStorage.csproj", "Infrastructure/UknfPlatform.Infrastructure.FileStorage/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Messaging/UknfPlatform.Infrastructure.Messaging.csproj", "Infrastructure/UknfPlatform.Infrastructure.Messaging/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Caching/UknfPlatform.Infrastructure.Caching.csproj", "Infrastructure/UknfPlatform.Infrastructure.Caching/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Email/UknfPlatform.Infrastructure.Email.csproj", "Infrastructure/UknfPlatform.Infrastructure.Email/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.Logging/UknfPlatform.Infrastructure.Logging.csproj", "Infrastructure/UknfPlatform.Infrastructure.Logging/"]
COPY ["Infrastructure/UknfPlatform.Infrastructure.ExternalServices/UknfPlatform.Infrastructure.ExternalServices.csproj", "Infrastructure/UknfPlatform.Infrastructure.ExternalServices/"]

# Restore dependencies
RUN dotnet restore "Api/UknfPlatform.Api/UknfPlatform.Api.csproj"

# Copy remaining source code
COPY . .

# Build
WORKDIR "/src/Api/UknfPlatform.Api"
RUN dotnet build "UknfPlatform.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UknfPlatform.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run migrations on startup (for demo purposes)
ENTRYPOINT ["dotnet", "UknfPlatform.Api.dll"]
```

### **Workers Dockerfile**

```dockerfile
# infrastructure/docker/workers.Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files (similar to backend)
COPY ["UknfPlatform.sln", "./"]
# ... (same project copies as backend)

RUN dotnet restore "Workers/UknfPlatform.Workers/UknfPlatform.Workers.csproj"
COPY . .

WORKDIR "/src/Workers/UknfPlatform.Workers"
RUN dotnet build "UknfPlatform.Workers.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UknfPlatform.Workers.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UknfPlatform.Workers.dll"]
```

### **Frontend Dockerfile**

```dockerfile
# infrastructure/docker/frontend.Dockerfile
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci

# Copy source code
COPY . .

# Build Angular app
RUN npm run build -- --configuration production

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist/uknf-platform-ui/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## **Environments**

| Environment | Purpose | Configuration |
|-------------|---------|---------------|
| **Development** | Local development on developer machines | docker-compose.dev.yml, hot reload enabled, verbose logging |
| **Demo** | Hackathon demonstration | docker-compose.yml, seeded with test data, optimized for judges |
| **Production** (future) | Production deployment | docker-compose.prod.yml, environment secrets, monitoring enabled |

---

## **Environment Promotion Flow**

```
Developer Workstation
         ↓
    [git push]
         ↓
GitHub Repository
         ↓
  [GitHub Actions CI]
         ↓
   Build & Test
         ↓
  Docker Images
         ↓
   Demo Environment (docker-compose up)
```

---

## **Rollback Strategy**

- **Primary Method:** Docker Compose down/up with previous image tags
- **Trigger Conditions:** 
  - Health check failures
  - Critical bugs discovered during demo
  - Database migration failures
- **Recovery Time Objective:** < 5 minutes (stop containers, revert to previous image, restart)

**For Hackathon Demo:**
- Keep previous working state in a separate branch
- Tag working Docker images
- Volume backups for database state
- Quick rollback script: `scripts/rollback.sh`

---

## **Setup and Deployment Scripts**

### **One-Command Setup**

```bash
#!/bin/bash
# scripts/setup-dev-env.sh

echo "🚀 Setting up UKNF Communication Platform..."

# Check prerequisites
command -v docker >/dev/null 2>&1 || { echo "❌ Docker is required but not installed. Aborting." >&2; exit 1; }
command -v docker-compose >/dev/null 2>&1 || { echo "❌ Docker Compose is required but not installed. Aborting." >&2; exit 1; }

# Pull base images
echo "📦 Pulling Docker images..."
docker-compose pull

# Build application images
echo "🔨 Building application images..."
docker-compose build

# Start infrastructure services first
echo "🗄️  Starting infrastructure services..."
docker-compose up -d postgres redis rabbitmq minio maildev

# Wait for services to be healthy
echo "⏳ Waiting for services to be ready..."
sleep 15

# Run database migrations
echo "🔄 Running database migrations..."
docker-compose run --rm backend-api dotnet ef database update

# Seed database with test data
echo "🌱 Seeding database with test data..."
docker-compose run --rm backend-api dotnet run --project Infrastructure/UknfPlatform.Infrastructure.Persistence/DbSeeder

# Start application services
echo "🚀 Starting application services..."
docker-compose up -d backend-api workers frontend

# Display access information
echo ""
echo "✅ Setup complete!"
echo ""
echo "📍 Access the application:"
echo "   Frontend:        http://localhost:4200"
echo "   Backend API:     http://localhost:5000"
echo "   Swagger UI:      http://localhost:5000/swagger"
echo "   RabbitMQ UI:     http://localhost:15672 (uknf/rabbitmq_pass)"
echo "   MinIO Console:   http://localhost:9001 (minio_admin/MinioP@ssw0rd2025)"
echo "   MailDev:         http://localhost:1080"
echo ""
echo "👤 Default Credentials:"
echo "   UKNF Admin:      admin@uknf.gov.pl / Admin@123"
echo "   External User:   user@entity.com / User@123"
echo ""
echo "📋 View logs: docker-compose logs -f"
echo "🛑 Stop all:  docker-compose down"
```

### **Quick Start for Judges**

```bash
#!/bin/bash
# scripts/quick-start.sh

echo "🎯 UKNF Platform - Hackathon Demo Quick Start"
echo ""

# One command to rule them all
docker-compose up --build -d

echo ""
echo "⏳ Waiting for application to be ready (this may take 1-2 minutes)..."
sleep 60

echo ""
echo "✅ Application is ready!"
echo ""
echo "🌐 Open in browser: http://localhost:4200"
echo ""
echo "📖 See README.md for demo credentials and walkthrough"
```

---

## **Health Checks and Monitoring**

### **Application Health Endpoint**

```csharp
// Backend health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        });
        await context.Response.WriteAsync(result);
    }
});

// Health checks configuration
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "PostgreSQL")
    .AddRedis(redisConnectionString, name: "Redis")
    .AddRabbitMQ(rabbitMqConnectionString, name: "RabbitMQ")
    .AddCheck<MinioHealthCheck>("MinIO");
```

### **Service Dependencies**

```
Frontend
   ↓ depends on
Backend API
   ↓ depends on
PostgreSQL (healthy)
Redis (healthy)
RabbitMQ (healthy)
MinIO (healthy)
```

---

## **Backup and Data Persistence**

**Volumes for Data Persistence:**
- `postgres_data`: Database data
- `redis_data`: Session and cache data
- `rabbitmq_data`: Message queue data
- `minio_data`: Uploaded files

**Backup Strategy (for demo):**
```bash
# Backup all volumes
docker run --rm -v uknf_postgres_data:/data -v $(pwd)/backups:/backup alpine tar czf /backup/postgres-backup.tar.gz /data

# Restore from backup
docker run --rm -v uknf_postgres_data:/data -v $(pwd)/backups:/backup alpine tar xzf /backup/postgres-backup.tar.gz -C /
```

---

## **CI/CD Pipeline (Optional for Hackathon)**

```yaml
# .github/workflows/backend-ci.yml
name: Backend CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore src/Backend/UknfPlatform.sln
    
    - name: Build
      run: dotnet build src/Backend/UknfPlatform.sln --no-restore
    
    - name: Run unit tests
      run: dotnet test src/Backend/Tests/UknfPlatform.UnitTests --no-build --verbosity normal
    
    - name: Run integration tests
      run: dotnet test src/Backend/Tests/UknfPlatform.IntegrationTests --no-build --verbosity normal
    
    - name: Build Docker image
      run: docker build -f infrastructure/docker/backend.Dockerfile -t uknf-backend:${{ github.sha }} src/Backend
    
    - name: Generate OpenAPI spec
      run: |
        dotnet run --project src/Backend/Api/UknfPlatform.Api -- swagger tofile --output docs/api/swagger.json
```

---

## **Resource Requirements**

**Minimum System Requirements for Demo:**
- **CPU:** 4 cores
- **RAM:** 8 GB
- **Disk:** 10 GB free space
- **OS:** Linux, macOS, or Windows with WSL2

**Docker Resources:**
- PostgreSQL: 512 MB RAM
- Redis: 256 MB RAM
- RabbitMQ: 512 MB RAM
- MinIO: 512 MB RAM
- Backend API: 1 GB RAM
- Workers: 512 MB RAM
- Frontend: 256 MB RAM

**Total:** ~3.5 GB RAM allocated to containers

---

## **Rationale:**

1. **Docker Compose**: PRD requires `docker-compose.yml` - using standard compose for simplicity and portability

2. **Service Dependencies**: Health checks ensure services start in correct order (database before API, etc.)

3. **Self-Contained**: Everything runs in containers - no external dependencies needed for hackathon demo

4. **One-Command Setup**: `docker-compose up` is all judges need to run (per PRD requirement)

5. **Developer Experience**: Additional scripts for convenient development (migrations, seeding, etc.)

6. **Volume Persistence**: Data survives container restarts - important for demo continuity

7. **Environment Variables**: All configuration via environment variables - easy to change without rebuilding

8. **Health Monitoring**: Health checks provide visibility into service status

9. **Rollback Strategy**: Simple rollback for hackathon demo (stop/start with different tags)

10. **Future-Ready**: Structure supports future migration to Kubernetes or cloud services
