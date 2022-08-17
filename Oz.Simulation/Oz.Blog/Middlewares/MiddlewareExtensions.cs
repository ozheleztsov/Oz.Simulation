namespace Oz.Blog.Middlewares;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<AuthMiddleware>();
}