# AppAuthService — Hướng Dẫn Sử Dụng

> **Phiên bản:** 1.0 | **Công nghệ:** .NET 8 Web API | **Database:** MS-SQL Server

---

## Mục Lục

1. [Tổng quan](#1-tổng-quan)
2. [Quy ước chung](#2-quy-ước-chung)
3. [Luồng thiết lập ban đầu (Setup Flow)](#3-luồng-thiết-lập-ban-đầu-setup-flow)
4. [API Reference](#4-api-reference)
   - [4.1 Company](#41-company)
   - [4.2 Application](#42-application)
   - [4.3 Role](#43-role)
   - [4.4 User](#44-user)
   - [4.5 Auth — Xác thực & Token](#45-auth--xác-thực--token)
   - [4.6 Blacklist](#46-blacklist)
5. [Luồng nghiệp vụ chi tiết](#5-luồng-nghiệp-vụ-chi-tiết)
   - [5.1 Đăng nhập (Login)](#51-đăng-nhập-login)
   - [5.2 Kiểm tra quyền tại ứng dụng client](#52-kiểm-tra-quyền-tại-ứng-dụng-client)
   - [5.3 Refresh Token](#53-refresh-token)
   - [5.4 Chuyển ứng dụng (Switch App / SSO)](#54-chuyển-ứng-dụng-switch-app--sso)
6. [Cấu trúc Response chuẩn](#6-cấu-trúc-response-chuẩn)
7. [Bảng Error Code](#7-bảng-error-code)
8. [Hướng dẫn tích hợp cho ứng dụng mới](#8-hướng-dẫn-tích-hợp-cho-ứng-dụng-mới)

---

## 1. Tổng Quan

**AppAuthService (AS)** là một microservice độc lập, đóng vai trò là **trung tâm xác thực và phân quyền** cho toàn bộ hệ sinh thái ứng dụng của doanh nghiệp.

```
┌─────────────────────────────────────────────────────────┐
│                    Hệ sinh thái ứng dụng                │
│                                                         │
│  ┌──────────┐   ┌──────────┐   ┌──────────────────┐    │
│  │ FnBInv   │   │   POS    │   │  App khác ...    │    │
│  │  Client  │   │  Client  │   │     Client       │    │
│  └────┬─────┘   └────┬─────┘   └────────┬─────────┘    │
│       │              │                  │               │
│       └──────────────┴──────────────────┘               │
│                           │                             │
│                  ┌────────▼────────┐                    │
│                  │ AppAuthService  │                    │
│                  │   (API :5000)   │                    │
│                  └────────┬────────┘                    │
│                           │                             │
│                  ┌────────▼────────┐                    │
│                  │  SQL Server DB  │                    │
│                  │ AppAuthService  │                    │
│                  └─────────────────┘                    │
└─────────────────────────────────────────────────────────┘
```

**Mục tiêu cốt lõi:**
- Tách biệt logic xác thực ra khỏi từng ứng dụng
- Hỗ trợ **SSO** — đăng nhập một lần, dùng trên nhiều app
- Quản lý phân quyền **chi tiết đến từng object** (View/Process/Data)
- Bảo mật nâng cao: Refresh Token Rotation, IP/User Blacklist, Security Audit

---

## 2. Quy Ước Chung

### 2.1 Authentication Header

Mọi request đến AS (trừ `POST /api/company/register`) đều **bắt buộc** có header:

```http
x-api-key: {RegKey_của_công_ty}
```

> `RegKey` được cấp phát khi đăng ký công ty (Company Registration). Admin giữ và phân phối key này cho các ứng dụng client.

Với các endpoint yêu cầu JWT (switch-app):

```http
x-api-key: {RegKey}
Authorization: Bearer {access_token}
```

### 2.2 Base URL

```
http(s)://{host}:{port}/api
```

Ví dụ môi trường dev: `http://localhost:5000/api`

### 2.3 Content-Type

```http
Content-Type: application/json
```

---

## 3. Luồng Thiết Lập Ban Đầu (Setup Flow)

Khi tích hợp một ứng dụng mới vào hệ thống, cần thực hiện **theo đúng thứ tự**:

```
Bước 1: Đăng ký Công ty          → Nhận RegKey (x-api-key)
    │
    ▼
Bước 2: Đăng ký Ứng dụng         → Dùng RegKey; Nhận AppKey (JWT Key)
    │
    ▼
Bước 3: Khai báo App Objects      → Định nghĩa các màn hình/chức năng cần phân quyền
    │
    ▼
Bước 4: Tạo Roles + phân quyền   → Gắn quyền (CRUD) lên từng AppObject cho mỗi Role
    │
    ▼
Bước 5: Tạo Users                 → Tạo tài khoản người dùng
    │
    ▼
Bước 6: Gán Roles cho Users       → Ánh xạ User ↔ Role trong từng App
    │
    ▼
✅ Sẵn sàng: Client có thể Login và hoạt động
```

---

## 4. API Reference

### 4.1 Company

#### `POST /api/company/register`

Đăng ký công ty mới. Không cần `x-api-key`.

**Request Body:**
```json
{
  "name": "Morning Star AppTech",
  "number": "MSA",
  "description": "Công ty phát triển phần mềm"
}
```

**Response thành công (`200 OK`):**
```json
{
  "ok": true,
  "message": "Company registered successfully",
  "errorCode": 0,
  "object": {
    "id": 1,
    "name": "Morning Star AppTech",
    "number": "MSA",
    "regKey": "a1b2c3d4e5f6..."
  }
}
```

> ⚠️ **Lưu `regKey` ngay lập tức.** Đây chính là `x-api-key` dùng cho toàn bộ các request tiếp theo.

---

#### `GET /api/company`

Lấy danh sách công ty.

**Headers:** `x-api-key: {RegKey}`

---

### 4.2 Application

#### `POST /api/application/register`

Đăng ký ứng dụng mới vào công ty.

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "name": "FnB Inventory",
  "number": "FnBInv",
  "description": "Quản lý hàng hóa chuỗi nhà hàng",
  "jwtConfig": {
    "issuer": "FnBInv",
    "audience": "FnBInv_Users",
    "expire": 60,
    "refExpire": 43200
  }
}
```

> **`jwtConfig` là optional.** Nếu không truyền hoặc để null, hệ thống sẽ dùng JwtConfig mặc định cấu hình trong `appsettings.json`.
>
> Nếu muốn **mỗi app có JWT key riêng biệt** (bảo mật cao hơn), hãy truyền `jwtConfig`.

**Response thành công:**
```json
{
  "ok": true,
  "object": {
    "id": 1,
    "number": "FnBInv",
    "appKey": "jwt-secret-key-được-sinh-tự-động"
  }
}
```

---

#### `POST /api/application/objects`

Khai báo danh sách App Objects (màn hình/chức năng cần phân quyền).

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "appNumber": "FnBInv",
  "objects": [
    {
      "number": "STKM",
      "name": "StockMaster",
      "description": "Quản trị danh mục stock",
      "objectType": "View"
    },
    {
      "number": "STK_IN",
      "name": "StockIn",
      "description": "Phiếu Nhập kho",
      "objectType": "Data"
    },
    {
      "number": "PR",
      "name": "PurchaseRequest",
      "description": "Phiếu yêu cầu mua hàng",
      "objectType": "Process"
    }
  ]
}
```

**`objectType` hợp lệ:** `View` | `Process` | `Data`

---

#### `GET /api/application`

Lấy danh sách ứng dụng.

#### `GET /api/application/{appId}/objects`

Lấy danh sách AppObjects của một ứng dụng.

---

### 4.3 Role

#### `POST /api/role/create`

Tạo Role mới và gán quyền lên các AppObjects.

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "name": "StockAdmin",
  "number": "STK_ADMIN",
  "description": "Quản trị nhập master data",
  "appNumber": "FnBInv",
  "rights": [
    {
      "appObjectNumber": "STKM",
      "canList": true,
      "canRead": true,
      "canCreate": true,
      "canUpdate": true,
      "canDelete": true
    },
    {
      "appObjectNumber": "WHM",
      "canList": true,
      "canRead": true,
      "canCreate": true,
      "canUpdate": true,
      "canDelete": false
    }
  ]
}
```

> **Gộp quyền (Merge):** Nếu một user có nhiều role, quyền sẽ được **OR** lại. Ví dụ: Role A có `CanCreate=false`, Role B có `CanCreate=true` → User có `CanCreate=true`.

---

#### `GET /api/role/app/{appId}`

Lấy danh sách role theo ứng dụng.

---

### 4.4 User

#### `POST /api/user/create`

Tạo tài khoản người dùng mới.

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "userName": "bac",
  "fullName": "Lâm Hồng Bắc",
  "password": "Pass123!",
  "email": "bac@msa.vn",
  "mobile": "0901234567",
  "cardNum": null
}
```

> **Lưu ý về định danh:** User có thể được định danh bằng 4 cách: `UserName`, `Email`, `Mobile`, `CardNum`. Tất cả đều có thể dùng để đăng nhập.

---

#### `POST /api/user/assign-roles`

Gán vai trò (roles) cho user trong một ứng dụng cụ thể.

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "userName": "bac",
  "appNumber": "FnBInv",
  "roleNumbers": ["STK_ADMIN"]
}
```

> Có thể gán **nhiều roles** cùng lúc. Quyền sẽ được merge khi user đăng nhập.

---

#### `GET /api/user`

Lấy danh sách người dùng.

---

### 4.5 Auth — Xác thực & Token

#### `POST /api/auth/login`

Xác thực người dùng và cấp phát JWT.

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "userName": "bac",
  "password": "Pass123!",
  "appNumber": "FnBInv"
}
```

**Response thành công (`200 OK`):**
```json
{
  "ok": true,
  "message": "Login successful",
  "errorCode": 0,
  "object": {
    "userInfo": {
      "id": 1,
      "username": "bac",
      "fullName": "Lâm Hồng Bắc",
      "email": "bac@msa.vn",
      "mobile": "0901234567",
      "currentAppNumber": "FnBInv",
      "mergedRights": [
        {
          "objectId": 1,
          "objectNumber": "STKM",
          "objectName": "StockMaster",
          "canList": true,
          "canRead": true,
          "canCreate": true,
          "canUpdate": true,
          "canDelete": true
        }
      ]
    },
    "jwtInfo": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": "d4f8a2b1-...",
      "expiredAt": "2026-05-12T16:00:00Z"
    }
  }
}
```

**Cách client sử dụng sau login:**
1. Lưu `accessToken` và `refreshToken` vào secure storage (localStorage / SecureStore / Keychain)
2. Lưu `mergedRights` vào app state để **kiểm tra quyền hiển thị UI** (ẩn/hiện nút, menu)
3. Gắn `accessToken` vào header `Authorization: Bearer {token}` cho mọi request đến app-service

---

#### `POST /api/auth/refresh-token`

Cấp phát cặp token mới khi `accessToken` hết hạn.

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...(token cũ, có thể đã expired)",
  "refreshToken": "d4f8a2b1-...",
  "appNumber": "FnBInv"
}
```

**Response thành công:** Cấu trúc tương tự `/auth/login` — trả về `LoginResponse` mới với cặp token mới.

> ⚠️ **Cơ chế Refresh Token Rotation:**
> - Mỗi lần refresh, `refreshToken` cũ sẽ bị đánh dấu `IsUsed=true` và **không thể dùng lại**
> - Nếu kẻ xấu cố dùng lại refreshToken cũ → hệ thống phát hiện **Token Reuse Attack**, tự động thu hồi tất cả session của user và trả về lỗi bảo mật

---

#### `POST /api/auth/switch-app`

Chuyển sang ứng dụng khác **mà không cần đăng nhập lại** (SSO).

**Headers:**
```http
x-api-key: {RegKey}
Authorization: Bearer {access_token_hiện_tại}
```

**Request Body:**
```json
{
  "appNumber": "POS"
}
```

**Response thành công:** Trả về `LoginResponse` mới với `mergedRights` của app đích (`POS`), token mới được ký bằng JWT key của app `POS`.

---

### 4.6 Blacklist

#### `POST /api/blacklist/add`

Thêm IP hoặc username vào danh sách chặn.

**Headers:** `x-api-key: {RegKey}`

**Request Body:**
```json
{
  "type": "user",
  "value": "hacker_user",
  "reason": "Brute force detected"
}
```

`type` hợp lệ: `"user"` | `"ip"`

---

#### `DELETE /api/blacklist/remove?type={type}&value={value}`

Gỡ bỏ khỏi danh sách chặn.

**Headers:** `x-api-key: {RegKey}`

---

## 5. Luồng Nghiệp Vụ Chi Tiết

### 5.1 Đăng Nhập (Login)

```
Client                          AppAuthService                      Database
  │                                  │                                  │
  │──POST /auth/login ──────────────►│                                  │
  │  {userName, password, appNumber} │                                  │
  │                                  │──Validate x-api-key ────────────►│
  │                                  │◄─ Company info ─────────────────│
  │                                  │                                  │
  │                                  │──Check User Blacklist ──────────►│
  │                                  │                                  │
  │                                  │──Find User by UserName ─────────►│
  │                                  │◄─ User record ──────────────────│
  │                                  │                                  │
  │                                  │  Verify Password (Hash + PwdKey) │
  │                                  │                                  │
  │                                  │──Get Roles in App ──────────────►│
  │                                  │──Get RoleRights ────────────────►│
  │                                  │  Merge Rights (OR logic)         │
  │                                  │                                  │
  │                                  │  Generate JWT + RefreshToken     │
  │                                  │──Save RefreshToken ─────────────►│
  │                                  │──Write AuditLog ────────────────►│
  │                                  │                                  │
  │◄─ LoginResponse (200 OK) ───────│                                  │
  │   {UserInfo, JwtInfo}            │                                  │
```

**Bảo mật tích hợp trong login:**
- Sau mỗi lần sai password: `SecurityAuditService.TrackFailedAttempt()` tăng bộ đếm
- Khi vượt ngưỡng: tự động thêm user vào blacklist
- Login thành công: reset bộ đếm

---

### 5.2 Kiểm Tra Quyền Tại Ứng Dụng Client

Sau khi login, client nhận được `mergedRights`. **Không cần gọi AS để kiểm tra từng action** — client tự kiểm tra từ dữ liệu đã có:

```csharp
// C# Blazor / .NET Client Example
var stkm = loginResponse.UserInfo.MergedRights
    .FirstOrDefault(r => r.ObjectNumber == "STKM");

bool canCreateStock = stkm?.CanCreate ?? false;
bool canDeleteStock = stkm?.CanDelete ?? false;

// Dùng để ẩn/hiện UI
<button style="display: @(canCreateStock ? "block" : "none")">Tạo mới</button>
```

```javascript
// JavaScript / React / Vue Example
const rights = loginResponse.object.userInfo.mergedRights;
const stkm = rights.find(r => r.objectNumber === 'STKM');

const canCreate = stkm?.canCreate ?? false;
```

---

### 5.3 Refresh Token

Áp dụng pattern **Interceptor** tại tầng HTTP client của ứng dụng:

```
Client Request (với JWT đã expired)
        │
        ▼
  HTTP Response 401
        │
        ▼
  Interceptor bắt lỗi 401
        │
        ▼
  Gọi POST /auth/refresh-token
        │
   ┌────┴────────┐
   │ Thành công  │──► Lưu token mới → Retry request gốc → Tiếp tục
   └─────────────┘
   │ Thất bại    │──► Đăng xuất, chuyển về màn hình Login
   └─────────────┘
```

---

### 5.4 Chuyển Ứng Dụng (Switch App / SSO)

```
User đang dùng FnBInv (có access_token_FnBInv)
        │
        │ Muốn chuyển sang POS
        ▼
POST /auth/switch-app
  Header: x-api-key, Authorization: Bearer {access_token_FnBInv}
  Body: { "appNumber": "POS" }
        │
        ▼
AS xác thực token cũ → Lấy MergedRights của user trong POS
        │
        ▼
Sinh JWT mới ký bằng AppKey của POS
        │
        ▼
Client nhận LoginResponse mới: token POS + rights POS
        │
        ▼
Lưu đè token mới, cập nhật UI theo rights của POS
```

---

## 6. Cấu Trúc Response Chuẩn

Mọi API đều trả về `BOProcessResult`:

```json
{
  "ok": true | false,
  "message": "Mô tả kết quả (tiếng Anh, dùng làm key đa ngôn ngữ)",
  "errorCode": 0,
  "object": { ... }
}
```

| Field | Type | Mô tả |
|---|---|---|
| `ok` | `bool` | `true` = thành công, `false` = thất bại |
| `message` | `string` | Message kết quả — client dùng làm key để dịch đa ngôn ngữ |
| `errorCode` | `int` | Mã lỗi cụ thể (xem bảng bên dưới) |
| `object` | `any` | Payload dữ liệu trả về khi thành công |

---

## 7. Bảng Error Code

| ErrorCode | Tên | Ý nghĩa |
|---|---|---|
| `0` | Success | Thành công |
| `1001` | InvalidToken | API Key hoặc JWT không hợp lệ |
| `1002` | InvalidCredentials | Sai username hoặc password |
| `1003` | UserLocked | User bị khoá hoặc nằm trong blacklist |
| `1004` | AppNotFound | Ứng dụng (AppNumber) không tồn tại |
| `1005` | ValidationError | Dữ liệu request không hợp lệ |
| `1006` | DuplicateEntry | Dữ liệu bị trùng lặp (number, username...) |
| `9999` | SystemError | Lỗi hệ thống không xác định |

---

## 8. Hướng Dẫn Tích Hợp Cho Ứng Dụng Mới

### Bước 1 — Khai báo dữ liệu (One-time Setup)

Thực hiện các bước theo thứ tự [Mục 3](#3-luồng-thiết-lập-ban-đầu-setup-flow).

Có thể dùng **AuthServiceAdmin** (Blazor Admin UI) hoặc gọi API trực tiếp qua Postman/Swagger.

### Bước 2 — Lưu cấu hình phía client

```json
// appsettings.json của ứng dụng client
{
  "AuthService": {
    "BaseUrl": "http://your-auth-service-host:5000",
    "RegKey": "api-key-nhận-được-khi-đăng-ký-company",
    "AppNumber": "FnBInv"
  }
}
```

### Bước 3 — Implement AuthService Client

```csharp
// Pseudo-code cho .NET client
public class AuthServiceClient
{
    private readonly HttpClient _http;
    private readonly string _regKey;
    private readonly string _appNumber;

    // Gọi login
    public async Task<LoginResponse> LoginAsync(string userName, string password)
    {
        _http.DefaultRequestHeaders.Add("x-api-key", _regKey);
        
        var body = new { UserName = userName, Password = password, AppNumber = _appNumber };
        var response = await _http.PostAsJsonAsync("/api/auth/login", body);
        var result = await response.Content.ReadFromJsonAsync<BOProcessResult>();
        
        if (result.OK)
            return result.Object as LoginResponse;
        
        throw new AuthException(result.Message, result.ErrorCode);
    }

    // Gọi refresh token (trong Interceptor)
    public async Task<LoginResponse> RefreshAsync(string accessToken, string refreshToken)
    {
        var body = new { AccessToken = accessToken, RefreshToken = refreshToken, AppNumber = _appNumber };
        var response = await _http.PostAsJsonAsync("/api/auth/refresh-token", body);
        // ...
    }

    // Kiểm tra quyền từ MergedRights (không cần gọi API)
    public bool HasRight(List<ObjectRight> mergedRights, string objectNumber, string rightType)
    {
        var obj = mergedRights.FirstOrDefault(r => r.ObjectNumber == objectNumber);
        return rightType.ToLower() switch
        {
            "list"   => obj?.CanList ?? false,
            "read"   => obj?.CanRead ?? false,
            "create" => obj?.CanCreate ?? false,
            "update" => obj?.CanUpdate ?? false,
            "delete" => obj?.CanDelete ?? false,
            _ => false
        };
    }
}
```

### Bước 4 — Bảo vệ màn hình/endpoint phía client

```csharp
// Blazor Page Example
@if (HasRight("STKM", "create"))
{
    <button @onclick="OpenCreateDialog">+ Thêm mới</button>
}

@if (HasRight("STK_IN", "list"))
{
    <StockInList />
}
```

---

*Tài liệu này được tạo tự động từ source code. Cập nhật lần cuối: 2026-05-12.*
