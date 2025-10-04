# **Section 15: Security**

This section defines **MANDATORY** security requirements for AI and human developers. These rules directly impact code generation and system safety.

---

## **Input Validation**

- **Validation Library:** FluentValidation 11.9.0
- **Validation Location:** API boundary (before processing) - all commands/queries validated via MediatR pipeline
- **Required Rules:**
  - All external inputs MUST be validated
  - Validation at API boundary before processing
  - Whitelist approach preferred over blacklist
  - Fail closed (deny by default)

**Validation Requirements by Type:**

```csharp
// File uploads
public class FileUploadValidator : AbstractValidator<IFormFile>
{
    private const long MAX_FILE_SIZE = 100 * 1024 * 1024; // 100 MB
    private static readonly string[] ALLOWED_EXTENSIONS = 
        { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt", ".mp3", ".zip" };

    public FileUploadValidator()
    {
        RuleFor(file => file.Length)
            .LessThanOrEqualTo(MAX_FILE_SIZE)
            .WithMessage("File size must not exceed 100 MB");

        RuleFor(file => file.FileName)
            .Must(HasAllowedExtension)
            .WithMessage($"Only the following file types are allowed: {string.Join(", ", ALLOWED_EXTENSIONS)}");

        RuleFor(file => file.ContentType)
            .Must(BeValidContentType)
            .WithMessage("Invalid file content type");
    }

    private bool HasAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return ALLOWED_EXTENSIONS.Contains(extension);
    }

    private bool BeValidContentType(string contentType)
    {
        // Verify content type matches file extension
        // Prevents spoofing with mismatched headers
        return true; // Implement actual validation
    }
}

// Email validation
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .MaximumLength(256)
    .WithMessage("Valid email address is required");

// Phone validation (international format)
RuleFor(x => x.Phone)
    .NotEmpty()
    .Matches(@"^\+(?:[0-9] ?){6,14}[0-9]$")
    .WithMessage("Phone number must be in international format (e.g., +48123456789)");

// PESEL validation
RuleFor(x => x.Pesel)
    .NotEmpty()
    .Length(11)
    .Matches(@"^\d{11}$")
    .Must(BeValidPesel)
    .WithMessage("Invalid PESEL number");

// SQL injection prevention (not needed with EF Core but as documentation)
// EF Core parameterizes all queries automatically
// NEVER use string concatenation for queries:
// ❌ WRONG: _context.Users.FromSqlRaw($"SELECT * FROM Users WHERE Email = '{email}'")
// ✅ CORRECT: _context.Users.FromSqlRaw("SELECT * FROM Users WHERE Email = {0}", email)

// XSS prevention
// Angular sanitizes HTML by default
// NEVER use [innerHTML] with user-generated content without sanitization
// ✅ Use DomSanitizer when absolutely necessary
```

---

## **Authentication & Authorization**

- **Auth Method:** JWT (JSON Web Tokens) with refresh tokens
- **Session Management:** Redis-backed session storage for refresh tokens
- **Token Expiration:** Access token: 60 minutes, Refresh token: 7 days
- **Password Requirements:** Configurable via PasswordPolicy (min 8 chars, uppercase, lowercase, digit, special char)

**Required Patterns:**

**JWT Token Structure:**

```json
{
  "sub": "c8a5d9e2-1234-5678-9abc-def012345678",
  "email": "user@entity.com",
  "user_type": "External",
  "entity_id": "1001",
  "permissions": ["reporting.submit", "cases.create"],
  "iat": 1696435200,
  "exp": 1696438800,
  "iss": "UknfPlatform",
  "aud": "UknfPlatformUsers"
}
```

**Authentication Implementation:**

```csharp
public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ICacheService _cache;

    public string GenerateAccessToken(User user, long? entityId = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("user_type", user.UserType.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (entityId.HasValue)
            claims.Add(new Claim("entity_id", entityId.Value.ToString()));

        // Add permissions
        var permissions = await _permissionService.GetUserPermissions(user.Id, entityId);
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshToken(Guid userId)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        // Store in Redis with expiration
        await _cache.SetAsync(
            $"refresh-token:{userId}",
            refreshToken,
            TimeSpan.FromDays(7));

        return refreshToken;
    }

    public async Task RevokeRefreshToken(Guid userId)
    {
        await _cache.RemoveAsync($"refresh-token:{userId}");
    }
}
```

**Authorization - Policy-Based:**

```csharp
// Define authorization policies
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("UknfEmployeeOnly", policy => 
        policy.RequireClaim("user_type", "Internal"));
    
    options.AddPolicy("EntityAdminOnly", policy =>
        policy.RequireClaim("permission", "entity.admin"));
    
    // Permission-based policies
    options.AddPolicy("CanSubmitReports", policy =>
        policy.RequireClaim("permission", "reporting.submit"));
    
    options.AddPolicy("CanManageCases", policy =>
        policy.RequireClaim("permission", "cases.manage"));
    
    // Custom policy with handler
    options.AddPolicy("CanAccessEntity", policy =>
        policy.Requirements.Add(new EntityAccessRequirement()));
});

// Custom authorization handler
public class EntityAccessHandler : AuthorizationHandler<EntityAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EntityAccessRequirement requirement)
    {
        var entityIdClaim = context.User.FindFirst("entity_id")?.Value;
        var requestedEntityId = _httpContextAccessor.HttpContext?.Items["EntityId"]?.ToString();

        if (entityIdClaim == requestedEntityId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

// Usage in controllers
[Authorize(Policy = "CanSubmitReports")]
[HttpPost("reports/upload")]
public async Task<IActionResult> UploadReport(...)
{
    // Only users with "reporting.submit" permission can access
}
```

**Password Security:**

```csharp
public class PasswordService
{
    public string HashPassword(string password)
    {
        // Use BCrypt with work factor 12 (2^12 iterations)
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public bool MeetsPasswordPolicy(string password, PasswordPolicy policy)
    {
        if (password.Length < policy.MinLength)
            return false;

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
            return false;

        if (policy.RequireLowercase && !password.Any(char.IsLower))
            return false;

        if (policy.RequireDigit && !password.Any(char.IsDigit))
            return false;

        if (policy.RequireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            return false;

        return true;
    }

    public async Task<bool> IsPasswordInHistory(Guid userId, string newPassword)
    {
        var passwordHistory = await _passwordHistoryRepository
            .GetRecentPasswordsAsync(userId, limit: 5);

        return passwordHistory.Any(hash => BCrypt.Net.BCrypt.Verify(newPassword, hash));
    }
}
```

---

## **Secrets Management**

- **Development:** appsettings.Development.json (NOT committed to git, in .gitignore)
- **Production:** Environment variables or Azure Key Vault / AWS Secrets Manager
- **Code Requirements:**
  - NEVER hardcode secrets (API keys, passwords, connection strings)
  - Access via IConfiguration or strongly-typed settings classes
  - No secrets in logs or error messages
  - Use User Secrets for local development (`dotnet user-secrets`)

**Configuration Pattern:**

```csharp
// ❌ WRONG - Hardcoded secrets
var connectionString = "Server=postgres;Database=uknf;User=admin;Password=secret123";

// ✅ CORRECT - Configuration-based
public class DatabaseSettings
{
    public string ConnectionString { get; set; }
}

// In Program.cs
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));

// In service
public class MyService
{
    private readonly string _connectionString;

    public MyService(IOptions<DatabaseSettings> settings)
    {
        _connectionString = settings.Value.ConnectionString;
    }
}

// In docker-compose.yml - environment variables
environment:
  ConnectionStrings__DefaultConnection: ${DATABASE_CONNECTION_STRING}
  MinIO__SecretKey: ${MINIO_SECRET_KEY}
  JWT__SecretKey: ${JWT_SECRET_KEY}
```

**Secrets in .gitignore:**

```
# Sensitive configuration files
appsettings.Development.json
appsettings.Production.json
*.secrets.json

# User secrets
secrets/
.env
.env.local

# Database files
*.db
*.db-shm
*.db-wal
```

---

## **API Security**

- **Rate Limiting:** AspNetCoreRateLimit 5.0.0
  - Per-user: 100 requests/minute
  - Global: 1000 requests/minute
  - Stricter limits for sensitive endpoints (login: 5 attempts/15 minutes)
  
- **CORS Policy:** 
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowFrontend", builder =>
      {
          builder.WithOrigins("http://localhost:4200", "https://uknf-demo.example.com")
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
      });
  });
  ```

- **Security Headers:** 
  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
      context.Response.Headers.Add("X-Frame-Options", "DENY");
      context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
      context.Response.Headers.Add("Referrer-Policy", "no-referrer");
      context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
      
      // HSTS (only in production over HTTPS)
      if (context.Request.IsHttps)
          context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

      await next();
  });
  ```

- **HTTPS Enforcement:** 
  ```csharp
  // Redirect HTTP to HTTPS in production
  if (app.Environment.IsProduction())
  {
      app.UseHttpsRedirection();
      app.UseHsts();
  }
  ```

**Rate Limiting Configuration:**

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*/auth/login",
        "Period": "15m",
        "Limit": 5
      },
      {
        "Endpoint": "*/reports/upload",
        "Period": "1h",
        "Limit": 50
      }
    ]
  }
}
```

---

## **Data Protection**

- **Encryption at Rest:** 
  - Database: PostgreSQL encryption (TDE - Transparent Data Encryption in production)
  - File storage: MinIO with server-side encryption (SSE)
  - Sensitive fields: AES-256 encryption for PESEL numbers
  
- **Encryption in Transit:** 
  - TLS 1.3 for all external connections
  - HTTPS enforced in production
  - Database connections over SSL/TLS

- **PII Handling Rules:**
  - PESEL: Encrypt in database, display last 4 digits only
  - Email: Store plaintext (needed for communication), never log in error messages
  - Passwords: BCrypt hashed (never stored in plaintext, never logged)
  - Phone numbers: Store as entered, validate format

- **Logging Restrictions - NEVER LOG:**
  - ❌ Passwords (plain or hashed)
  - ❌ JWT tokens or refresh tokens
  - ❌ Full PESEL (log last 4 digits only: `****1234`)
  - ❌ Credit card numbers or financial data
  - ❌ Session IDs or authentication cookies
  - ❌ Personal addresses (log only city/country if needed)
  - ❌ File contents (log filenames and metadata only)

**Data Encryption Implementation:**

```csharp
public class EncryptionService
{
    private readonly string _encryptionKey;

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_encryptionKey);
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        using var msEncrypt = new MemoryStream();
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);
        
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_encryptionKey);
        
        var iv = new byte[aes.IV.Length];
        var cipher = new byte[cipherBytes.Length - iv.Length];
        
        Array.Copy(cipherBytes, iv, iv.Length);
        Array.Copy(cipherBytes, iv.Length, cipher, 0, cipher.Length);
        
        aes.IV = iv;
        
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }
}

// Usage for PESEL
public class User
{
    private string _peselEncrypted;
    
    [NotMapped]
    public string Pesel
    {
        get => _encryptionService.Decrypt(_peselEncrypted);
        set
        {
            _peselEncrypted = _encryptionService.Encrypt(value);
            PeselLast4 = value.Substring(value.Length - 4);
        }
    }
    
    public string PeselLast4 { get; private set; } // For display only
}
```

---

## **Dependency Security**

- **Scanning Tool:** 
  - .NET: `dotnet list package --vulnerable`
  - npm: `npm audit`
  - GitHub Dependabot (automated alerts)

- **Update Policy:** 
  - Security patches: Apply within 48 hours
  - Minor updates: Monthly review
  - Major updates: Quarterly evaluation

- **Approval Process:** 
  - All new dependencies require security review
  - Check for known vulnerabilities (CVE database)
  - Verify package authenticity and maintainer reputation
  - Prefer well-established packages with active maintenance

**Automated Dependency Scanning:**

```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  schedule:
    - cron: '0 0 * * 0' # Weekly on Sunday
  push:
    branches: [ main ]

jobs:
  dependency-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Check vulnerable packages (.NET)
        run: dotnet list package --vulnerable --include-transitive
        
      - name: Check vulnerable packages (npm)
        run: npm audit
        working-directory: src/Frontend/uknf-platform-ui
```

---

## **Security Testing**

- **SAST Tool:** SonarAnalyzer.CSharp (integrated in build)
- **DAST Tool:** OWASP ZAP (optional for hackathon, recommended for production)
- **Penetration Testing:** Not applicable for hackathon demo

**Static Analysis Configuration:**

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.21.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

---

## **Security Checklist (Pre-Deployment)**

**Authentication & Authorization:**
- ✅ All endpoints require authentication (except public routes)
- ✅ JWT tokens expire after reasonable time (60 minutes)
- ✅ Refresh tokens stored securely (Redis with expiration)
- ✅ Password policy enforced (min 8 chars, complexity requirements)
- ✅ Failed login attempts rate-limited (5 attempts per 15 minutes)
- ✅ Authorization checks use policies, not role strings

**Input Validation:**
- ✅ All user inputs validated using FluentValidation
- ✅ File uploads validated (type, size, content)
- ✅ SQL injection prevented (EF Core parameterizes automatically)
- ✅ XSS prevention enabled (Angular sanitizes by default)

**Data Protection:**
- ✅ Sensitive data encrypted (PESEL)
- ✅ Passwords hashed with BCrypt
- ✅ TLS/HTTPS enforced in production
- ✅ No sensitive data in logs

**API Security:**
- ✅ Rate limiting configured
- ✅ CORS policy restrictive
- ✅ Security headers set
- ✅ HTTPS redirection enabled

**Secrets Management:**
- ✅ No hardcoded secrets in code
- ✅ Secrets loaded from environment variables
- ✅ appsettings.Development.json in .gitignore
- ✅ Production secrets use key vault

**Dependencies:**
- ✅ All packages scanned for vulnerabilities
- ✅ No known high/critical CVEs
- ✅ Dependabot enabled for automated alerts

---

## **Incident Response (Production)**

For production deployment beyond hackathon:

1. **Detection:** Monitor logs for suspicious patterns (failed logins, unusual traffic)
2. **Response:** Revoke compromised tokens, reset affected passwords
3. **Recovery:** Restore from backups if data compromised
4. **Post-Mortem:** Document incident, update security measures

---

## **Rationale:**

1. **Defense in Depth:** Multiple layers of security (authentication, authorization, validation, encryption)

2. **Principle of Least Privilege:** Users only have permissions they need; entity-based data isolation

3. **Secure by Default:** Security headers, HTTPS redirection, rate limiting enabled out of the box

4. **OWASP Top 10 Coverage:** Addresses injection, broken auth, XSS, insecure deserialization, etc.

5. **GDPR/Privacy Considerations:** PII encryption, logging restrictions, data retention policies

6. **Audit Trail:** Comprehensive logging (without sensitive data) for security investigations

7. **Hackathon-Appropriate:** Strong security without over-engineering; focuses on common vulnerabilities

8. **AI-Friendly:** Clear rules that AI agents can follow consistently (never log X, always validate Y)

9. **Production-Ready Foundation:** Can be enhanced for production (key vault, pen testing, WAF)

10. **Compliance-Conscious:** Addresses requirements typical of financial sector regulations
