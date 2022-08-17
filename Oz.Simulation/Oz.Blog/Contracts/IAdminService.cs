namespace Oz.Blog.Contracts;

public interface IAdminService
{
    bool IsAdmin { get; }

    void SetIsAdmin(bool isAdmin);
}