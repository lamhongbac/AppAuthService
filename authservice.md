# AppAuthService AI Agent Integration Specification

This document serves as an instruction and reference specification for AI Coding Agents to integrate external application clients (e.g., Inventory, POS, sales) with the **AppAuthService (AS)**.

---

## 1. System Role & Context
`AppAuthService` is a centralized authentication and authorization service that provides:
1. Central credential verification.
2. Central authorization management down to individual `AppObject` permissions.
3. Single Sign-On (SSO) support to switch between client apps.
4. Centralized security auditing, IP/user blacklisting, and refresh token rotation.

Every client app must communicate with this service for session and permission management.

---

## 2. Configuration Requirements
To connect a client application to the `AppAuthService`, the following configuration values are required in the client's settings (e.g., `appsettings.json` or environment variables):

```json
{
  "AuthService": {
    "BaseUrl": "http://<auth-service-host>:<port>",
    "RegKey": "<company-reg-key>",
    "AppNumber": "<client-app-number>"
  }
}
```

- **`BaseUrl`**: The URL where the `AppAuthService` API is hosted (e.g., `http://localhost:5000`).
- **`RegKey`**: The unique Company Registration Key. This **must** be sent in the `x-api-key` header of every HTTP request to the service.
- **`AppNumber`**: The unique identifier of the client application (e.g., `"FnBInv"`).

---

## 3. Communication Protocols

### 3.1 Base Headers
Every request to the `AppAuthService` must include the following headers:
- `x-api-key`: `{RegKey}`
- `Content-Type`: `application/json`

For endpoints requiring authentication (such as switching applications):
- `Authorization`: `Bearer {accessToken}`

### 3.2 Standard Response Wrapper (`BOProcessResult`)
All API responses from the service are wrapped in a standard `BOProcessResult` structure. When generating model definitions or client-side JSON parsers, use this layout:

```json
{
  "ok": true,
  "message": "Success",
  "errorCode": 0,
  "object": { ... }
}
```

- **`ok`** (`bool`): `true` if the operation was successful; `false` otherwise.
- **`message`** (`string`): Description of the outcome, suitable for localization lookups.
- **`errorCode`** (`int`): Error code indicator (0 representing success).
- **`object`** (`T`): The specific payload of the response (e.g., `LoginResponse`).

#### Error Code Dictionary
| ErrorCode | Name | Description |
|---|---|---|
| `0` | Success | Operation succeeded |
| `1001` | InvalidToken | API Key or JWT is missing or invalid |
| `1002` | InvalidCredentials | Incorrect username or password |
| `1003` | UserLocked | User is locked or in the blacklist |
| `1004` | AppNotFound | Target `AppNumber` is not registered |
| `1005` | ValidationError | Payload fields failed validation checks |
| `1006` | DuplicateEntry | Duplicate identifier detected |
| `9999` | SystemError | Unhandled system exception |

---

## 4. Model Definitions for Code Generation

Use these class templates to generate matching types in the client application:

### C# / .NET Models
```csharp
public class BOProcessResult<T>
{
    public bool Ok { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ErrorCode { get; set; }
    public T? Object { get; set; }
}

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string AppNumber { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string AppNumber { get; set; } = string.Empty;
}

public class SwitchAppRequest
{
    public string AppNumber { get; set; } = string.Empty;
}

public class LoginResponse
{
    public UserData UserInfo { get; set; } = null!;
    public JwtData JwtInfo { get; set; } = null!;
}

public class UserData
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string CurrentAppNumber { get; set; } = string.Empty;
    public List<ObjectRight> MergedRights { get; set; } = new();
}

public class JwtData
{
    public string Jwt { get; set; } = string.Empty; // Access Token
    public string Rft { get; set; } = string.Empty; // Refresh Token
}

public class ObjectRight
{
    public int Id { get; set; }
    public int ObjectId { get; set; }
    public string ObjectNumber { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public bool CanList { get; set; }
    public bool CanRead { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}
```

### TypeScript / JavaScript Types
```typescript
export interface BOProcessResult<T> {
  ok: boolean;
  message: string;
  errorCode: number;
  object: T | null;
}

export interface LoginRequest {
  userName: string;
  password: string;
  appNumber: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
  appNumber: string;
}

export interface SwitchAppRequest {
  appNumber: string;
}

export interface LoginResponse {
  userInfo: UserData;
  jwtInfo: JwtData;
}

export interface UserData {
  id: number;
  username: string;
  fullName: string;
  email: string | null;
  mobile: string | null;
  currentAppNumber: string;
  mergedRights: ObjectRight[];
}

export interface JwtData {
  jwt: string; // Access Token
  rft: string; // Refresh Token
}

export interface ObjectRight {
  id: number;
  objectId: number;
  objectNumber: string;
  objectName: string;
  canList: boolean;
  canRead: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}
```

---

## 5. API Endpoints

### 5.1 User Login
Authenticates a user and retrieves application tokens and merged permissions.

- **URL**: `POST /api/auth/login`
- **Headers**:
  - `x-api-key`: `{RegKey}`
- **Payload**: `LoginRequest`
- **Output**: `BOProcessResult<LoginResponse>`

### 5.2 Token Refresh
Rotates expired access and refresh tokens.

- **URL**: `POST /api/auth/refresh-token`
- **Headers**:
  - `x-api-key`: `{RegKey}`
- **Payload**: `RefreshTokenRequest`
- **Output**: `BOProcessResult<LoginResponse>`

### 5.3 Switch App (SSO)
Transitions user credentials to another target application without prompts.

- **URL**: `POST /api/auth/switch-app`
- **Headers**:
  - `x-api-key`: `{RegKey}`
  - `Authorization`: `Bearer {currentAccessToken}`
- **Payload**: `SwitchAppRequest`
- **Output**: `BOProcessResult<LoginResponse>`

---

## 6. Implementation Workflow for Client Agents

When generating integration logic, implement the following patterns:

### 6.1 Safe Storage of Tokens
Upon login or token refresh, store:
- `jwt` (Access Token)
- `rft` (Refresh Token)
- `mergedRights` (Local Permissions Array)

Store these securely according to client type (e.g., HTTP-only cookies, localStorage, or native secure keychain).

### 6.2 The HTTP Interceptor / Token Rotation Loop
To handle JWT expiration gracefully, the client must implement an HTTP Interceptor (or DelegatingHandler in .NET):

1. **Intercept Requests**: Inject `Authorization: Bearer {jwt}` into outgoing API calls to client backend resource services.
2. **Handle `401 Unauthorized`**: If a resource service returns `401`, intercept the response.
3. **Execute Rotation**:
   - Call `/api/auth/refresh-token` with the expired access token, the current refresh token, and the client application's `AppNumber`.
   - Update client session storage with the new `jwt` and `rft`.
   - Retry the failed original request with the new access token.
4. **Token Reuse Protection**: If the refresh token endpoint returns a validation error or security code:
   - **Immediately invalidate client session** (logout user).
   - Clear storage and redirect to login page.

### 6.3 Local Permission Checks (No Network Polling)
Client views and components must check permissions locally. Do **not** invoke the `AppAuthService` API for each permission check.

Generate a helper utility equivalent to the following:

#### C# Client Helper
```csharp
public class PermissionService
{
    public bool HasRight(List<ObjectRight> mergedRights, string objectNumber, string action)
    {
        var right = mergedRights.FirstOrDefault(r => r.ObjectNumber == objectNumber);
        if (right == null) return false;

        return action.ToLower() switch
        {
            "list"   => right.CanList,
            "read"   => right.CanRead,
            "create" => right.CanCreate,
            "update" => right.CanUpdate,
            "delete" => right.CanDelete,
            _        => false
        };
    }
}
```

#### TypeScript Client Helper
```typescript
export function hasRight(mergedRights: ObjectRight[], objectNumber: string, action: 'list' | 'read' | 'create' | 'update' | 'delete'): boolean {
  const right = mergedRights.find(r => r.objectNumber === objectNumber);
  if (!right) return false;

  switch (action) {
    case 'list': return right.canList;
    case 'read': return right.canRead;
    case 'create': return right.canCreate;
    case 'update': return right.canUpdate;
    case 'delete': return right.canDelete;
    default: return false;
  }
}
```

---

## 7. Sample Integration Templates

### .NET Core HttpClient DelegatingHandler Example
Generate this handler in C# clients to automate token injection and rotation:

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ISessionStorage _storage;
    private readonly HttpClient _authHttp; // Configured with AppAuthService BaseUrl and x-api-key
    private readonly string _appNumber;

    public AuthTokenHandler(ISessionStorage storage, HttpClient authHttp, string appNumber)
    {
        _storage = storage;
        _authHttp = authHttp;
        _appNumber = appNumber;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _storage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshToken = await _storage.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken))
            {
                var body = new RefreshTokenRequest 
                { 
                    AccessToken = token, 
                    RefreshToken = refreshToken, 
                    AppNumber = _appNumber 
                };

                var refreshResponse = await _authHttp.PostAsJsonAsync("/api/auth/refresh-token", body, cancellationToken);
                var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<BOProcessResult<LoginResponse>>(cancellationToken: cancellationToken);

                if (refreshResult != null && refreshResult.Ok && refreshResult.Object != null)
                {
                    var newTokens = refreshResult.Object.JwtInfo;
                    await _storage.SaveTokensAsync(newTokens.Jwt, newTokens.Rft);

                    // Retry original request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Jwt);
                    return await base.SendAsync(request, cancellationToken);
                }
            }

            // Force logout if rotation fails
            await _storage.ClearSessionAsync();
        }

        return response;
    }
}
```
