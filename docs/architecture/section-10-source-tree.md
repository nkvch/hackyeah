# **Section 10: Source Tree**

Let me define the project folder structure that reflects the modular monolith architecture, CQRS pattern, and monorepo approach.

---

```plaintext
uknf-communication-platform/
├── .github/
│   └── workflows/
│       ├── backend-ci.yml                 # Backend CI/CD pipeline
│       └── frontend-ci.yml                # Frontend CI/CD pipeline
│
├── docs/
│   ├── architecture.md                    # This architecture document
│   ├── prd.md                             # Product requirements
│   ├── api/                               # API documentation
│   │   └── swagger.json                   # Generated OpenAPI spec
│   └── diagrams/                          # Architecture diagrams
│
├── src/
│   ├── Backend/
│   │   ├── UknfPlatform.sln               # Main solution file
│   │   │
│   │   ├── Api/                           # API Gateway layer
│   │   │   ├── UknfPlatform.Api/
│   │   │   │   ├── Controllers/           # REST API controllers
│   │   │   │   │   ├── AuthController.cs
│   │   │   │   │   ├── AuthorizationController.cs
│   │   │   │   │   ├── ReportsController.cs
│   │   │   │   │   ├── MessagesController.cs
│   │   │   │   │   ├── CasesController.cs
│   │   │   │   │   ├── LibraryController.cs
│   │   │   │   │   ├── FaqController.cs
│   │   │   │   │   ├── BulletinBoardController.cs
│   │   │   │   │   └── EntitiesController.cs
│   │   │   │   ├── Hubs/                  # SignalR hubs
│   │   │   │   │   ├── NotificationHub.cs
│   │   │   │   │   └── MessageHub.cs
│   │   │   │   ├── Middleware/            # Custom middleware
│   │   │   │   │   ├── EntityContextMiddleware.cs
│   │   │   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   │   │   └── AuditLoggingMiddleware.cs
│   │   │   │   ├── Filters/               # Action filters
│   │   │   │   │   └── ValidateModelStateAttribute.cs
│   │   │   │   ├── Program.cs             # Application entry point
│   │   │   │   ├── appsettings.json       # Configuration
│   │   │   │   ├── appsettings.Development.json
│   │   │   │   └── UknfPlatform.Api.csproj
│   │   │   │
│   │   │   └── UknfPlatform.Api.Gateway/  # Optional: YARP Gateway (if separate)
│   │   │       ├── appsettings.json
│   │   │       ├── Program.cs
│   │   │       └── UknfPlatform.Api.Gateway.csproj
│   │   │
│   │   ├── Application/                   # Application layer (CQRS handlers)
│   │   │   ├── UknfPlatform.Application.Communication/
│   │   │   │   ├── Reports/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   │   ├── UploadReportCommand.cs
│   │   │   │   │   │   ├── UploadReportCommandHandler.cs
│   │   │   │   │   │   ├── SubmitReportCommand.cs
│   │   │   │   │   │   ├── ContestReportCommand.cs
│   │   │   │   │   │   └── ArchiveReportCommand.cs
│   │   │   │   │   ├── Queries/
│   │   │   │   │   │   ├── GetReportsQuery.cs
│   │   │   │   │   │   ├── GetReportsQueryHandler.cs
│   │   │   │   │   │   ├── GetReportDetailsQuery.cs
│   │   │   │   │   │   └── GetMissingEntitiesQuery.cs
│   │   │   │   │   ├── Events/
│   │   │   │   │   │   ├── ReportSubmittedEvent.cs
│   │   │   │   │   │   ├── ReportValidatedEvent.cs
│   │   │   │   │   │   └── ReportValidationFailedEvent.cs
│   │   │   │   │   └── Validators/
│   │   │   │   │       ├── UploadReportCommandValidator.cs
│   │   │   │   │       └── SubmitReportCommandValidator.cs
│   │   │   │   ├── Messages/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   ├── Queries/
│   │   │   │   │   ├── Events/
│   │   │   │   │   └── Validators/
│   │   │   │   ├── Cases/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   ├── Queries/
│   │   │   │   │   ├── Events/
│   │   │   │   │   └── Validators/
│   │   │   │   ├── Library/
│   │   │   │   ├── FAQ/
│   │   │   │   ├── BulletinBoard/
│   │   │   │   └── UknfPlatform.Application.Communication.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Application.Auth/
│   │   │   │   ├── Authentication/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   │   ├── RegisterUserCommand.cs
│   │   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   │   └── ChangePasswordCommand.cs
│   │   │   │   │   ├── Queries/
│   │   │   │   │   └── Validators/
│   │   │   │   ├── Authorization/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   ├── Queries/
│   │   │   │   │   └── Policies/
│   │   │   │   ├── AccessRequests/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   ├── Queries/
│   │   │   │   │   ├── Events/
│   │   │   │   │   └── Validators/
│   │   │   │   └── UknfPlatform.Application.Auth.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Application.Admin/
│   │   │   │   ├── Users/
│   │   │   │   ├── Roles/
│   │   │   │   ├── PasswordPolicy/
│   │   │   │   └── UknfPlatform.Application.Admin.csproj
│   │   │   │
│   │   │   └── UknfPlatform.Application.Shared/
│   │   │       ├── Behaviors/              # MediatR pipeline behaviors
│   │   │       │   ├── ValidationBehavior.cs
│   │   │       │   ├── LoggingBehavior.cs
│   │   │       │   └── TransactionBehavior.cs
│   │   │       ├── DTOs/                   # Shared data transfer objects
│   │   │       ├── Mappings/               # Mapster configurations
│   │   │       │   └── MappingConfig.cs
│   │   │       ├── Interfaces/
│   │   │       │   ├── ICommand.cs
│   │   │       │   ├── IQuery.cs
│   │   │       │   └── IEventHandler.cs
│   │   │       └── UknfPlatform.Application.Shared.csproj
│   │   │
│   │   ├── Domain/                         # Domain layer (business logic)
│   │   │   ├── UknfPlatform.Domain.Communication/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Report.cs
│   │   │   │   │   ├── Message.cs
│   │   │   │   │   ├── Case.cs
│   │   │   │   │   ├── LibraryFile.cs
│   │   │   │   │   ├── FAQ.cs
│   │   │   │   │   └── BulletinBoardMessage.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── ValidationStatus.cs
│   │   │   │   │   ├── MessageStatus.cs
│   │   │   │   │   ├── CaseStatus.cs
│   │   │   │   │   └── CaseCategory.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── ReportingPeriod.cs
│   │   │   │   │   └── ValidationResult.cs
│   │   │   │   ├── Specifications/         # Business rules
│   │   │   │   │   └── ReportValidationSpec.cs
│   │   │   │   └── UknfPlatform.Domain.Communication.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Domain.Auth/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── User.cs
│   │   │   │   │   ├── Role.cs
│   │   │   │   │   ├── Permission.cs
│   │   │   │   │   ├── AccessRequest.cs
│   │   │   │   │   └── PermissionLine.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── UserType.cs
│   │   │   │   │   └── AccessRequestStatus.cs
│   │   │   │   └── UknfPlatform.Domain.Auth.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Domain.Admin/
│   │   │   │   └── UknfPlatform.Domain.Admin.csproj
│   │   │   │
│   │   │   └── UknfPlatform.Domain.Shared/
│   │   │       ├── Common/
│   │   │       │   ├── BaseEntity.cs
│   │   │       │   ├── AuditableEntity.cs
│   │   │       │   └── IAggregateRoot.cs
│   │   │       ├── Entities/
│   │   │       │   ├── Entity.cs          # Supervised entity (shared across modules)
│   │   │       │   ├── ContactGroup.cs
│   │   │       │   └── Contact.cs
│   │   │       ├── Events/
│   │   │       │   └── IDomainEvent.cs
│   │   │       └── UknfPlatform.Domain.Shared.csproj
│   │   │
│   │   ├── Infrastructure/                 # Infrastructure layer
│   │   │   ├── UknfPlatform.Infrastructure.Persistence/
│   │   │   │   ├── Contexts/
│   │   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   │   └── ApplicationDbContextFactory.cs  # For migrations
│   │   │   │   ├── Configurations/         # EF Core entity configurations
│   │   │   │   │   ├── UserConfiguration.cs
│   │   │   │   │   ├── EntityConfiguration.cs
│   │   │   │   │   ├── ReportConfiguration.cs
│   │   │   │   │   └── ... (one per entity)
│   │   │   │   ├── Migrations/             # EF Core migrations
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── ReportRepository.cs
│   │   │   │   │   ├── MessageRepository.cs
│   │   │   │   │   ├── CaseRepository.cs
│   │   │   │   │   └── GenericRepository.cs
│   │   │   │   ├── Interceptors/
│   │   │   │   │   ├── AuditInterceptor.cs
│   │   │   │   │   └── SoftDeleteInterceptor.cs
│   │   │   │   ├── Seeds/                  # Database seeding
│   │   │   │   │   ├── EntitySeed.cs
│   │   │   │   │   ├── UserSeed.cs
│   │   │   │   │   └── RoleSeed.cs
│   │   │   │   └── UknfPlatform.Infrastructure.Persistence.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Infrastructure.Identity/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── JwtTokenService.cs
│   │   │   │   │   ├── PasswordHasher.cs
│   │   │   │   │   └── AuthenticationService.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   └── JwtSettings.cs
│   │   │   │   └── UknfPlatform.Infrastructure.Identity.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Infrastructure.FileStorage/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── MinioFileStorageService.cs
│   │   │   │   │   └── IFileStorageService.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   └── MinioSettings.cs
│   │   │   │   └── UknfPlatform.Infrastructure.FileStorage.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Infrastructure.Messaging/
│   │   │   │   ├── RabbitMQ/
│   │   │   │   │   ├── RabbitMqPublisher.cs
│   │   │   │   │   ├── RabbitMqConsumer.cs
│   │   │   │   │   └── RabbitMqSettings.cs
│   │   │   │   └── UknfPlatform.Infrastructure.Messaging.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Infrastructure.Caching/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── RedisCacheService.cs
│   │   │   │   │   └── ICacheService.cs
│   │   │   │   └── UknfPlatform.Infrastructure.Caching.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Infrastructure.Email/
│   │   │   │   ├── Services/
│   │   │   │   │   └── EmailService.cs
│   │   │   │   ├── Templates/              # Email templates
│   │   │   │   │   ├── WelcomeEmail.cshtml
│   │   │   │   │   ├── PasswordResetEmail.cshtml
│   │   │   │   │   └── AccessRequestApprovedEmail.cshtml
│   │   │   │   └── UknfPlatform.Infrastructure.Email.csproj
│   │   │   │
│   │   │   ├── UknfPlatform.Infrastructure.Logging/
│   │   │   │   ├── Services/
│   │   │   │   │   └── SerilogConfiguration.cs
│   │   │   │   └── UknfPlatform.Infrastructure.Logging.csproj
│   │   │   │
│   │   │   └── UknfPlatform.Infrastructure.ExternalServices/
│   │   │       ├── ReportValidator/
│   │   │       │   ├── MockReportValidatorService.cs
│   │   │       │   ├── IReportValidatorService.cs
│   │   │       │   └── ReportValidatorSettings.cs
│   │   │       └── UknfPlatform.Infrastructure.ExternalServices.csproj
│   │   │
│   │   ├── Workers/                        # Background workers
│   │   │   ├── UknfPlatform.Workers/
│   │   │   │   ├── ReportValidationWorker.cs
│   │   │   │   ├── VirusScanWorker.cs
│   │   │   │   ├── NotificationWorker.cs
│   │   │   │   ├── ReportTimeoutMonitor.cs  # Hangfire job
│   │   │   │   ├── Program.cs
│   │   │   │   └── UknfPlatform.Workers.csproj
│   │   │   │
│   │   │   └── UknfPlatform.Workers.Jobs/
│   │   │       ├── Jobs/                   # Hangfire recurring jobs
│   │   │       │   ├── DataVerificationReminderJob.cs
│   │   │       │   └── ExpiredAnnouncementCleanupJob.cs
│   │   │       └── UknfPlatform.Workers.Jobs.csproj
│   │   │
│   │   └── Tests/
│   │       ├── UknfPlatform.UnitTests/
│   │       │   ├── Application/
│   │       │   │   ├── Reports/
│   │       │   │   │   └── UploadReportCommandHandlerTests.cs
│   │       │   │   └── Auth/
│   │       │   ├── Domain/
│   │       │   └── Infrastructure/
│   │       │
│   │       ├── UknfPlatform.IntegrationTests/
│   │       │   ├── Controllers/
│   │       │   │   ├── ReportsControllerTests.cs
│   │       │   │   └── AuthControllerTests.cs
│   │       │   ├── Fixtures/
│   │       │   │   ├── WebApplicationFactory.cs
│   │       │   │   └── TestcontainersFixture.cs
│   │       │   └── TestData/
│   │       │       └── SampleReports/
│   │       │           ├── RIP100000_Q1_2025.xlsx  # Valid report
│   │       │           └── RIP100000_Q2_2025.xlsx  # Invalid report
│   │       │
│   │       └── UknfPlatform.E2ETests/
│   │           └── Scenarios/
│   │               ├── UserRegistrationE2ETests.cs
│   │               └── ReportSubmissionE2ETests.cs
│   │
│   └── Frontend/
│       ├── uknf-platform-ui/              # Angular application
│       │   ├── src/
│       │   │   ├── app/
│       │   │   │   ├── core/              # Singleton services, guards
│       │   │   │   │   ├── services/
│       │   │   │   │   │   ├── auth.service.ts
│       │   │   │   │   │   ├── api.service.ts
│       │   │   │   │   │   ├── signalr.service.ts
│       │   │   │   │   │   └── entity-context.service.ts
│       │   │   │   │   ├── guards/
│       │   │   │   │   │   ├── auth.guard.ts
│       │   │   │   │   │   └── role.guard.ts
│       │   │   │   │   ├── interceptors/
│       │   │   │   │   │   ├── auth.interceptor.ts
│       │   │   │   │   │   ├── error.interceptor.ts
│       │   │   │   │   │   └── entity-context.interceptor.ts
│       │   │   │   │   └── models/        # TypeScript interfaces/types
│       │   │   │   │       ├── user.model.ts
│       │   │   │   │       ├── report.model.ts
│       │   │   │   │       └── entity.model.ts
│       │   │   │   │
│       │   │   │   ├── shared/            # Shared components, directives, pipes
│       │   │   │   │   ├── components/
│       │   │   │   │   │   ├── data-table/
│       │   │   │   │   │   ├── file-upload/
│       │   │   │   │   │   └── entity-selector/
│       │   │   │   │   ├── directives/
│       │   │   │   │   └── pipes/
│       │   │   │   │
│       │   │   │   ├── features/          # Feature modules
│       │   │   │   │   ├── auth/
│       │   │   │   │   │   ├── login/
│       │   │   │   │   │   ├── register/
│       │   │   │   │   │   └── auth.module.ts
│       │   │   │   │   ├── reports/
│       │   │   │   │   │   ├── report-list/
│       │   │   │   │   │   ├── report-details/
│       │   │   │   │   │   ├── report-upload/
│       │   │   │   │   │   └── reports.module.ts
│       │   │   │   │   ├── messages/
│       │   │   │   │   ├── cases/
│       │   │   │   │   ├── library/
│       │   │   │   │   ├── faq/
│       │   │   │   │   ├── bulletin-board/
│       │   │   │   │   ├── dashboard/
│       │   │   │   │   └── admin/
│       │   │   │   │
│       │   │   │   ├── layouts/           # Layout components
│       │   │   │   │   ├── main-layout/
│       │   │   │   │   └── auth-layout/
│       │   │   │   │
│       │   │   │   ├── app.component.ts
│       │   │   │   ├── app.routes.ts
│       │   │   │   └── app.config.ts
│       │   │   │
│       │   │   ├── assets/
│       │   │   │   ├── images/
│       │   │   │   └── i18n/              # Internationalization files
│       │   │   │       ├── en.json
│       │   │   │       └── pl.json
│       │   │   │
│       │   │   ├── environments/
│       │   │   │   ├── environment.ts
│       │   │   │   └── environment.development.ts
│       │   │   │
│       │   │   ├── index.html
│       │   │   ├── main.ts
│       │   │   └── styles.scss            # Global styles
│       │   │
│       │   ├── angular.json
│       │   ├── package.json
│       │   ├── tsconfig.json
│       │   ├── tailwind.config.js
│       │   └── README.md
│       │
│       └── e2e/                           # Frontend E2E tests (if separate)
│           └── cypress/
│
├── infrastructure/                        # Infrastructure as Code
│   ├── docker/
│   │   ├── backend.Dockerfile
│   │   ├── frontend.Dockerfile
│   │   └── workers.Dockerfile
│   │
│   ├── docker-compose.yml                 # Main compose file (REQUIRED by PRD)
│   ├── docker-compose.dev.yml             # Development overrides
│   └── docker-compose.prod.yml            # Production configuration
│
├── scripts/
│   ├── seed-database.sh                   # Import test data
│   ├── run-migrations.sh                  # Run EF Core migrations
│   ├── generate-swagger.sh                # Generate OpenAPI spec
│   └── setup-dev-env.sh                   # One-command dev setup
│
├── test-data/                             # Test data provided by PRD
│   ├── entities.csv                       # Supervised entities to import
│   └── reports/
│       ├── RIP100000_Q1_2025.xlsx         # Valid report (PRD provided)
│       └── RIP100000_Q2_2025.xlsx         # Invalid report (PRD provided)
│
├── prompts.md                             # REQUIRED: Chronological prompts log
├── .gitignore
├── .dockerignore
├── README.md                              # Setup and run instructions
└── LICENSE

```

---

## **Key Directory Explanations**

### **Backend Structure (Modular Monolith)**

1. **`Api/`** - API Gateway Layer
   - Controllers: Thin controllers that delegate to MediatR
   - SignalR Hubs: WebSocket connections for real-time features
   - Middleware: Cross-cutting concerns (auth, logging, entity context)

2. **`Application/`** - Application Layer (CQRS)
   - Organized by module (Communication, Auth, Admin)
   - Commands: Write operations with validation
   - Queries: Read operations (optimized for performance)
   - Events: Domain events for cross-module communication
   - Validators: FluentValidation rules

3. **`Domain/`** - Domain Layer
   - Entities: Business entities with behavior
   - Enums: Strong typing for status values
   - Value Objects: Immutable domain concepts
   - Specifications: Complex business rules

4. **`Infrastructure/`** - Infrastructure Layer
   - Persistence: EF Core, repositories, migrations
   - Identity: JWT, authentication, authorization
   - FileStorage: MinIO/S3 integration
   - Messaging: RabbitMQ pub/sub
   - Caching: Redis integration
   - Email: MailKit SMTP
   - ExternalServices: Report validator, etc.

5. **`Workers/`** - Background Processing
   - Standalone console apps for queue consumers
   - Hangfire jobs for scheduled tasks

6. **`Tests/`** - All test projects
   - Unit tests: Domain and application logic
   - Integration tests: API endpoints with Testcontainers
   - E2E tests: Full workflow scenarios

### **Frontend Structure (Angular 20)**

- **`core/`**: Singleton services, guards, interceptors
- **`shared/`**: Reusable components, directives, pipes
- **`features/`**: Feature modules (one per PRD module)
- **`layouts/`**: Application shells
- Follows Angular style guide and best practices

### **Infrastructure**

- **`docker/`**: Dockerfiles for each deployable unit
- **`docker-compose.yml`**: **REQUIRED by PRD** - defines all services
- Includes: Backend API, Workers, Frontend, PostgreSQL, Redis, RabbitMQ, MinIO, MailDev

### **Hackathon Requirements**

- **`prompts.md`**: **REQUIRED by PRD** - chronological log of all AI prompts used
- **`test-data/`**: PRD-provided test files
- **`README.md`**: Quick setup instructions
- **`scripts/`**: One-command setup for judges

---

## **Solution Structure (.NET)**

```
UknfPlatform.sln
├── Api
│   └── UknfPlatform.Api
├── Application
│   ├── UknfPlatform.Application.Communication
│   ├── UknfPlatform.Application.Auth
│   ├── UknfPlatform.Application.Admin
│   └── UknfPlatform.Application.Shared
├── Domain
│   ├── UknfPlatform.Domain.Communication
│   ├── UknfPlatform.Domain.Auth
│   ├── UknfPlatform.Domain.Admin
│   └── UknfPlatform.Domain.Shared
├── Infrastructure
│   ├── UknfPlatform.Infrastructure.Persistence
│   ├── UknfPlatform.Infrastructure.Identity
│   ├── UknfPlatform.Infrastructure.FileStorage
│   ├── UknfPlatform.Infrastructure.Messaging
│   ├── UknfPlatform.Infrastructure.Caching
│   ├── UknfPlatform.Infrastructure.Email
│   ├── UknfPlatform.Infrastructure.Logging
│   └── UknfPlatform.Infrastructure.ExternalServices
├── Workers
│   ├── UknfPlatform.Workers
│   └── UknfPlatform.Workers.Jobs
└── Tests
    ├── UknfPlatform.UnitTests
    ├── UknfPlatform.IntegrationTests
    └── UknfPlatform.E2ETests
```

---

## **Rationale:**

1. **Clean Architecture**: Clear separation between API, Application, Domain, and Infrastructure layers

2. **Modular Organization**: Each module (Communication, Auth, Admin) is isolated but shares common infrastructure

3. **CQRS Structure**: Commands and Queries in separate folders make the pattern obvious and easy to follow

4. **Monorepo**: Frontend and backend in one repository for easier development and version management (hackathon-friendly)

5. **Testability**: Test projects mirror source structure; Testcontainers for realistic integration tests

6. **Docker-First**: All Dockerfiles and compose files at root level for easy deployment

7. **PRD Compliance**: 
   - `docker-compose.yml` at root (required)
   - `prompts.md` for AI development documentation (required)
   - Test data included (required)

8. **Developer Experience**: Scripts for common tasks, clear naming conventions, self-documenting structure

9. **Scalability**: Easy to extract modules into separate services later if needed

10. **Hackathon-Ready**: Judges can run `docker-compose up` and have everything working
