# **Section 13: Coding Standards**

**⚠️ CRITICAL: These standards directly control AI agent behavior during development.**

These are **MANDATORY** rules. Focus is on project-specific conventions and critical rules to prevent bad code. Assume AI knows general best practices - only document what's unique or critical to this project.

---

## **Core Standards**

- **Languages & Runtimes:** 
  - Backend: C# 12.0 with .NET 8.0 LTS
  - Frontend: TypeScript 5.3+ with Angular 20.x
  
- **Style & Linting:**
  - Backend: EditorConfig + StyleCop analyzers (enforce on build)
  - Frontend: ESLint + Prettier (auto-format on save)
  - **Rule:** Code MUST pass linter checks before commit
  
- **Test Organization:**
  - Test file naming: `{ClassName}Tests.cs` (e.g., `ReportServiceTests.cs`)
  - Test location: Mirror source structure in Tests/ directory
  - Test method naming: `MethodName_Scenario_ExpectedBehavior` (e.g., `SubmitReport_WhenAlreadySubmitted_ThrowsBusinessRuleViolation`)

---

## **Naming Conventions**

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `ReportService`, `UserRepository` |
| Interfaces | IPascalCase | `IReportService`, `IUserRepository` |
| Methods | PascalCase | `GetReportById`, `SubmitReport` |
| Private fields | _camelCase | `_reportRepository`, `_logger` |
| Parameters | camelCase | `reportId`, `userId` |
| Constants | UPPER_SNAKE_CASE | `MAX_FILE_SIZE`, `DEFAULT_PAGE_SIZE` |
| Enums | PascalCase (singular) | `ValidationStatus`, `UserType` |
| Enum values | PascalCase | `ValidationStatus.Successful` |
| Database tables | PascalCase (plural) | `Reports`, `Users`, `AccessRequests` |
| Database columns | PascalCase | `EntityId`, `CreatedDate` |
| API routes | kebab-case | `/api/access-requests`, `/api/bulletin-board` |
| Angular components | kebab-case | `report-list.component.ts` |
| Angular services | PascalCase | `AuthService`, `ReportService` |

---

## **Critical Rules**

**⚠️ MANDATORY - AI agents MUST follow these rules:**

- **Never use `Console.WriteLine` in production code** - Use `ILogger<T>` dependency injection. Console output is not captured by logging infrastructure.
  ```csharp
  // ❌ WRONG
  Console.WriteLine($"Processing report {reportId}");
  
  // ✅ CORRECT
  _logger.LogInformation("Processing report {ReportId}", reportId);
  ```

- **All API responses MUST use consistent DTO types** - Never return domain entities directly from controllers. Always map to DTOs using Mapster.
  ```csharp
  // ❌ WRONG
  [HttpGet("{id}")]
  public async Task<Report> GetReport(Guid id) 
  {
      return await _reportRepository.GetByIdAsync(id);
  }
  
  // ✅ CORRECT
  [HttpGet("{id}")]
  public async Task<ReportDto> GetReport(Guid id) 
  {
      var report = await _reportRepository.GetByIdAsync(id);
      return _mapper.Map<ReportDto>(report);
  }
  ```

- **Database queries MUST use repositories, never direct DbContext** - Maintain abstraction layer for testability and consistency.
  ```csharp
  // ❌ WRONG (in Command Handler)
  var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == reportId);
  
  // ✅ CORRECT
  var report = await _reportRepository.GetByIdAsync(reportId);
  ```

- **All commands/queries MUST have validators** - Use FluentValidation for every MediatR command and query. No bare handlers.
  ```csharp
  // ✅ REQUIRED for every command
  public class SubmitReportCommandValidator : AbstractValidator<SubmitReportCommand>
  {
      public SubmitReportCommandValidator()
      {
          RuleFor(x => x.ReportId).NotEmpty();
      }
  }
  ```

- **Never hardcode entity context - ALWAYS use EntityContextMiddleware** - Entity context must come from middleware, never from query parameters or user input.
  ```csharp
  // ❌ WRONG
  var reports = await _context.Reports.Where(r => r.EntityId == requestedEntityId).ToListAsync();
  
  // ✅ CORRECT - Entity filter applied automatically via EF Core global query filter
  var reports = await _context.Reports.ToListAsync(); // Already filtered by current entity context
  ```

- **All async methods MUST end with 'Async' suffix** - Enforce async naming convention for clarity.
  ```csharp
  // ❌ WRONG
  public async Task<Report> GetReport(Guid id)
  
  // ✅ CORRECT
  public async Task<Report> GetReportAsync(Guid id)
  ```

- **Entity properties MUST be private set or init** - Enforce encapsulation; use methods for state changes.
  ```csharp
  // ❌ WRONG
  public class Report 
  {
      public ValidationStatus Status { get; set; } // Public setter
  }
  
  // ✅ CORRECT
  public class Report 
  {
      public ValidationStatus Status { get; private set; }
      
      public void Submit() 
      {
          if (Status != ValidationStatus.Working)
              throw new BusinessRuleViolationException("Report already submitted");
          
          Status = ValidationStatus.Transmitted;
          SubmittedDate = DateTime.UtcNow;
      }
  }
  ```

- **NEVER store sensitive data in logs** - No PESEL, passwords, tokens, or full email addresses. See Error Handling section for complete list.
  ```csharp
  // ❌ WRONG
  _logger.LogInformation("User {Email} logged in with password {Password}", email, password);
  
  // ✅ CORRECT
  _logger.LogInformation("User {UserId} logged in successfully", userId);
  ```

- **All dates MUST be stored in UTC** - Use `DateTime.UtcNow`, never `DateTime.Now`.
  ```csharp
  // ❌ WRONG
  CreatedDate = DateTime.Now;
  
  // ✅ CORRECT
  CreatedDate = DateTime.UtcNow;
  ```

- **File uploads MUST validate type and size before processing** - Security requirement.
  ```csharp
  // ✅ REQUIRED for all file uploads
  private const long MAX_FILE_SIZE = 100 * 1024 * 1024; // 100 MB
  private static readonly string[] ALLOWED_EXTENSIONS = { ".pdf", ".xlsx", ".docx", ".zip" };
  
  if (file.Length > MAX_FILE_SIZE)
      throw new ValidationException("File size exceeds 100 MB limit");
      
  var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
  if (!ALLOWED_EXTENSIONS.Contains(extension))
      throw new ValidationException($"File type {extension} not allowed");
  ```

- **SignalR hub methods MUST check authorization** - Never trust client-provided entity context.
  ```csharp
  // ✅ REQUIRED for all hub methods
  [Authorize]
  public class NotificationHub : Hub
  {
      public async Task JoinEntityGroup(long entityId)
      {
          // Verify user has access to this entity
          if (!await _authService.UserHasAccessToEntity(Context.User, entityId))
              throw new HubException("Access denied to entity");
          
          await Groups.AddToGroupAsync(Context.ConnectionId, $"entity-{entityId}");
      }
  }
  ```

- **Background workers MUST implement graceful shutdown** - Handle CancellationToken properly.
  ```csharp
  // ✅ REQUIRED for all background workers
  public class ReportValidationWorker : BackgroundService
  {
      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
          while (!stoppingToken.IsCancellationRequested)
          {
              try
              {
                  await ProcessQueue(stoppingToken);
              }
              catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
              {
                  _logger.LogInformation("Worker shutting down gracefully");
                  break;
              }
          }
      }
  }
  ```

- **Angular services MUST handle errors with catchError** - Never let errors propagate unhandled.
  ```typescript
  // ❌ WRONG
  getReports(): Observable<Report[]> {
    return this.http.get<Report[]>('/api/reports');
  }
  
  // ✅ CORRECT
  getReports(): Observable<Report[]> {
    return this.http.get<Report[]>('/api/reports').pipe(
      catchError(error => {
        this.notificationService.showError('Failed to load reports');
        return throwError(() => error);
      })
    );
  }
  ```

- **Angular components MUST unsubscribe from observables** - Use `takeUntilDestroyed()` or async pipe.
  ```typescript
  // ❌ WRONG
  ngOnInit() {
    this.reportService.getReports().subscribe(reports => {
      this.reports = reports;
    });
  }
  
  // ✅ CORRECT (Option 1: async pipe - preferred)
  reports$ = this.reportService.getReports();
  // In template: <div *ngFor="let report of reports$ | async">
  
  // ✅ CORRECT (Option 2: takeUntilDestroyed)
  private destroyRef = inject(DestroyRef);
  
  ngOnInit() {
    this.reportService.getReports()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(reports => this.reports = reports);
  }
  ```

- **All configuration MUST use strongly-typed settings classes** - No magic strings in appsettings.json lookups.
  ```csharp
  // ❌ WRONG
  var maxFileSize = _configuration["FileUpload:MaxSize"];
  
  // ✅ CORRECT
  public class FileUploadSettings 
  {
      public long MaxSize { get; set; }
      public string[] AllowedExtensions { get; set; }
  }
  
  // In Program.cs
  builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUpload"));
  
  // In service
  public MyService(IOptions<FileUploadSettings> settings) 
  {
      _settings = settings.Value;
  }
  ```

---

## **C# Specifics**

- **Use `record` types for DTOs** - Immutable, value-based equality, concise syntax.
  ```csharp
  // ✅ Preferred for DTOs
  public record ReportDto(
      Guid Id,
      string FileName,
      ValidationStatus Status,
      DateTime SubmittedDate);
  ```

- **Use nullable reference types** - Project has `<Nullable>enable</Nullable>`, respect nullability annotations.
  ```csharp
  // ✅ CORRECT
  public class UserDto 
  {
      public Guid Id { get; init; }
      public string Email { get; init; } = string.Empty; // Never null
      public string? PhoneNumber { get; init; } // Can be null
  }
  ```

- **Prefer `string.IsNullOrWhiteSpace` over `string.IsNullOrEmpty`** - More thorough validation.

- **Use `nameof()` for property names** - Refactor-safe, compile-time checked.
  ```csharp
  // ✅ CORRECT
  throw new NotFoundException(nameof(Report), reportId);
  ```

- **Use pattern matching where appropriate** - Modern C# syntax.
  ```csharp
  // ✅ CORRECT
  var result = report switch
  {
      { Status: ValidationStatus.Successful } => "Valid",
      { Status: ValidationStatus.ValidationErrors, IsArchived: false } => "Needs correction",
      _ => "Unknown"
  };
  ```

---

## **TypeScript/Angular Specifics**

- **Use strict TypeScript** - `strict: true` in tsconfig.json, no implicit any.

- **Prefer signals over RxJS where appropriate** - Angular 20 feature for simpler reactivity.
  ```typescript
  // ✅ Preferred for simple state
  reports = signal<Report[]>([]);
  selectedReport = signal<Report | null>(null);
  ```

- **Use standalone components** - No NgModules, standalone APIs only.
  ```typescript
  @Component({
    selector: 'app-report-list',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule],
    templateUrl: './report-list.component.html'
  })
  ```

- **Use PrimeNG components consistently** - Don't mix with other component libraries.

- **Use Tailwind utility classes** - Avoid custom CSS where possible.
  ```html
  <!-- ✅ CORRECT -->
  <div class="flex items-center justify-between p-4 bg-white rounded-lg shadow">
  ```

- **File organization:** One component/service/model per file, named after the class.

---

## **Database/EF Core Specifics**

- **Never use `SaveChanges()` directly in application code** - Use Unit of Work pattern via repository.
  ```csharp
  // ❌ WRONG (in command handler)
  _context.Reports.Add(report);
  await _context.SaveChangesAsync();
  
  // ✅ CORRECT
  await _reportRepository.AddAsync(report);
  await _unitOfWork.SaveChangesAsync();
  ```

- **Always use `.AsNoTracking()` for read-only queries** - Performance optimization.
  ```csharp
  // ✅ CORRECT for queries
  public async Task<IEnumerable<Report>> GetReportsAsync()
  {
      return await _context.Reports
          .AsNoTracking()
          .ToListAsync();
  }
  ```

- **Use explicit loading for related entities** - Avoid lazy loading and N+1 queries.
  ```csharp
  // ✅ CORRECT
  var report = await _context.Reports
      .Include(r => r.Entity)
      .Include(r => r.Messages)
      .FirstOrDefaultAsync(r => r.Id == reportId);
  ```

- **Migration naming:** `{YYYYMMDDHHMMSS}_{DescriptiveName}` (auto-generated by EF).

---

## **Testing Standards**

- **Test coverage target:** Minimum 70% for application layer, 90% for domain layer.

- **Use AAA pattern** - Arrange, Act, Assert with clear sections.
  ```csharp
  [Fact]
  public async Task SubmitReport_WhenStatusIsWorking_UpdatesStatusToTransmitted()
  {
      // Arrange
      var report = ReportFactory.CreateWorking();
      var handler = new SubmitReportCommandHandler(_reportRepository, _eventBus);
      
      // Act
      await handler.Handle(new SubmitReportCommand { ReportId = report.Id }, CancellationToken.None);
      
      // Assert
      report.Status.Should().Be(ValidationStatus.Transmitted);
  }
  ```

- **Use FluentAssertions** - More readable assertions.
  ```csharp
  // ✅ CORRECT
  result.Should().NotBeNull();
  result.Status.Should().Be(ValidationStatus.Successful);
  errors.Should().BeEmpty();
  ```

- **Mock external dependencies only** - Don't mock repositories in integration tests; use Testcontainers.

- **Test file structure:** Mirror source structure exactly.

---

## **Git Commit Standards**

- **Commit message format:** `[type]: Brief description`
  - Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`
  - Example: `feat: Add report submission with validation`
  - Example: `fix: Correct entity context filtering in report queries`

- **Branch naming:** `feature/description`, `bugfix/description`, `hotfix/description`

---

## **Documentation Requirements**

- **XML documentation comments required for:**
  - All public APIs (controllers, services)
  - Complex business logic methods
  - Configurations and settings classes
  
  ```csharp
  /// <summary>
  /// Submits a report for validation. Report must be in Working status.
  /// </summary>
  /// <param name="reportId">The unique identifier of the report</param>
  /// <returns>Updated report information</returns>
  /// <exception cref="NotFoundException">Report not found</exception>
  /// <exception cref="BusinessRuleViolationException">Report not in Working status</exception>
  public async Task<ReportDto> SubmitReportAsync(Guid reportId)
  ```

- **JSDoc comments for complex Angular logic** - Public service methods, complex algorithms.

- **README files required in:** Each major directory explaining its purpose and structure.

---

## **Performance Guidelines**

- **Pagination required for all list endpoints** - Default page size: 20, max: 100.

- **Use caching for:**
  - User permissions (Redis, 5 minutes TTL)
  - Entity context (Redis, session duration)
  - Library file metadata (Redis, 1 hour TTL)
  
- **Lazy load Angular modules** - Route-level code splitting.

- **Debounce search inputs** - 300ms delay for search/filter operations.

---

## **Security Checklist**

- ✅ All endpoints require authentication (except `/auth/login`, `/auth/register`)
- ✅ Authorization checks use policy-based authorization, not role strings
- ✅ SQL injection prevention: Always use parameterized queries (EF Core handles this)
- ✅ XSS prevention: Angular sanitizes by default; never use `innerHTML` with user content
- ✅ CSRF protection: Enabled by default in Angular HttpClient
- ✅ File upload validation: Type and size checked before processing
- ✅ CORS: Configured in backend, only allow known origins

---

## **Rationale:**

1. **Project-Specific Focus**: Rules are tailored to this architecture (CQRS, EF Core, multi-tenancy)

2. **AI-Friendly**: Clear, enforceable rules that AI agents can follow consistently

3. **Security-First**: Critical security rules highlighted (logging, file uploads, authorization)

4. **Maintainability**: Conventions ensure consistent codebase across multiple AI-generated features

5. **Best Practices**: Industry-standard patterns (repository, DTO, async/await, nullable types)

6. **Performance**: Built-in performance considerations (caching, pagination, no-tracking queries)

7. **Hackathon-Appropriate**: Comprehensive but not overwhelming; focuses on preventing common mistakes

8. **Testability**: Standards support automated testing (DI, repository pattern, validators)

---

**These standards will be extracted to a separate `docs/architecture/coding-standards.md` file and loaded by the dev agent for all code generation tasks.**
