# Core Project Standards & Professionalism

Bộ quy tắc chung về tiêu chuẩn lập trình, quản lý mã nguồn và quy trình phát triển nhằm đảm bảo tính chuyên nghiệp và chất lượng dự án.

## 1. Tiêu chuẩn Coding (Coding Style)

### A. Quy chuẩn Đặt tên (Naming Conventions)
- **C# Standard**:
    - `PascalCase` cho Tên lớp, Interface (`I...`), Method, Public Property.
    - `camelCase` cho biến cục bộ, tham số hàm.
    - `_camelCase` (có gạch dưới) cho các private fields.
- **Súc tích & Ý nghĩa**: Tránh đặt tên biến kiểu `a`, `b`, `temp`. Tên phải nói lên mục đích (vd: `isUserAuthenticated` thay vì `check`).

### B. Vệ sinh Mã nguồn (Code Hygiene)
- **DRY (Don't Repeat Yourself)**: Nếu một đoạn code xuất hiện quá 2 lần, hãy tách nó thành hàm hoặc helper dùng chung.
- **No Commented Code**: Không commit code đã bị comment (rác). Nếu không dùng, hãy xóa nó đi (Git đã lưu lịch sử).
- **Hardcode Prevention**: Tuyệt đối không hardcode chuỗi kết nối, API Key hay đường dẫn vật lý. Tất cả phải nằm trong `appsettings.json` hoặc Environment Variables.

## 2. Bảo mật & Hiệu năng Hệ thống
- **Authentication**: Sử dụng `X-Api-Key` truyền qua Header thay vì Query String cho các giao tiếp hệ thống.
- **Static Files & CDN**:
  - Map thư mục vật lý ra Virtual Path (`/cdn`).
  - Thêm Header `X-Content-Type-Options: nosniff`.
  - Cấu hình Cache trình duyệt (`max-age: 31536000`) cho tài nguyên tĩnh.
- **Error Handling Philosophy**:
  - **Fail Fast**: Kiểm tra dữ liệu đầu vào (Validation) ngay lập tức.
  - **Graceful Error**: Trả về thông báo lỗi thân thiện cho client. Chỉ ghi log chi tiết lỗi hệ thống (StackTrace) vào server, không gửi cho client.

## 3. Quản lý Mã nguồn (Git Standards)
- **Conventional Commits**: Sử dụng tiền tố để phân loại commit:
    - `feat:` : Thêm tính năng mới.
    - `fix:` : Sửa lỗi.
    - `docs:` : Cập nhật tài liệu.
    - `refactor:` : Tối ưu code nhưng không thay đổi tính năng.
    - `perf:` : Tối ưu hiệu năng.
- **Atomic Commits**: Mỗi commit chỉ nên giải quyết một vấn đề duy nhất. Không dồn quá nhiều thay đổi vào một commit.

## 4. Quy trình Kiểm thử (Testing Strategy)
- **Unit Test**: Sử dụng **XUnit** để kiểm tra logic ánh xạ (Mapper) và các hàm xử lý logic lõi.
- **Integration Test**: Sử dụng **PowerShell Script** để giả lập các kịch bản thực tế gọi trực tiếp vào API.

## 5. Tài liệu hóa (Documentation)
- **SQL Scripts**: Luôn có tệp `.sql` đi kèm để khởi tạo schema database.
- **XML Comments chuyên sâu**: 
  - Sử dụng thẻ `<summary>` để tóm tắt chức năng.
  - Sử dụng thẻ `<remarks>` để bổ sung phần **Usage** (Cách dùng và Khi nào dùng).
  - Giải thích các cấu hình trong `appsettings.json`.
  - Giải thích các cơ chế tự động (ví dụ: tự dọn dẹp thư mục).

## 6. Tư duy Sản phẩm (Product Mindset)
- **Sẵn sàng cho CI/CD**: Mã nguồn phải luôn ở trạng thái "Build & Run" được ngay sau khi clone.
- **README chất lượng**: Mỗi dự án phải có file `README.md` hướng dẫn Setup môi trường trong tối đa 5 phút.
