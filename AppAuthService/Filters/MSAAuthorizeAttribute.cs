using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AppAuth.BLL.Interfaces;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace AppAuthService.Filters
{
    /// <summary>
    /// Thuộc tính dùng để kiểm tra quyền hạn của người dùng trên một đối tượng cụ thể trong App.
    /// Cách dùng: [MSAAuthorize("USER_MANAGEMENT", "Create")]
    /// </summary>
    public class MSAAuthorizeAttribute : TypeFilterAttribute
    {
        public MSAAuthorizeAttribute(string appObject, string right) : base(typeof(MSAAuthorizeFilter))
        {
            Arguments = new object[] { appObject, right };
        }
    }

    public class MSAAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly string _appObject;
        private readonly string _right;
        private readonly IAuthService _authService;

        public MSAAuthorizeFilter(string appObject, string right, IAuthService authService)
        {
            _appObject = appObject;
            _right = right;
            _authService = authService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            // 1. Kiểm tra xem user đã authenticated chưa (qua JWT Middleware)
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 2. Lấy UserId và AppNumber từ Claims
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var appNumber = user.FindFirst("AppNumber")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(appNumber) || !int.TryParse(userIdStr, out int userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // 3. Gọi BLL để kiểm tra quyền trong CSDL
            bool isAuthorized = await _authService.AuthorizeAsync(userId, appNumber, _appObject, _right);

            if (!isAuthorized)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
