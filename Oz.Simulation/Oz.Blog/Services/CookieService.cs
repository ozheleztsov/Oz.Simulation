using Oz.Blog.Contracts;
using Oz.Blog.Exceptions;

namespace Oz.Blog.Services;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CookieService> _logger;

    public CookieService(IHttpContextAccessor httpContextAccessor, ILogger<CookieService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public bool HasCookie(string key)
    {
        var context = GetHttpContext();
        return context.Request.Cookies.ContainsKey(key);
    }

    public string GetCookie(string key)
    {
        var context = GetHttpContext();
        return context.Request.Cookies[key] ?? throw new BlogException($"No cookie with key {key}");
    }

    public void SetCookie(string key, string value)
    {
        var context = GetHttpContext();
        context.Response.Cookies.Append(key, value, new CookieOptions()
        {
            MaxAge = TimeSpan.FromHours(1)
        });
    }

    private HttpContext GetHttpContext()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new BlogException("HttpContext is null");
        }

        return context;
    }
}