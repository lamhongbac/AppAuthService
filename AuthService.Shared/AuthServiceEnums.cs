using System.ComponentModel;

namespace AuthService.Shared
{
    public enum AuthServiceErrorCode
    {
        [Description("Thành công")]
        Success = 0,

        [Description("Lỗi hệ thống")]
        SystemError = 500,

        [Description("Dữ liệu không hợp lệ")]
        ValidationError = 400,

        // Domain: Company (100-199)
        [Description("Mã công ty đã tồn tại")]
        DuplicateCompanyCode = 101,

        [Description("Đăng ký công ty thất bại")]
        RegistrationFailed = 102,

        [Description("Không tìm thấy công ty")]
        CompanyNotFound = 103,

        // Domain: Application (200-299)
        [Description("Ứng dụng đã tồn tại")]
        DuplicateAppCode = 201,

        [Description("Không tìm thấy ứng dụng")]
        AppNotFound = 202,

        // Domain: User & Auth (300-399)
        [Description("Tên đăng nhập đã tồn tại")]
        DuplicateUsername = 301,

        [Description("Sai tên đăng nhập hoặc mật khẩu")]
        InvalidCredentials = 302,

        [Description("Tài khoản bị khóa")]
        UserLocked = 303,

        [Description("Token không hợp lệ hoặc hết hạn")]
        InvalidToken = 304
    }
}
