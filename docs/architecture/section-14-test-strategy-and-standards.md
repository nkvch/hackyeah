# **Section 14: Test Strategy and Standards**

Let me define a comprehensive testing approach that ensures quality and supports AI-driven development.

---

## **Testing Philosophy**

- **Approach:** Test-Driven Development (TDD) for domain logic; test-after for infrastructure code
- **Coverage Goals:** 
  - Domain Layer: 90%+ (business logic is critical)
  - Application Layer: 80%+
  - Infrastructure Layer: 60%+ (focus on critical paths)
  - Overall: 75%+ code coverage
- **Test Pyramid:** 
  - 70% Unit Tests (fast, isolated, many)
  - 20% Integration Tests (API + database, fewer)
  - 10% E2E Tests (full workflows, critical paths only)

---

## **Test Types and Organization**

### **Unit Tests**

- **Framework:** xUnit 2.6.6
- **File Convention:** `{ClassName}Tests.cs` (e.g., `SubmitReportCommandHandlerTests.cs`)
- **Location:** `Tests/UknfPlatform.UnitTests/{Layer}/{Module}/`
- **Mocking Library:** Moq 4.20.70
- **Coverage Requirement:** 90% for domain entities, 80% for command/query handlers

**AI Agent Requirements:**
- Generate tests for all public methods
- Cover happy path, error cases, and edge cases
- Follow AAA pattern (Arrange, Act, Assert)
- Mock all external dependencies (repositories, external services, event bus)
- Use FluentAssertions for readable assertions

**Example Unit Test:**

```csharp
public class SubmitReportCommandHandlerTests
{
    private readonly Mock<IReportRepository> _reportRepositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly SubmitReportCommandHandler _handler;

    public SubmitReportCommandHandlerTests()
    {
        _reportRepositoryMock = new Mock<IReportRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new SubmitReportCommandHandler(
            _reportRepositoryMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task Handle_WhenReportInWorkingStatus_SubmitsSuccessfully()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var report = new Report 
        { 
            Id = reportId, 
            Status = ValidationStatus.Working 
        };
        
        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        var command = new SubmitReportCommand { ReportId = reportId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        report.Status.Should().Be(ValidationStatus.Transmitted);
        result.Status.Should().Be(ValidationStatus.Transmitted);
        
        _reportRepositoryMock.Verify(r => r.UpdateAsync(report), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(
            It.Is<ReportSubmittedEvent>(evt => evt.ReportId == reportId)), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReportNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report)null);

        var command = new SubmitReportCommand { ReportId = reportId };

        // Act & Assert
        await _handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{nameof(Report)}*{reportId}*");
    }

    [Fact]
    public async Task Handle_WhenReportAlreadySubmitted_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var report = new Report 
        { 
            Id = Guid.NewGuid(), 
            Status = ValidationStatus.Transmitted 
        };
        
        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(report.Id))
            .ReturnsAsync(report);

        var command = new SubmitReportCommand { ReportId = report.Id };

        // Act & Assert
        await _handler
            .Invoking(h => h.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<BusinessRuleViolationException>();
    }
}
```

---

### **Integration Tests**

- **Scope:** API endpoints + database + infrastructure services
- **Location:** `Tests/UknfPlatform.IntegrationTests/`
- **Test Infrastructure:**
  - **PostgreSQL:** Testcontainers (real database in Docker)
  - **Redis:** Testcontainers
  - **RabbitMQ:** Testcontainers
  - **MinIO:** Testcontainers or in-memory mock
  - **External APIs:** WireMock for stubbing

**WebApplicationFactory Setup:**

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real database
            var descriptor = services.SingleOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database (Testcontainers)
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(TestDbConnectionString);
            });

            // Override external services with mocks
            services.AddSingleton<IReportValidatorService, MockReportValidatorService>();
        });

        builder.UseEnvironment("Test");
    }
}
```

**Testcontainers Setup:**

```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    protected HttpClient Client;
    protected CustomWebApplicationFactory Factory;

    public IntegrationTestBase()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16.3-alpine")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7.2-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection", 
            _postgresContainer.GetConnectionString());
        Environment.SetEnvironmentVariable(
            "Redis__ConnectionString", 
            _redisContainer.GetConnectionString());

        Factory = new CustomWebApplicationFactory();
        Client = Factory.CreateClient();

        // Run migrations
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        Factory?.Dispose();
    }
}
```

**Example Integration Test:**

```csharp
public class ReportsControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task UploadReport_WithValidFile_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsExternalUser();
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(TestData.GetValidReportFile());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "report.xlsx");
        content.Add(new StringContent("1001"), "entityId");
        content.Add(new StringContent("Q1_2025"), "reportingPeriod");

        // Act
        var response = await Client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ReportDto>();
        result.Should().NotBeNull();
        result.FileName.Should().Be("report.xlsx");
        result.ValidationStatus.Should().Be(ValidationStatus.Working);
    }

    [Fact]
    public async Task UploadReport_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsExternalUser();
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        content.Add(fileContent, "file", "document.pdf");

        // Act
        var response = await Client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Errors.Should().ContainKey("File");
    }

    private async Task AuthenticateAsExternalUser()
    {
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@entity.com",
            Password = "Test@123"
        });

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
    }
}
```

---

### **End-to-End Tests**

- **Framework:** xUnit 2.6.6 + Playwright (for browser automation if needed)
- **Scope:** Complete user workflows across frontend and backend
- **Environment:** Full Docker Compose stack running
- **Test Data:** Seeded database with known test entities

**Example E2E Test:**

```csharp
public class ReportSubmissionE2ETests : E2ETestBase
{
    [Fact]
    public async Task ExternalUser_CanSubmitReportAndReceiveValidation()
    {
        // Arrange - User registers and gets approved
        var userId = await RegisterAndApproveExternalUser("jan.kowalski@entity.com");
        await LoginUser("jan.kowalski@entity.com", "Test@123");
        await SelectEntityContext(1001);

        // Act - Upload report
        var reportId = await UploadReport("RIP100000_Q1_2025.xlsx");
        
        // Act - Submit for validation
        await SubmitReport(reportId);
        
        // Assert - Report enters validation queue
        var report = await GetReport(reportId);
        report.ValidationStatus.Should().Be(ValidationStatus.Transmitted);

        // Act - Wait for validation to complete (or simulate)
        await WaitForValidationCompletion(reportId, timeoutSeconds: 30);

        // Assert - Validation result available
        report = await GetReport(reportId);
        report.ValidationStatus.Should().Be(ValidationStatus.Successful);
        report.ValidationResultFileKey.Should().NotBeNullOrEmpty();

        // Assert - User received notification
        var notifications = await GetUserNotifications(userId);
        notifications.Should().Contain(n => 
            n.Type == "ReportValidated" && 
            n.ReportId == reportId);
    }
}
```

---

## **Test Data Management**

- **Strategy:** Factory pattern + builders for test data
- **Fixtures:** Reusable test data in `Tests/TestData/` directory
- **Factories:** Create domain entities with sensible defaults

**Test Data Factory:**

```csharp
public static class ReportFactory
{
    public static Report CreateWorking(
        Guid? id = null,
        long entityId = 1001,
        Guid? userId = null)
    {
        return new Report
        {
            Id = id ?? Guid.NewGuid(),
            EntityId = entityId,
            UserId = userId ?? Guid.NewGuid(),
            FileName = "test-report.xlsx",
            FileStorageKey = "reports/test-report-key",
            FileSize = 1024 * 100,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            ValidationStatus = ValidationStatus.Working,
            SubmittedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }

    public static Report CreateValidated(ValidationStatus status = ValidationStatus.Successful)
    {
        var report = CreateWorking();
        report.Submit();
        report.CompleteValidation(status, "validation-result-key");
        return report;
    }
}

public static class UserFactory
{
    public static User CreateExternalUser(string email = "test@entity.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Phone = "+48123456789",
            PeselEncrypted = "encrypted_pesel",
            PeselLast4 = "1234",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            UserType = UserType.External,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }
}
```

**Test File Fixtures:**

```
Tests/TestData/
├── Reports/
│   ├── RIP100000_Q1_2025.xlsx     # Valid report (PRD provided)
│   ├── RIP100000_Q2_2025.xlsx     # Invalid report (PRD provided)
│   └── LargeReport.xlsx           # 100MB file for size testing
├── Entities/
│   └── test-entities.csv          # Sample entities for seeding
└── Users/
    └── test-users.json            # Test user accounts
```

**Cleanup Strategy:**

```csharp
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestConnectionString)
            .Options;
        
        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        // Clean up test data after each test
        Context.Reports.RemoveRange(Context.Reports);
        Context.Messages.RemoveRange(Context.Messages);
        Context.Cases.RemoveRange(Context.Cases);
        Context.SaveChanges();
        Context.Dispose();
    }
}
```

---

## **Continuous Testing**

- **CI Integration:** GitHub Actions runs tests on every push/PR
- **Performance Tests:** Benchmark critical endpoints (report upload, large queries)
- **Security Tests:** OWASP ZAP scan for vulnerabilities (optional for hackathon)

**CI Test Pipeline:**

```yaml
# .github/workflows/test.yml
name: Test Suite

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      
      - name: Run unit tests
        run: |
          dotnet test src/Backend/Tests/UknfPlatform.UnitTests \
            --configuration Release \
            --logger "trx;LogFileName=unit-tests.trx" \
            /p:CollectCoverage=true \
            /p:CoverletOutputFormat=opencover

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.opencover.xml

  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      
      - name: Run integration tests
        run: |
          dotnet test src/Backend/Tests/UknfPlatform.IntegrationTests \
            --configuration Release \
            --logger "trx;LogFileName=integration-tests.trx"

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 20
      
      - name: Install dependencies
        run: npm ci
        working-directory: src/Frontend/uknf-platform-ui
      
      - name: Run tests
        run: npm test -- --code-coverage
        working-directory: src/Frontend/uknf-platform-ui
```

---

## **Frontend Testing (Angular)**

**Unit Tests (Jasmine + Karma):**

```typescript
describe('ReportListComponent', () => {
  let component: ReportListComponent;
  let fixture: ComponentFixture<ReportListComponent>;
  let reportService: jasmine.SpyObj<ReportService>;

  beforeEach(async () => {
    const reportServiceSpy = jasmine.createSpyObj('ReportService', ['getReports']);

    await TestBed.configureTestingModule({
      imports: [ReportListComponent],
      providers: [
        { provide: ReportService, useValue: reportServiceSpy }
      ]
    }).compileComponents();

    reportService = TestBed.inject(ReportService) as jasmine.SpyObj<ReportService>;
    fixture = TestBed.createComponent(ReportListComponent);
    component = fixture.componentInstance;
  });

  it('should load reports on init', () => {
    const mockReports = [
      { id: '123', fileName: 'report1.xlsx', status: 'Working' }
    ];
    reportService.getReports.and.returnValue(of(mockReports));

    fixture.detectChanges();

    expect(reportService.getReports).toHaveBeenCalled();
    expect(component.reports()).toEqual(mockReports);
  });
});
```

**E2E Tests (Cypress - optional):**

```typescript
describe('Report Submission Flow', () => {
  beforeEach(() => {
    cy.login('test@entity.com', 'Test@123');
    cy.selectEntity('Test Entity');
  });

  it('allows user to upload and submit report', () => {
    cy.visit('/reports/upload');
    
    cy.get('[data-test="file-input"]')
      .attachFile('RIP100000_Q1_2025.xlsx');
    
    cy.get('[data-test="reporting-period"]')
      .type('Q1_2025');
    
    cy.get('[data-test="submit-button"]').click();
    
    cy.get('[data-test="success-message"]')
      .should('contain', 'Report uploaded successfully');
    
    cy.url().should('include', '/reports');
    cy.get('[data-test="report-status"]')
      .should('contain', 'Transmitted');
  });
});
```

---

## **Test Execution Commands**

```bash
# Backend - Run all tests
dotnet test src/Backend/UknfPlatform.sln

# Backend - Run specific test project
dotnet test src/Backend/Tests/UknfPlatform.UnitTests

# Backend - Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Frontend - Run tests
cd src/Frontend/uknf-platform-ui && npm test

# Frontend - Run tests with coverage
npm test -- --code-coverage

# Frontend - Run E2E tests
npm run e2e

# Docker - Run integration tests
docker-compose -f docker-compose.test.yml up --abort-on-container-exit
```

---

## **Rationale:**

1. **Test Pyramid**: Balanced distribution ensures fast feedback (many unit tests) with confidence (some integration/E2E)

2. **Testcontainers**: Real database/services in tests = high confidence, catches integration issues early

3. **Factory Pattern**: Consistent test data creation, reduces boilerplate, improves maintainability

4. **AI-Friendly**: Clear patterns and conventions make it easy for AI to generate tests

5. **CI Integration**: Automated testing on every commit ensures quality gates

6. **Coverage Goals**: Realistic targets (not 100%) focused on critical business logic

7. **Hackathon-Appropriate**: Comprehensive strategy but pragmatic (not over-testing)

8. **Real Test Files**: Using PRD-provided test files (valid/invalid reports) ensures realistic testing
