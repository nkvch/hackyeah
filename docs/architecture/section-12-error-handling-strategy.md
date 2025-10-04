# **Section 12: Error Handling Strategy**

Let me define a comprehensive error handling approach for the platform that ensures consistent, secure, and debuggable error management across all layers.

---

## **General Approach**

- **Error Model:** Exception-based error handling with global exception middleware
- **Exception Hierarchy:** Custom exception types that map to HTTP status codes
- **Error Propagation:** Exceptions bubble up to middleware layer which formats consistent API responses

---

## **Exception Hierarchy**

```csharp
// Base application exception
public abstract class ApplicationException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public Dictionary<string, string[]> ValidationErrors { get; }

    protected ApplicationException(
        string message, 
        string errorCode, 
        int statusCode,
        Exception innerException = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        ValidationErrors = new Dictionary<string, string[]>();
    }
}

// Domain exceptions
public class ValidationException : ApplicationException
{
    public ValidationException(string message, Dictionary<string, string[]> errors)
        : base(message, "VALIDATION_ERROR", 400)
    {
        ValidationErrors = errors;
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with id {entityId} not found", "NOT_FOUND", 404)
    {
    }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, "UNAUTHORIZED", 401)
    {
    }
}

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message, "FORBIDDEN", 403)
    {
    }
}

public class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base(message, "CONFLICT", 409)
    {
    }
}

public class BusinessRuleViolationException : ApplicationException
{
    public BusinessRuleViolationException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION", 422)
    {
    }
}

// Infrastructure exceptions
public class ExternalServiceException : ApplicationException
{
    public ExternalServiceException(string serviceName, string message, Exception innerException = null)
        : base($"External service {serviceName} failed: {message}", "EXTERNAL_SERVICE_ERROR", 502, innerException)
    {
    }
}

public class FileStorageException : ApplicationException
{
    public FileStorageException(string message, Exception innerException = null)
        : base(message, "FILE_STORAGE_ERROR", 500, innerException)
    {
    }
}
```

---

## **Global Exception Middleware**

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var entityContext = context.Items["EntityId"]?.ToString();
        var userId = context.User?.FindFirst("sub")?.Value;

        // Log exception with context
        _logger.LogError(exception,
            "Unhandled exception. CorrelationId: {CorrelationId}, UserId: {UserId}, EntityId: {EntityId}, Path: {Path}",
            correlationId, userId, entityContext, context.Request.Path);

        // Prepare response
        var (statusCode, errorResponse) = exception switch
        {
            ApplicationException appEx => (appEx.StatusCode, new ErrorResponse
            {
                Message = appEx.Message,
                ErrorCode = appEx.ErrorCode,
                Errors = appEx.ValidationErrors.Any() ? appEx.ValidationErrors : null,
                CorrelationId = correlationId
            }),
            
            FluentValidation.ValidationException validationEx => (400, new ErrorResponse
            {
                Message = "Validation failed",
                ErrorCode = "VALIDATION_ERROR",
                Errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                CorrelationId = correlationId
            }),
            
            _ => (500, new ErrorResponse
            {
                Message = "An internal server error occurred",
                ErrorCode = "INTERNAL_ERROR",
                CorrelationId = correlationId
            })
        };

        // Don't expose internal error details in production
        if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction() 
            && statusCode == 500)
        {
            errorResponse.Message = "An error occurred while processing your request";
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
}

public class ErrorResponse
{
    public string Message { get; set; }
    public string ErrorCode { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
    public string CorrelationId { get; set; }
}
```

---

## **Logging Standards**

- **Library:** Serilog 3.1.1
- **Format:** JSON structured logging
- **Levels:** 
  - **Trace**: Detailed diagnostic information (disabled in production)
  - **Debug**: Internal system events (disabled in production)
  - **Information**: General application flow (startup, requests, significant events)
  - **Warning**: Abnormal but expected events (validation failures, missing data)
  - **Error**: Errors and exceptions that require attention
  - **Critical**: Critical failures requiring immediate action

- **Required Context:**
  - **Correlation ID:** Unique ID per request (format: `GUID`)
  - **Service Context:** Service name, version, environment
  - **User Context:** User ID (never log PII like email/name), entity ID, user type

**Serilog Configuration:**

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "UknfPlatform")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.File(
        formatter: new JsonFormatter(),
        path: "logs/uknf-platform-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .WriteTo.Conditional(
        condition: _ => builder.Environment.IsProduction(),
        configureSink: sink => sink.Seq("http://seq:5341")) // Optional: Seq for production
    .CreateLogger();
```

**Log Entry Example:**

```json
{
  "@t": "2025-10-04T12:34:56.7890123Z",
  "@l": "Error",
  "@m": "Report validation failed",
  "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "UserId": "c8a5d9e2-1234-5678-9abc-def012345678",
  "EntityId": 1001,
  "ReportId": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "ValidationStatus": "ValidationErrors",
  "ErrorCount": 3,
  "Application": "UknfPlatform",
  "Environment": "Development",
  "MachineName": "uknf-backend-api"
}
```

---

## **Error Handling Patterns**

### **External API Errors (Report Validator, SMTP, etc.)**

- **Retry Policy:** Exponential backoff (1s, 2s, 4s, 8s, 16s max)
- **Circuit Breaker:** Open after 5 consecutive failures, half-open after 30s
- **Timeout Configuration:** 
  - Report validation: 30 seconds per HTTP request (24-hour overall timeout tracked separately)
  - SMTP: 10 seconds
  - File storage: 60 seconds for large files
- **Error Translation:** Map external errors to internal exception types

**Using Polly for Resilience:**

```csharp
// HTTP client with retry and circuit breaker
services.AddHttpClient<IReportValidatorService, MockReportValidatorService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetTimeoutPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning("Retry {RetryCount} after {Delay}s due to {Error}",
                    retryCount, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                Log.Error("Circuit breaker opened for {Duration}s due to {Error}",
                    duration.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
            },
            onReset: () => Log.Information("Circuit breaker reset"));
}

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
}
```

### **Business Logic Errors**

- **Custom Exceptions:** Throw domain-specific exceptions (e.g., `ReportAlreadySubmittedException`, `CaseAlreadyClosedException`)
- **User-Facing Errors:** Clear, actionable error messages (localized if needed)
- **Error Codes:** Consistent error code system for frontend handling

**Example:**

```csharp
public class SubmitReportCommandHandler : IRequestHandler<SubmitReportCommand, ReportDto>
{
    public async Task<ReportDto> Handle(SubmitReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.ReportId);
        
        if (report == null)
            throw new NotFoundException(nameof(Report), request.ReportId);
        
        if (report.ValidationStatus != ValidationStatus.Working)
            throw new BusinessRuleViolationException(
                $"Report cannot be submitted. Current status: {report.ValidationStatus}. Only reports with 'Working' status can be submitted.");
        
        // Update report status
        report.Submit();
        await _reportRepository.UpdateAsync(report);
        
        // Publish event for async validation
        await _eventBus.PublishAsync(new ReportSubmittedEvent(report.Id));
        
        return _mapper.Map<ReportDto>(report);
    }
}
```

### **Data Consistency**

- **Transaction Strategy:** Use EF Core transactions for multi-entity operations
- **Compensation Logic:** Saga pattern for long-running workflows (report validation)
- **Idempotency:** Idempotent API endpoints using idempotency keys

**Transaction Example:**

```csharp
public async Task<Result> ApproveAccessRequestAsync(Guid accessRequestId, Guid reviewerId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Update access request
        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.PermissionLines)
            .FirstOrDefaultAsync(ar => ar.Id == accessRequestId);
        
        if (accessRequest == null)
            throw new NotFoundException(nameof(AccessRequest), accessRequestId);
        
        accessRequest.Approve(reviewerId);
        
        // Grant permissions to user
        foreach (var permissionLine in accessRequest.PermissionLines)
        {
            await _userEntityRepository.AddUserEntityMappingAsync(
                accessRequest.UserId, 
                permissionLine.EntityId,
                permissionLine);
        }
        
        // Save changes
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // Publish event (outside transaction)
        await _eventBus.PublishAsync(new AccessRequestApprovedEvent(accessRequestId));
        
        return Result.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Failed to approve access request {AccessRequestId}", accessRequestId);
        throw;
    }
}
```

**Idempotency for Report Submission:**

```csharp
[HttpPost("{id}/submit")]
[ProducesResponseType(typeof(ReportDto), 200)]
[ProducesResponseType(typeof(ErrorResponse), 400)]
public async Task<IActionResult> SubmitReport(
    Guid id,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey = null)
{
    // Check if this submission was already processed
    if (!string.IsNullOrEmpty(idempotencyKey))
    {
        var cachedResult = await _cache.GetAsync<ReportDto>($"submit-report:{idempotencyKey}");
        if (cachedResult != null)
        {
            _logger.LogInformation("Idempotent request detected for key {IdempotencyKey}", idempotencyKey);
            return Ok(cachedResult);
        }
    }
    
    var command = new SubmitReportCommand { ReportId = id };
    var result = await _mediator.Send(command);
    
    // Cache result for idempotency (24 hours)
    if (!string.IsNullOrEmpty(idempotencyKey))
    {
        await _cache.SetAsync($"submit-report:{idempotencyKey}", result, TimeSpan.FromHours(24));
    }
    
    return Ok(result);
}
```

---

## **Validation Strategy**

**FluentValidation for Input Validation:**

```csharp
public class UploadReportCommandValidator : AbstractValidator<UploadReportCommand>
{
    public UploadReportCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Report file is required")
            .Must(BeValidFileType).WithMessage("Only XLSX files are allowed")
            .Must(BeValidFileSize).WithMessage("File size must not exceed 100 MB");
        
        RuleFor(x => x.EntityId)
            .GreaterThan(0).WithMessage("Valid entity ID is required");
        
        RuleFor(x => x.ReportingPeriod)
            .NotEmpty().WithMessage("Reporting period is required")
            .Matches(@"^Q[1-4]_\d{4}$|^Y_\d{4}$").WithMessage("Invalid reporting period format");
    }
    
    private bool BeValidFileType(IFormFile file)
    {
        var allowedExtensions = new[] { ".xlsx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
    
    private bool BeValidFileSize(IFormFile file)
    {
        const long maxSize = 100 * 1024 * 1024; // 100 MB
        return file.Length <= maxSize;
    }
}

// MediatR pipeline behavior for automatic validation
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new ValidationException("Validation failed", errors);
        }

        return await next();
    }
}
```

---

## **Security Considerations**

**What NOT to Log:**
- ❌ Passwords (even hashed)
- ❌ JWT tokens or refresh tokens
- ❌ PESEL numbers (log only last 4 digits)
- ❌ Full email addresses in error messages visible to users
- ❌ Database connection strings
- ❌ API keys or secrets
- ❌ File contents (log file names and sizes only)
- ❌ Sensitive entity data (financial details)

**Safe to Log:**
- ✅ User ID (GUID)
- ✅ Entity ID (numeric ID)
- ✅ Correlation IDs
- ✅ Request paths and methods
- ✅ Status codes
- ✅ Execution times
- ✅ Validation error types (not values)
- ✅ Exception types and stack traces (in development)

**Error Messages in Production:**
```csharp
// Development: Detailed error
"Database connection failed: Connection timeout to postgres:5432"

// Production: Generic error
"An error occurred while processing your request. Please try again later. (CorrelationId: 3fa85f64-5717-4562-b3fc-2c963f66afa6)"
```

---

## **Frontend Error Handling**

**HTTP Interceptor:**

```typescript
// Angular error interceptor
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private notificationService: NotificationService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An unexpected error occurred';
        let errorCode = 'UNKNOWN_ERROR';

        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = error.error.message;
        } else {
          // Server-side error
          if (error.error?.message) {
            errorMessage = error.error.message;
            errorCode = error.error.errorCode || 'SERVER_ERROR';
            
            // Handle specific error types
            switch (error.status) {
              case 401:
                this.router.navigate(['/login']);
                break;
              case 403:
                this.notificationService.showError('You do not have permission to perform this action');
                break;
              case 404:
                this.notificationService.showError('The requested resource was not found');
                break;
              case 422:
                // Business rule violation - show specific message
                this.notificationService.showWarning(errorMessage);
                break;
              case 500:
                this.notificationService.showError('A server error occurred. Please try again later.');
                break;
            }
            
            // Show validation errors
            if (error.error.errors) {
              Object.keys(error.error.errors).forEach(field => {
                error.error.errors[field].forEach((msg: string) => {
                  this.notificationService.showValidationError(field, msg);
                });
              });
            }
          }
        }

        // Log to console in development
        if (!environment.production) {
          console.error('HTTP Error:', {
            status: error.status,
            message: errorMessage,
            correlationId: error.error?.correlationId,
            url: request.url
          });
        }

        return throwError(() => ({
          message: errorMessage,
          code: errorCode,
          correlationId: error.error?.correlationId
        }));
      })
    );
  }
}
```

---

## **Monitoring and Alerting (Future Enhancement)**

For production deployment beyond hackathon:

- **Application Insights / OpenTelemetry**: Distributed tracing
- **Prometheus + Grafana**: Metrics and dashboards
- **Sentry / Raygun**: Error tracking and alerting
- **Health Check Dashboard**: Real-time service status

---

## **Rationale:**

1. **Consistent Error Format**: All API errors follow the same JSON structure with correlation IDs for debugging

2. **Layered Exception Handling**: Exceptions defined at domain level, caught and formatted at API level

3. **Security First**: Never expose sensitive data or internal details in error messages (especially production)

4. **Debuggability**: Correlation IDs tie logs across services; structured logging enables efficient querying

5. **Resilience**: Retry policies and circuit breakers handle transient failures gracefully

6. **Developer Experience**: Clear error messages and validation feedback help developers build correct integrations

7. **User Experience**: Friendly, actionable error messages for end users; technical details only in logs

8. **Idempotency**: Prevents duplicate submissions and ensures safe retries

9. **Transaction Management**: ACID guarantees for critical operations

10. **Hackathon-Ready**: Error handling is comprehensive but not over-engineered; focuses on common scenarios
