# Data Access Layer (DAL) Standards

Quy chuẩn phát triển tầng truy cập dữ liệu, đảm bảo tính nhất quán, hiệu năng và khả năng bảo trì Database.

## 1. Cấu trúc Solution
- **{Project}.DAL**: Tầng truy cập dữ liệu trực tiếp.
    - Kiểu dữ liệu: **Models / Entities** (Phản ánh chính xác cấu trúc các bảng trong CSDL).

## 2. Chiến lược Data Access (DAL Strategy)

### A. Công nghệ Sử dụng
1. **Dapper (Hiệu năng cao / Query phức tạp)**:
    - Sử dụng khi cần kiểm soát SQL tuyệt đối.
    - Luôn sử dụng `IDbConnection` thông qua Dependency Injection (`AddScoped`).
    - Sử dụng Parameterized Query (`@Param`) để chống SQL Injection.
2. **Entity Framework Core (CRUD nhiều / Quản lý bảng)**:
    - **DB-First**: Ưu tiên sử dụng **EF Core Power Tools** để Reverse Engineer từ Database.
    - **Tối ưu**: Luôn sử dụng `.AsNoTracking()` cho các truy vấn chỉ đọc.

### B. Repository Pattern
- Luôn bọc logic truy cập dữ liệu trong lớp **Repository**.
- Tầng BLL không được phụ thuộc trực tiếp vào công nghệ DB (Dapper/EF).

## 3. Thiết kế Cơ sở dữ liệu (Database Design)

### A. Phân loại & Đặt tên (Naming & Schema)
Sử dụng **Schema** để phân loại bảng thay vì dùng tiền tố (prefix) trực tiếp vào tên bảng. Việc này giúp tổ chức dữ liệu chuyên nghiệp và sạch sẽ.

1. **Schema `mst` (Master Data)**: Các bảng danh mục, ít thay đổi.
    - Ví dụ: `mst.Store`, `mst.Product`, `mst.Customer`.
2. **Schema `trn` (Transactional Data)**: Các bảng chứa dữ liệu giao dịch, phát sinh theo thời gian.
    - Ví dụ: `trn.CardTransaction`, `trn.Invoice`, `trn.MediaLog`.
3. **Schema `sys` / `cfg`**: Các bảng hệ thống hoặc cấu hình.
    - Ví dụ: `sys.User`, `cfg.AppSetting`.

### B. Các trường Audit (Bắt buộc)
Mọi bảng đều phải có 4 trường dấu vết sau:
- `CreatedBy`: varchar(20), default 'system'.
- `CreatedOn`: DateTime, default GetDate().
- `ModifiedBy`: varchar(20), default 'system'.
- `ModifiedOn`: DateTime, default GetDate().

### C. Primary Key (Id)
- Kiểu `int` (Identity): Cho dữ liệu ít hoặc ổn định.
- Kiểu `Guid`: Cho dữ liệu lớn hoặc hệ thống phân tán.
### C. Các trường theo dõi status
- `IsDeleted`: Bit, default 0.
- `IsActive`: Bit, default 1. 
## 4. BaseEntity trong DAL
Các Model trong DAL phải kế thừa từ `BaseEntity` để thống nhất cấu trúc:
```csharp
public abstract class BaseEntity<TId>
{
    public TId Id { get; set; }
    public string CreatedBy { get; set; } = "system";
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string ModifiedBy { get; set; } = "system";
    public DateTime ModifiedOn { get; set; } = DateTime.Now;
}
```
