# Technical Report: AppAuthService - Areas for Improvement

**Date:** 2026-05-19  
**Project:** AppAuthService (.NET 8 Web API - Authentication & Authorization Microservice)  
**Architecture:** Clean Architecture (3 Layers)  

---

## 1. SECURITY ISSUES (Critical)

### 1.1. Hardcoded Database Credentials
**Location:** `appsettings.json:9`
```json
"DefaultConnection": "Server=42.96.19.182;Database=AppAuthService;User Id=msa;Password=HelloMs@2025;..."
```
**Problem:** Database credentials with password are committed to source control.
**Recommendation:**
- Use Azure Key Vault, AWS Secrets Manager, or environment variables
- Add `appsettings.json` to `.gitignore` and use `appsettings.Local.json` for development
- Implement secret rotation policy

### 1.2. Overly Permissive CORS Policy
**Location:** `Program.cs:13-21`
```csharp
builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
```
**Problem:** Allows any origin, method, and header - vulnerable to CSRF attacks.
**Recommendation:**
- Configure specific allowed origins per environment
- Use `builder.WithOrigins("https://yourdomain.com")`

### 1.3. BlacklistService Uses File-Based Storage
**Location:** `BlacklistService.cs:19`
```csharp
_filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blacklist.json");
```
**Problem:**
- File-based storage is not scalable for microservices
- Race conditions possible despite `lock` in multi-instance deployments
- Data loss on deployment/restart
- Not suitable for distributed systems

**Recommendation:**
- Move blacklist to database (SQL Server) or Redis cache
- Implement distributed caching with expiration

### 1.4. Password Hashing Uses Custom Implementation
**Location:** `AuthService.cs:92`
```csharp
string hashedPassword = SecurityUtility.HashToken(request.Password, user.PwdKey);
```
**Problem:** Custom hashing instead of industry-standard algorithms (bcrypt, Argon2, PBKDF2).
**Recommendation:**
- Use `Microsoft.AspNetCore.Cryptography.KeyDerivation` (PBKDF2)
- Or use BCrypt.Net-Next / Argon2 bindings

### 1.5. JWT Signing Algorithm - HMAC-SHA256
**Location:** `JwtUtil.cs:31`
```csharp
new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
```
**Problem:** Symmetric key (HS256) requires key sharing between services.
**Recommendation:**
- Consider asymmetric algorithms (RS256/ES256) for microservice architecture
- Use RSA/ECDSA keys for better key management

### 1.6. Sensitive Data in JWT Claims
**Location:** `JwtUtil.cs:37-44`
**Problem:** Email and personal data stored in JWT payload (base64 encoded, not encrypted).
**Recommendation:**
- Minimize claims to essential identifiers only (UserId, AppNumber)
- Fetch user details from API when needed

### 1.7. Exception Messages Exposed to Client
**Location:** `AuthService.cs:252`
```csharp
return BOProcessResult.Failure(ex.Message, (int)AuthServiceErrorCode.SystemError);
```
**Problem:** Raw exception messages can reveal internal implementation details.
**Recommendation:**
- Return generic error messages to clients
- Log detailed exceptions server-side only

---

## 2. ARCHITECTURE & DESIGN ISSUES

### 2.1. N+1 Query Problem in GetMergedRights
**Location:** `AuthService.cs:565-609`
**Problem:** Multiple sequential database calls in loop:
- 1 query for user roles
- N queries for role rights (one per role)
- M queries for app objects (one per right)

**Recommendation:**
```csharp
// Use a single JOIN query
var rights = await _context.RoleRights
    .Include(rr => rr.AppObject)
    .Where(rr => userRoleIds.Contains(rr.RoleId) && rr.AppRole.AppId == appId)
    .ToListAsync();
```

### 2.2. Repository Pattern Not Adding Value
**Location:** `SpecificRepositories.cs`
**Problem:** All specific repositories are empty wrappers inheriting from `BaseRepository` with no custom methods.
**Recommendation:**
- Either add meaningful repository-specific methods
- Or use EF Core `DbContext` directly with specification pattern
- Consider removing the abstraction if it provides no benefit

### 2.3. God Class - AuthService
**Location:** `AuthService.cs` (611 lines)
**Problem:** Single class handles authentication, authorization, token refresh, app switching, and rights merging.
**Recommendation:**
- Split into: `AuthenticationService`, `AuthorizationService`, `TokenService`, `RightsService`
- Follow Single Responsibility Principle

### 2.4. Duplicate Code - GetMergedRights
**Location:** `AuthService.cs:131-166` and `AuthService.cs:565-609`
**Problem:** Same logic duplicated in `AuthenticateAsync` and `GetMergedRights` method.
**Recommendation:**
- Extract to shared private method (already done partially)
- Ensure `AuthenticateAsync` uses `GetMergedRights`

### 2.5. Dead Code - UserManager
**Location:** `UserManager.cs`
**Problem:** All methods throw `NotImplementedException`.
**Recommendation:**
- Remove if not needed
- Or implement if part of future roadmap

### 2.6. Services Folder Empty
**Location:** `AppAuthService/Services/`
**Problem:** Empty directory suggests incomplete refactoring.
**Recommendation:**
- Remove if not needed
- Or move API-level services here (e.g., token validation middleware helpers)

---

## 3. DATABASE & DATA ACCESS

### 3.1. No Database Indexes Defined
**Problem:** No indexes on frequently queried columns:
- `AppUser.UserName`
- `Application.Number`
- `AppObject.Number`
- `RefTokenTracking.TokenHash`
- `UserRole.UserId`, `UserRole.AppRoleId`

**Recommendation:**
- Add indexes in `OnModelCreating` or via migrations
- Create composite indexes for common query patterns

### 3.2. Soft Delete Not Enforced
**Problem:** `IsDeleted` field exists but `BaseRepository` doesn't filter deleted records.
**Recommendation:**
- Implement global query filter in EF Core:
```csharp
modelBuilder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
```

### 3.3. No Database Migrations
**Location:** `Program.cs:54`
```csharp
context.Database.EnsureCreated();
```
**Problem:** `EnsureCreated()` doesn't support schema updates, only initial creation.
**Recommendation:**
- Use EF Core Migrations: `context.Database.Migrate()`
- Create migration scripts for production deployments

### 3.4. No Transaction Management
**Problem:** Operations that modify multiple entities (e.g., token refresh + user rights lookup) lack transaction boundaries.
**Recommendation:**
- Wrap multi-entity operations in `IDbContextTransaction`

---

## 4. API DESIGN

### 4.1. Inconsistent HTTP Status Codes
**Location:** Various controllers
**Problem:**
- Failed login returns `401 Unauthorized` (should be `400 Bad Request` for wrong credentials)
- All errors return `BOProcessResult` wrapped in various status codes

**Recommendation:**
- Use standard HTTP status codes consistently
- `400` for validation errors
- `401` for missing/invalid authentication
- `403` for authorization failures
- `404` for resource not found

### 4.2. No API Versioning
**Problem:** No versioning strategy for API endpoints.
**Recommendation:**
- Implement URL versioning: `/api/v1/auth/login`
- Or header-based versioning

### 4.3. No Rate Limiting
**Problem:** No rate limiting on authentication endpoints.
**Recommendation:**
- Add ASP.NET Core Rate Limiting middleware
- Configure stricter limits on `/api/auth/login`

### 4.4. Missing Input Validation Attributes
**Problem:** Request models lack data annotation validation attributes.
**Recommendation:**
- Add `[Required]`, `[StringLength]`, `[EmailAddress]` attributes to request DTOs
- Implement custom validation attributes for domain rules

### 4.5. No Pagination for List Endpoints
**Location:** `CompanyController.cs:44-48`, `UserController.cs:74-79`
```csharp
var companies = await _companyRepository.GetAllAsync();
```
**Problem:** Returns all records without pagination.
**Recommendation:**
- Implement pagination with `skip`/`take` parameters
- Return `PagedResult<T>` with total count

---

## 5. CODE QUALITY

### 5.1. Missing Async/Await Best Practices
**Location:** `BlacklistService.cs:32-49`
```csharp
#pragma warning disable CS1998
public async Task<bool> IsIpBlacklistedAsync(string ip)
```
**Problem:** Methods marked async but have no await - synchronous operations wrapped in Task.
**Recommendation:**
- Remove async/await for synchronous file I/O
- Or use `File.ReadAllTextAsync()` for true async

### 5.2. Magic Strings
**Location:** Multiple files
**Problem:** Hardcoded strings for claim types, action names, etc.
```csharp
var userName = principal.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
```
**Recommendation:**
- Define constants class: `JwtClaimTypes.UserName`
- Use enums for action types

### 5.3. No Logging Framework
**Problem:** Only `Console.WriteLine` used for security alerts (`SecurityAuditService.cs:23`).
**Recommendation:**
- Implement Serilog or NLog
- Configure structured logging with log levels
- Add correlation IDs for request tracing

### 5.4. Inconsistent Naming Conventions
**Problem:**
- `jwtInfo` (camelCase) vs `UserInfo` (PascalCase) in `LoginResponse`
- Mixed Vietnamese/English comments

**Recommendation:**
- Follow C# conventions: PascalCase for public properties
- Standardize on English for code comments

### 5.5. Missing XML Documentation
**Problem:** Most public methods lack XML documentation.
**Recommendation:**
- Add `<summary>`, `<param>`, `<returns>` tags
- Enable XML documentation file generation in `.csproj`

---

## 6. TESTING

### 6.1. Insufficient Test Coverage
**Location:** `AppAuth.Tests/`
**Problem:**
- Only 3 test cases in `AuthServiceTests.cs`
- No integration tests
- `UnitTest1.cs` appears to be template file
- No tests for controllers, middleware, or JWT utilities

**Recommendation:**
- Aim for minimum 70% code coverage
- Add tests for:
  - All controller endpoints
  - JWT generation and validation
  - Authorization filter
  - Security middleware
  - Edge cases (null inputs, empty collections)

### 6.2. No Mock Database Testing
**Problem:** Tests mock repositories but don't test against real database scenarios.
**Recommendation:**
- Add integration tests with SQLite in-memory or Testcontainers
- Test actual EF Core queries

### 6.3. No Load/Performance Testing
**Problem:** No tests for concurrent authentication requests.
**Recommendation:**
- Add load tests for login endpoint
- Test token refresh under concurrent access

---

## 7. DEPLOYMENT & OPERATIONS

### 7.1. No Health Checks
**Problem:** No health check endpoints for monitoring.
**Recommendation:**
- Add ASP.NET Core Health Checks
- Include database connectivity check
- Expose `/health` endpoint for load balancers

### 7.2. No Application Insights / Monitoring
**Problem:** No telemetry or monitoring configured.
**Recommendation:**
- Add Application Insights or OpenTelemetry
- Track request rates, error rates, latency
- Monitor authentication failures

### 7.3. No CI/CD Pipeline
**Problem:** No GitHub Actions or Azure DevOps pipeline defined.
**Recommendation:**
- Create pipeline for:
  - Build and test on PR
  - Code analysis (SonarQube)
  - Container build and push
  - Deployment to staging/production

### 7.4. No Docker Support
**Problem:** No Dockerfile or docker-compose configuration.
**Recommendation:**
- Add multi-stage Dockerfile
- Create docker-compose for local development (API + SQL Server)

---

## 8. PERFORMANCE

### 8.1. No Caching
**Problem:** Rights and user data fetched from database on every request.
**Recommendation:**
- Cache user rights in Redis or IMemoryCache
- Implement cache invalidation on role changes
- Cache JWT validation results

### 8.2. Inefficient Token Validation
**Location:** `MSAAuthorizeFilter.cs:58`
**Problem:** Database query on every authorized request.
**Recommendation:**
- Embed rights in JWT claims (if size permits)
- Or use distributed cache for rights lookup

### 8.3. No Connection Pooling Configuration
**Problem:** Default EF Core connection pooling settings.
**Recommendation:**
- Configure connection string with `Max Pool Size` and `Min Pool Size`
- Monitor connection usage under load

---

## PRIORITY SUMMARY

| Priority | Category | Issues Count |
|----------|----------|--------------|
| **P0 - Critical** | Security (hardcoded secrets, CORS, password hashing) | 7 |
| **P1 - High** | Architecture (N+1 queries, God class, transactions) | 6 |
| **P1 - High** | Database (indexes, migrations, soft delete) | 4 |
| **P2 - Medium** | API Design (status codes, rate limiting, pagination) | 5 |
| **P2 - Medium** | Code Quality (logging, naming, documentation) | 5 |
| **P2 - Medium** | Testing (coverage, integration tests) | 3 |
| **P3 - Low** | Deployment (health checks, CI/CD, Docker) | 4 |
| **P3 - Low** | Performance (caching, connection pooling) | 3 |

---

## RECOMMENDED ACTION PLAN

### Phase 1: Security Fixes (Week 1-2)
1. Remove hardcoded credentials from source control
2. Fix CORS policy
3. Replace custom password hashing with PBKDF2/BCrypt
4. Implement proper error handling (no exception leakage)

### Phase 2: Architecture Refactoring (Week 3-4)
1. Fix N+1 query problem with JOIN queries
2. Split `AuthService` into focused services
3. Add database indexes
4. Implement EF Core migrations

### Phase 3: API & Code Quality (Week 5-6)
1. Standardize HTTP status codes
2. Add rate limiting
3. Implement Serilog logging
4. Add input validation attributes
5. Implement pagination

### Phase 4: Testing & DevOps (Week 7-8)
1. Increase test coverage to 70%+
2. Add health checks
3. Create Docker configuration
4. Set up CI/CD pipeline
5. Implement caching strategy
