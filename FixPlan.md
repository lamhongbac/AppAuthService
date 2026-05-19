# AppAuthService - Detailed Fix Plan

**Based on:** TechnicalReport_Improvements.md  
**Estimated Duration:** 8 weeks (4 phases)  
**Total Issues:** 37  

---

## PHASE 1: CRITICAL SECURITY FIXES (Week 1-2)

### Task 1.1: Remove Hardcoded Database Credentials
**Priority:** P0 - Critical  
**Files:** `appsettings.json`, `.gitignore`, `Program.cs`

**Steps:**
1. Add `appsettings.*.json` patterns to `.gitignore` (keep `appsettings.json` template only)
2. Create `appsettings.example.json` with placeholder connection string
3. Modify connection string to use environment variable:
   ```json
   "DefaultConnection": "Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};..."
   ```
4. Update `Program.cs` to read from environment variables or User Secrets in Development
5. Document setup in README

**Acceptance Criteria:**
- [ ] No credentials in source control
- [ ] App works with environment variables
- [ ] README updated with configuration instructions

---

### Task 1.2: Fix CORS Policy
**Priority:** P0 - Critical  
**Files:** `Program.cs`, `appsettings.json`

**Steps:**
1. Add `AllowedOrigins` array to `appsettings.json`:
   ```json
   "Cors": {
     "AllowedOrigins": ["https://yourdomain.com", "https://app.yourdomain.com"]
   }
   ```
2. Update `Program.cs`:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("ProductionPolicy", builder =>
       {
           builder.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
       });
   });
   ```
3. Use different policies for Development vs Production

**Acceptance Criteria:**
- [ ] CORS configured with specific origins
- [ ] Different policies per environment
- [ ] `AllowAnyOrigin()` removed

---

### Task 1.3: Replace Password Hashing with PBKDF2
**Priority:** P0 - Critical  
**Files:** `AppAuth.BLL\Services\AuthService.cs`, New utility class

**Steps:**
1. Create `PasswordHasher.cs` in `AppAuth.Utils`:
   ```csharp
   public static class PasswordHasher
   {
       public static string Hash(string password, out string salt)
       {
           salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
           return Convert.ToBase64String(KeyDerivation.Pbkdf2(
               password: password,
               salt: Convert.FromBase64String(salt),
               prf: KeyDerivationPrf.HMACSHA256,
               iterationCount: 100000,
               numBytesRequested: 256 / 8));
       }

       public static bool Verify(string password, string salt, string hash)
       {
           var computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
               password: password,
               salt: Convert.FromBase64String(salt),
               prf: KeyDerivationPrf.HMACSHA256,
               iterationCount: 100000,
               numBytesRequested: 256 / 8));
           return computedHash == hash;
       }
   }
   ```
2. Update `AuthService.cs` line 92-106 to use new hasher
3. Create migration script to re-hash existing passwords on next login
4. Add `PasswordHash` and `PasswordSalt` columns to `AppUser` table

**Acceptance Criteria:**
- [ ] PBKDF2 with 100,000 iterations implemented
- [ ] All password operations use new hasher
- [ ] Migration path for existing passwords documented

---

### Task 1.4: Move Blacklist to Database
**Priority:** P0 - Critical  
**Files:** `AppAuth.BLL\Services\BlacklistService.cs`, `AuthService.DAL\Models\`, `AuthService.DAL\Repos\`

**Steps:**
1. Create `BlacklistEntry` model in `AuthService.DAL\Models\`:
   ```csharp
   public class BlacklistEntry
   {
       public int Id { get; set; }
       public string Type { get; set; } // "ip" or "user"
       public string Value { get; set; }
       public string Reason { get; set; }
       public DateTime CreatedAt { get; set; }
       public DateTime? ExpiresAt { get; set; }
       public bool IsActive { get; set; } = true;
   }
   ```
2. Add `DbSet<BlacklistEntry>` to `AppAuthServiceContext`
3. Create `IBlacklistRepository` and `BlacklistRepository`
4. Rewrite `BlacklistService` to use repository instead of file I/O
5. Add expiration logic for temporary blacklists
6. Delete `blacklist.json` file

**Acceptance Criteria:**
- [ ] Blacklist stored in database
- [ ] File I/O completely removed
- [ ] Expiration support added
- [ ] Thread-safe without file locks

---

### Task 1.5: Fix Exception Message Exposure
**Priority:** P0 - Critical  
**Files:** `AppAuth.BLL\Services\AuthService.cs`, All service files

**Steps:**
1. Create `ErrorResponse.cs` in `AuthService.Shared`:
   ```csharp
   public static class ErrorMessages
   {
       public const string GeneralError = "An unexpected error occurred. Please try again later.";
       public const string AuthFailed = "Authentication failed.";
       public const string Unauthorized = "You are not authorized to perform this action.";
   }
   ```
2. Replace all `ex.Message` in catch blocks with generic messages
3. Log detailed exceptions using ILogger (prepare for Task 1.6)
4. Create exception middleware for global error handling

**Acceptance Criteria:**
- [ ] No `ex.Message` returned to clients
- [ ] Generic error messages used
- [ ] Detailed errors logged server-side

---

### Task 1.6: Implement Structured Logging with Serilog
**Priority:** P1 - High  
**Files:** `Program.cs`, All service files

**Steps:**
1. Add NuGet packages:
   - `Serilog.AspNetCore`
   - `Serilog.Sinks.Console`
   - `Serilog.Sinks.File`
   - `Serilog.Enrichers.Environment`
   - `Serilog.Enrichers.CorrelationId`
2. Configure in `Program.cs`:
   ```csharp
   Log.Logger = new LoggerConfiguration()
       .Enrich.FromLogContext()
       .Enrich.WithCorrelationId()
       .WriteTo.Console()
       .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
       .CreateLogger();
   builder.Host.UseSerilog();
   ```
3. Inject `ILogger<T>` into all services
4. Replace all `Console.WriteLine` with proper logging
5. Add correlation ID middleware

**Acceptance Criteria:**
- [ ] Serilog configured and working
- [ ] All `Console.WriteLine` replaced
- [ ] Correlation IDs in logs
- [ ] Daily log rotation

---

## PHASE 2: ARCHITECTURE REFACTORING (Week 3-4)

### Task 2.1: Fix N+1 Query Problem
**Priority:** P1 - High  
**Files:** `AppAuth.BLL\Services\AuthService.cs`, `AuthService.DAL\Repos\`

**Steps:**
1. Add repository methods with eager loading:
   ```csharp
   // In IRoleRightRepository
   Task<IEnumerable<RoleRight>> GetRightsByRoleIdsAsync(IEnumerable<int> roleIds);
   
   // In IAppObjectRepository
   Task<IEnumerable<AppObject>> GetByIdsAsync(IEnumerable<int> ids);
   ```
2. Implement with single queries:
   ```csharp
   public async Task<IEnumerable<RoleRight>> GetRightsByRoleIdsAsync(IEnumerable<int> roleIds)
   {
       return await _context.RoleRights
           .Include(rr => rr.AppObject)
           .Where(rr => roleIds.Contains(rr.RoleId))
           .ToListAsync();
   }
   ```
3. Rewrite `GetMergedRights` to use batch queries:
   ```csharp
   // Single query instead of N queries
   var roleRights = await _roleRightRepository.GetRightsByRoleIdsAsync(roleIds);
   var objectIds = roleRights.Select(rr => rr.AppObjectId).Distinct();
   var appObjects = await _appObjectRepository.GetByIdsAsync(objectIds);
   ```

**Acceptance Criteria:**
- [ ] `GetMergedRights` uses 2-3 queries instead of N+1
- [ ] SQL profiling shows reduced query count
- [ ] Performance test shows improvement

---

### Task 2.2: Split AuthService God Class
**Priority:** P1 - High  
**Files:** `AppAuth.BLL\Services\AuthService.cs` (611 lines)

**Steps:**
1. Create `AuthenticationService.cs`:
   - `AuthenticateAsync()`
   - Password validation
   - User lookup
   
2. Create `TokenService.cs`:
   - `RefreshTokenAsync()`
   - `SwitchAppAsync()`
   - JWT generation logic
   
3. Create `AuthorizationService.cs`:
   - `AuthorizeAsync()`
   - `GetMergedRights()`
   
4. Create `RightsCalculator.cs` (helper):
   - Rights merging logic
   - Used by both auth and token services

5. Update `IAuthService` interface or create new interfaces
6. Update DI registration in `Program.cs`
7. Update controllers to use new services

**Acceptance Criteria:**
- [ ] No class exceeds 200 lines
- [ ] Each service has single responsibility
- [ ] All existing tests pass
- [ ] No breaking changes to API

---

### Task 2.3: Add Database Indexes
**Priority:** P1 - High  
**Files:** `AuthService.DAL\Models\AppAuthServiceContext.cs`

**Steps:**
1. Add indexes in `OnModelCreating`:
   ```csharp
   modelBuilder.Entity<AppUser>(entity =>
   {
       entity.HasIndex(u => u.UserName).IsUnique();
       entity.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
       entity.HasIndex(u => u.Mobile).HasFilter("[Mobile] IS NOT NULL");
       entity.HasIndex(u => u.CardNum).HasFilter("[CardNum] IS NOT NULL");
   });

   modelBuilder.Entity<Application>(entity =>
   {
       entity.HasIndex(a => new { a.CompanyId, a.Number }).IsUnique();
   });

   modelBuilder.Entity<AppObject>(entity =>
   {
       entity.HasIndex(o => new { o.AppId, o.Number }).IsUnique();
   });

   modelBuilder.Entity<RefTokenTracking>(entity =>
   {
       entity.HasIndex(t => t.TokenHash);
       entity.HasIndex(t => t.UserId);
       entity.HasIndex(t => new { t.UserId, t.IsRevoked, t.IsUsed });
   });

   modelBuilder.Entity<UserRole>(entity =>
   {
       entity.HasIndex(ur => new { ur.UserId, ur.AppRoleId }).IsUnique();
   });
   ```

**Acceptance Criteria:**
- [ ] All frequently queried columns indexed
- [ ] Composite indexes for common query patterns
- [ ] Migration created for indexes

---

### Task 2.4: Implement EF Core Migrations
**Priority:** P1 - High  
**Files:** `Program.cs`, New Migrations folder

**Steps:**
1. Install `Microsoft.EntityFrameworkCore.Tools` NuGet package
2. Replace `context.Database.EnsureCreated()` with `context.Database.Migrate()`
3. Create initial migration:
   ```bash
   dotnet ef migrations add InitialCreate -p AuthService.DAL -s AppAuthService
   ```
4. Create migration for blacklist table (from Task 1.4)
5. Create migration for indexes (from Task 2.3)
6. Document migration commands in README

**Acceptance Criteria:**
- [ ] `EnsureCreated()` replaced with `Migrate()`
- [ ] Initial migration created and tested
- [ ] Migration commands documented

---

### Task 2.5: Implement Soft Delete Global Filter
**Priority:** P1 - High  
**Files:** `AuthService.DAL\Models\AppAuthServiceContext.cs`

**Steps:**
1. Add global query filters in `OnModelCreating`:
   ```csharp
   modelBuilder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
   modelBuilder.Entity<Application>().HasQueryFilter(a => !a.IsDeleted);
   modelBuilder.Entity<AppObject>().HasQueryFilter(o => !u.IsDeleted);
   modelBuilder.Entity<AppRole>().HasQueryFilter(r => !r.IsDeleted);
   modelBuilder.Entity<Company>().HasQueryFilter(c => !c.IsDeleted);
   ```
2. Add method to query deleted records when needed:
   ```csharp
   public IQueryable<AppUser> IncludeDeleted() => _context.AppUsers.IgnoreQueryFilters();
   ```
3. Update `BaseRepository` to respect filters

**Acceptance Criteria:**
- [ ] All entities with `IsDeleted` automatically filtered
- [ ] Explicit method to include deleted records
- [ ] Existing queries still work

---

### Task 2.6: Add Transaction Management
**Priority:** P1 - High  
**Files:** `AppAuth.BLL\Services\` (various)

**Steps:**
1. Create `IUnitOfWork` interface:
   ```csharp
   public interface IUnitOfWork : IDisposable
   {
       Task<int> SaveChangesAsync(CancellationToken ct = default);
       Task<IDbContextTransaction> BeginTransactionAsync();
   }
   ```
2. Implement `UnitOfWork` wrapping DbContext
3. Wrap multi-entity operations in transactions:
   - Token refresh (mark old + create new)
   - User creation + role assignment
   - Blacklist operations

**Acceptance Criteria:**
- [ ] All multi-entity operations wrapped in transactions
- [ ] Rollback on failure
- [ ] Unit tests for transaction scenarios

---

## PHASE 3: API & CODE QUALITY (Week 5-6)

### Task 3.1: Standardize HTTP Status Codes
**Priority:** P2 - Medium  
**Files:** All controllers

**Steps:**
1. Create response mapping:
   | Scenario | Status Code |
   |----------|-------------|
   | Validation error | 400 Bad Request |
   | Invalid credentials | 400 Bad Request (not 401) |
   | Missing/invalid token | 401 Unauthorized |
   | Insufficient permissions | 403 Forbidden |
   | Resource not found | 404 Not Found |
   | Success | 200 OK |
   | Created | 201 Created |

2. Update all controller responses to match standard
3. Create helper method in `BaseController`:
   ```csharp
   protected ActionResult Response(BOProcessResult result) => result.OK switch
   {
       true => Ok(result),
       false when result.ErrorCode is 302 or 400 => BadRequest(result),
       false when result.ErrorCode is 304 => Unauthorized(result),
       false when result.ErrorCode is 403 => Forbid(),
       false when result.ErrorCode is 404 => NotFound(result),
       _ => StatusCode(500, result)
   };
   ```

**Acceptance Criteria:**
- [ ] All endpoints return correct status codes
- [ ] Consistent error response format
- [ ] API documentation updated

---

### Task 3.2: Add Rate Limiting
**Priority:** P2 - Medium  
**Files:** `Program.cs`, `appsettings.json`

**Steps:**
1. Add NuGet package: `Microsoft.AspNetCore.RateLimiting`
2. Configure in `Program.cs`:
   ```csharp
   builder.Services.AddRateLimiter(options =>
   {
       options.AddPolicy("login", httpContext =>
           RateLimitPartition.GetFixedWindowLimiter(
               partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
               factory: _ => new FixedWindowRateLimiterOptions
               {
                   PermitLimit = 5,
                   Window = TimeSpan.FromMinutes(1)
               }));
       
       options.AddPolicy("api", httpContext =>
           RateLimitPartition.GetFixedWindowLimiter(
               partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
               factory: _ => new FixedWindowRateLimiterOptions
               {
                   PermitLimit = 100,
                   Window = TimeSpan.FromMinutes(1)
               }));
   });
   ```
3. Apply to endpoints:
   ```csharp
   [HttpPost("login")]
   [EnableRateLimiting("login")]
   public async Task<ActionResult> Login(...)
   ```

**Acceptance Criteria:**
- [ ] Login endpoint limited to 5 requests/minute per IP
- [ ] General API limited to 100 requests/minute per IP
- [ ] Proper 429 Too Many Requests response

---

### Task 3.3: Add Input Validation Attributes
**Priority:** P2 - Medium  
**Files:** `AuthService.Shared\RequestResponse\*.cs`

**Steps:**
1. Add data annotations to all request DTOs:
   ```csharp
   public class LoginRequest
   {
       [Required(ErrorMessage = "Username is required")]
       [StringLength(50, MinimumLength = 3)]
       public string UserName { get; set; }

       [Required(ErrorMessage = "Password is required")]
       [StringLength(100, MinimumLength = 6)]
       public string Password { get; set; }

       [Required]
       [StringLength(20)]
       public string AppNumber { get; set; }
   }
   ```
2. Add custom validation attributes for domain rules
3. Enable automatic model state validation filter

**Acceptance Criteria:**
- [ ] All request DTOs have validation attributes
- [ ] Invalid requests return 400 with validation errors
- [ ] No manual validation checks needed in controllers

---

### Task 3.4: Implement Pagination
**Priority:** P2 - Medium  
**Files:** `AuthService.DAL\Repos\BaseRepository.cs`, Controllers

**Steps:**
1. Create `PagedResult<T>` in `AuthService.Shared`:
   ```csharp
   public class PagedResult<T>
   {
       public IEnumerable<T> Items { get; set; }
       public int TotalCount { get; set; }
       public int Page { get; set; }
       public int PageSize { get; set; }
       public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
   }
   ```
2. Add pagination to `BaseRepository`:
   ```csharp
   public async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize)
   {
       var totalCount = await _dbSet.CountAsync();
       var items = await _dbSet
           .Skip((page - 1) * pageSize)
           .Take(pageSize)
           .ToListAsync();
       return new PagedResult<T> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
   }
   ```
3. Update list endpoints to accept pagination parameters

**Acceptance Criteria:**
- [ ] All list endpoints support pagination
- [ ] Default page size: 20, max: 100
- [ ] Response includes total count and page info

---

### Task 3.5: Eliminate Magic Strings
**Priority:** P2 - Medium  
**Files:** Multiple files across solution

**Steps:**
1. Create `JwtClaimTypes.cs`:
   ```csharp
   public static class JwtClaimTypes
   {
       public const string UserName = "UserName";
       public const string AppNumber = "AppNumber";
       public const string UserId = ClaimTypes.NameIdentifier;
   }
   ```
2. Create `RightTypes.cs`:
   ```csharp
   public static class RightTypes
   {
       public const string List = "list";
       public const string Read = "read";
       public const string Create = "create";
       public const string Update = "update";
       public const string Delete = "delete";
   }
   ```
3. Replace all hardcoded strings with constants
4. Update `MSAAuthorizeAttribute` to use enum instead of string

**Acceptance Criteria:**
- [ ] No hardcoded claim types, right types, or action names
- [ ] All constants in dedicated classes
- [ ] IntelliSense support for all string values

---

### Task 3.6: Add XML Documentation
**Priority:** P2 - Medium  
**Files:** All public classes and methods

**Steps:**
1. Enable XML documentation in `.csproj` files:
   ```xml
   <PropertyGroup>
       <GenerateDocumentationFile>true</GenerateDocumentationFile>
       <NoWarn>$(NoWarn);1591</NoWarn>
   </PropertyGroup>
   ```
2. Add XML comments to all public APIs:
   ```csharp
   /// <summary>
   /// Authenticates a user and returns JWT tokens.
   /// </summary>
   /// <param name="apiKey">Company API key from x-api-key header</param>
   /// <param name="request">Login credentials and app number</param>
   /// <returns>Process result containing user data and JWT tokens</returns>
   Task<BOProcessResult> AuthenticateAsync(string apiKey, LoginRequest request);
   ```
3. Configure Swagger to use XML comments

**Acceptance Criteria:**
- [ ] All public methods have XML documentation
- [ ] Swagger UI shows method descriptions
- [ ] No CS1591 warnings

---

## PHASE 4: TESTING & DEVOPS (Week 7-8)

### Task 4.1: Increase Unit Test Coverage
**Priority:** P2 - Medium  
**Files:** `AppAuth.Tests\`

**Steps:**
1. Add test cases for all service methods:
   - AuthenticationService: 15+ tests
   - TokenService: 10+ tests
   - AuthorizationService: 10+ tests
   - BlacklistService: 5+ tests

2. Test scenarios to cover:
   - Happy path
   - Invalid inputs (null, empty, too long)
   - Edge cases (max values, special characters)
   - Error conditions (DB failures, invalid tokens)

3. Add controller tests:
   - AuthController: 8+ tests
   - UserController: 6+ tests
   - CompanyController: 4+ tests

4. Add JWT utility tests:
   - Token generation
   - Token validation
   - Token expiration
   - Refresh token rotation

**Acceptance Criteria:**
- [ ] Minimum 70% code coverage
- [ ] All critical paths tested
- [ ] Tests run in CI pipeline

---

### Task 4.2: Add Integration Tests
**Priority:** P2 - Medium  
**Files:** `AppAuth.Tests\Integration\`

**Steps:**
1. Set up test database (SQLite in-memory or Testcontainers)
2. Create integration test base class:
   ```csharp
   public class IntegrationTestBase : IDisposable
   {
       protected readonly WebApplicationFactory<Program> _factory;
       protected readonly HttpClient _client;
       
       public IntegrationTestBase()
       {
           _factory = new WebApplicationFactory<Program>()
               .WithWebHostBuilder(builder =>
               {
                   builder.ConfigureServices(services =>
                   {
                       // Replace DB context with in-memory
                   });
               });
           _client = _factory.CreateClient();
       }
   }
   ```
3. Add integration tests for:
   - Full login flow
   - Token refresh flow
   - Authorization flow
   - CRUD operations

**Acceptance Criteria:**
- [ ] 10+ integration tests
- [ ] Tests use real HTTP pipeline
- [ ] Database state isolated per test

---

### Task 4.3: Add Health Checks
**Priority:** P3 - Low  
**Files:** `Program.cs`, New HealthController.cs

**Steps:**
1. Add NuGet packages:
   - `Microsoft.Extensions.Diagnostics.HealthChecks`
   - `AspNetCore.HealthChecks.SqlServer`
2. Configure in `Program.cs`:
   ```csharp
   builder.Services.AddHealthChecks()
       .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
       .AddDbContextCheck<AppAuthServiceContext>();
   ```
3. Map health endpoints:
   ```csharp
   app.MapHealthChecks("/health", new HealthCheckOptions
   {
       ResponseWriter = async (context, report) =>
       {
           context.Response.ContentType = "application/json";
           var result = JsonSerializer.Serialize(new
           {
               status = report.Status.ToString(),
               checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
           });
           await context.Response.WriteAsync(result);
       }
   });
   ```

**Acceptance Criteria:**
- [ ] `/health` endpoint returns 200 when healthy
- [ ] Database connectivity checked
- [ ] JSON response with detailed status

---

### Task 4.4: Create Docker Configuration
**Priority:** P3 - Low  
**Files:** New Dockerfile, docker-compose.yml

**Steps:**
1. Create `Dockerfile`:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
   WORKDIR /app
   EXPOSE 8080

   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY ["AppAuthService.sln", "."]
   # ... copy and build steps
   RUN dotnet publish -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=build /app/publish .
   ENTRYPOINT ["dotnet", "AppAuthService.dll"]
   ```
2. Create `docker-compose.yml`:
   ```yaml
   version: '3.8'
   services:
     appauth:
       build: .
       ports:
         - "8080:8080"
       environment:
         - ConnectionStrings__DefaultConnection=Server=db;...
       depends_on:
         - db
     db:
       image: mcr.microsoft.com/mssql/server:2022-latest
       environment:
         - ACCEPT_EULA=Y
         - SA_PASSWORD=YourStrongPassword
       ports:
         - "1433:1433"
   ```

**Acceptance Criteria:**
- [ ] Dockerfile builds successfully
- [ ] docker-compose starts API and database
- [ ] App accessible at localhost:8080

---

### Task 4.5: Set Up CI/CD Pipeline
**Priority:** P3 - Low  
**Files:** `.github/workflows/ci.yml`

**Steps:**
1. Create GitHub Actions workflow:
   ```yaml
   name: CI/CD Pipeline
   on:
     push:
       branches: [main, develop]
     pull_request:
       branches: [main]

   jobs:
     build:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - name: Setup .NET
           uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '8.0.x'
         - name: Restore
           run: dotnet restore
         - name: Build
           run: dotnet build --no-restore
         - name: Test
           run: dotnet test --no-build --verbosity normal
         - name: Publish
           run: dotnet publish -c Release -o ./publish
   ```
2. Add code quality gates (optional):
   - SonarQube analysis
   - Code coverage threshold

**Acceptance Criteria:**
- [ ] Pipeline runs on PR and push
- [ ] Build, test, and publish steps pass
- [ ] Coverage report generated

---

### Task 4.6: Implement Caching Strategy
**Priority:** P3 - Low  
**Files:** `AppAuth.BLL\Services\`, `Program.cs`

**Steps:**
1. Add IMemoryCache for development / Redis for production
2. Cache user rights:
   ```csharp
   public async Task<List<ObjectRight>> GetMergedRightsCached(int userId, int appId)
   {
       var cacheKey = $"rights:{userId}:{appId}";
       if (_cache.TryGetValue(cacheKey, out List<ObjectRight> cachedRights))
           return cachedRights;

       var rights = await GetMergedRights(userId, appId);
       _cache.Set(cacheKey, rights, TimeSpan.FromMinutes(30));
       return rights;
   }
   ```
3. Add cache invalidation on role changes
4. Cache JWT validation results (short TTL)

**Acceptance Criteria:**
- [ ] Rights cached with 30-minute TTL
- [ ] Cache invalidated on role changes
- [ ] Performance improvement measurable

---

## DEPENDENCY GRAPH

```
Phase 1 (Security)
├── 1.1 Remove credentials ──────────────────────────────┐
├── 1.2 Fix CORS ────────────────────────────────────────┤
├── 1.3 Password hashing ────────────────────────────────┤
├── 1.4 Blacklist to DB ────────────┐                    │
├── 1.5 Exception handling ─────────┤                    │
└── 1.6 Serilog logging ────────────┼────────────────────┘
                                    │
Phase 2 (Architecture)              │
├── 2.1 Fix N+1 queries ────────────┼────────────────────┐
├── 2.2 Split AuthService ──────────┼────────────────────┤
├── 2.3 Add indexes ────────────────┤                    │
├── 2.4 EF Migrations ──────────────┼────────────────────┤
├── 2.5 Soft delete filter ─────────┼────────────────────┤
└── 2.6 Transactions ───────────────┘                    │
                                                         │
Phase 3 (API & Quality)                                  │
├── 3.1 Status codes ────────────────────────────────────┤
├── 3.2 Rate limiting ───────────────────────────────────┤
├── 3.3 Input validation ────────────────────────────────┤
├── 3.4 Pagination ──────────────────────────────────────┤
├── 3.5 Magic strings ───────────────────────────────────┤
└── 3.6 XML documentation ───────────────────────────────┤
                                                         │
Phase 4 (Testing & DevOps)                               │
├── 4.1 Unit tests ──────────────────────────────────────┤
├── 4.2 Integration tests ───────────────────────────────┤
├── 4.3 Health checks ───────────────────────────────────┤
├── 4.4 Docker ──────────────────────────────────────────┤
├── 4.5 CI/CD ───────────────────────────────────────────┤
└── 4.6 Caching ─────────────────────────────────────────┘
```

---

## RISK ASSESSMENT

| Task | Risk Level | Mitigation |
|------|------------|------------|
| 1.3 Password hashing | High | Provide migration script for existing passwords |
| 1.4 Blacklist to DB | Medium | Keep file fallback during transition |
| 2.2 Split AuthService | High | Comprehensive tests before refactoring |
| 2.4 EF Migrations | Medium | Backup database before running migrations |
| 3.1 Status codes | Low | Update API documentation, notify consumers |
| 4.2 Integration tests | Medium | Use isolated test database |

---

## SUCCESS METRICS

| Metric | Current | Target |
|--------|---------|--------|
| Code Coverage | ~10% | 70%+ |
| Security Issues | 7 critical | 0 critical |
| N+1 Queries in GetMergedRights | 15+ queries | 2-3 queries |
| Largest Class (AuthService) | 611 lines | <200 lines |
| API Response Time (login) | ~500ms | <200ms |
| Test Count | 3 | 50+ |
| Hardcoded Secrets | 1 | 0 |
| File-based Storage | 1 (blacklist) | 0 |
