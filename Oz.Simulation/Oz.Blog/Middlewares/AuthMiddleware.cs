using Oz.Blog.Contracts;

namespace Oz.Blog.Middlewares;

public class AuthMiddleware : IMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly IAdminService _adminService;
    private readonly string _adminKey;

    public AuthMiddleware(IConfiguration configuration, IAdminService adminService)
    {
        _configuration = configuration;
        _adminService = adminService;
        _adminKey = configuration["AdminKey"];
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _adminService.SetIsAdmin(false);
        foreach (var queryKey in context.Request.Query.Keys)
        {
            if (queryKey.Equals("key", StringComparison.OrdinalIgnoreCase))
            {
                var actualValue = context.Request.Query[queryKey].First();
                if (_adminKey == actualValue)
                {
                    _adminService.SetIsAdmin(true);
                    break;
                }
            }
        }

        await next(context);
    }
}