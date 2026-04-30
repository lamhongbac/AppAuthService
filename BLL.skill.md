# Business Logic Layer (BLL) Standards

Quy chuẩn xử lý nghiệp vụ, đảm bảo logic tập trung, nhất quán và trả về kết quả chuẩn hóa.

## 1. Cấu trúc Solution
- **{Project}.BLL**: Backend nghiệp vụ chính.
    - Kiểu dữ liệu: **BO (Business Objects)** nằm trong `AuthService.Shared` (Phục vụ tính toán và xử lý logic nghiệp vụ).

## 2. Chuẩn hóa kết quả xử lý (BOProcessResult)
Mọi hàm BLL khi giao tiếp với API hoặc GUI phải bọc kết quả trong đối tượng `BOProcessResult`.

### A. Cấu trúc `BOProcessResult`
- `OK` (bool): Trạng thái thành công hoặc thất bại.
- `Content` (object?): Kết quả  trả về (cần ghi chú kiểu dữ liệu trong XML Comments).
- `ErrorNumber`: Mã lỗi logic (Enum). Theo mẫu `{Domain}_ErrorCode`.
- `Message` (string): Thông báo dự phòng (fallback).
- 'NumOfRow' (int): số dòng thực hiện trên CSDL, giống lệnh update SQL

### B. Quy tắc thực hiện
1. **Chỉ Wrap khi trả về Client**: Các hàm nội bộ không cần wrap.
2. **Hỗ trợ Đa ngôn ngữ**: Client dựa vào `ErrorCode` để tra cứu nội dung dịch.
3. **Định nghĩa ErrorCode**: Tạo enum riêng cho từng domain nghiệp vụ.
   ```csharp
   public enum Media_ErrorCode {
       [Description("Thành công")] Success = 0,
       [Description("Lỗi nghiệp vụ")] BusinessError = 101
   }
   ```

## 3. Quy tắc Nghiệp vụ
- **Tách biệt**: Tầng BLL chỉ làm việc với **BO**.
- **Ánh xạ**: Thực hiện ánh xạ `BO <-> Model` (DAL) tại đây.
- **Dependency Injection**: Đăng ký các Service/Business qua `AddScoped`.
