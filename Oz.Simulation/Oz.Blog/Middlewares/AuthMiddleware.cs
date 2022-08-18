using Oz.Blog.Contracts;

namespace Oz.Blog.Middlewares;

public class AuthMiddleware : IMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly IAdminService _adminService;
    private readonly ICookieService _cookieService;
    private readonly string _adminKey;
    private const string AdminKey = "adminKey";

    public AuthMiddleware(IConfiguration configuration, IAdminService adminService, ICookieService cookieService)
    {
        _configuration = configuration;
        _adminService = adminService;
        _cookieService = cookieService;
        _adminKey = configuration["AdminKey"];
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _adminService.SetIsAdmin(false);
        if (_cookieService.HasCookie(AdminKey))
        {
            var cookieValue = _cookieService.GetCookie(AdminKey);
            if (cookieValue == _adminKey)
            {
                _adminService.SetIsAdmin(true);
            }
        }

        if (!_adminService.IsAdmin)
        {
            foreach (var queryKey in context.Request.Query.Keys)
            {
                if (queryKey.Equals("key", StringComparison.OrdinalIgnoreCase))
                {
                    var actualValue = context.Request.Query[queryKey].First();
                    if (_adminKey == actualValue)
                    {
                        _cookieService.SetCookie(AdminKey, actualValue);
                        _adminService.SetIsAdmin(true);
                        break;
                    }
                }
            }
        }

        await next(context);
    }
}