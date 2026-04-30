# Utilities & Helper Standards

Quy chuẩn cho các thư viện dùng chung, hàm tiện ích và xử lý hệ thống bổ trợ trong dự án.

### AuthService.Shared
- Chứa các kiểu dữ liệu dùng chung (BO, Request/Response, DTO) giữa API, BLL và các Frontend client của hệ sinh thái AuthService.

### MSA.Shared
- Chứa các kiểu dữ liệu nền tảng (Base classes, common Enums, core types) dùng chung cho tất cả các ứng dụng trong doanh nghiệp.

### AppAuth.Utils
- Dự án chuyên biệt xử lý logic JWT (Xác thực, tạo token, quản lý Refresh Token).

### MSA.Utils
- Thư viện chứa các hàm tiện ích (String, DateTime, Security, Mapper) dùng chung cho toàn bộ các ứng dụng.

## 2. Chiến lược Ánh xạ dữ liệu (Mapping Strategy)
Để đảm bảo tính tách biệt, dự án sử dụng chiến lược ánh xạ dữ liệu nghiêm ngặt giữa các tầng.

- **Công cụ**: Sử dụng **Mapster** (được cấu hình trong `MSA.Utils`) để thực hiện ánh xạ tự động.
- **Luồng ánh xạ**:
    - `ViewModel` <-> `BO` (Thực hiện tại **GUI/API**).
    - `BO` <-> `Model` (Thực hiện tại **BLL**).
- **Nguyên tắc**: Tuyệt đối không dùng chung kiểu dữ liệu giữa các tầng khác nhau.

## 3. Xử lý File & IO
- **Stream**: Ưu tiên xử lý qua Stream thay vì byte array để tối ưu RAM.
- **Cấu trúc lưu trữ**: Phân cấp rõ ràng: `Root/Type/Entity/UniqueCode/File`.
- **Naming**: `{UniqueCode}_{Timestamp}` và luôn chuyển về **lowercase**.

## 4. Các Hàm Tiện ích (Helpers)
- **Dự án**: **MSA.Utils** (Chứa StringUtils, DateTimeUtils, EnumUtils, MapperServices).
- **SecurityUtils**: Các hàm hash, mã hóa (nằm trong MSA.Utils).

## 5. Cấu hình & Constants
- Lưu trữ các chuỗi hằng số (Constants), Key cấu hình dùng chung để tránh hardcode.

## 6. Phân loại Kiểu dữ liệu Shared
- **MSA.Shared**: Các kiểu dữ liệu cơ bản (C# standard), BaseObject, DataTypes chung cho mọi dự án.
- **AuthService.Shared**: 
    - `RequestResponse`: DTO cho API.
    - `BusinessObjects`: Các đối tượng xử lý nghiệp vụ (BO).
    - `BOProcessResult`: Kết quả xử lý chuẩn hóa dùng chung cho API/BLL.
