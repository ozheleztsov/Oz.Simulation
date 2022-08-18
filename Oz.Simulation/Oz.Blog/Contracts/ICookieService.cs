namespace Oz.Blog.Contracts;

public interface ICookieService
{
    bool HasCookie(string key);
    string GetCookie(string key);
    void SetCookie(string key, string value);
}