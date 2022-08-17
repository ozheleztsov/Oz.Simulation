using Oz.Blog.Contracts;

namespace Oz.Blog.Services;

public class AdminService : IAdminService
{
    public bool IsAdmin { get; private set; }

    public void SetIsAdmin(bool isAdmin) =>
        IsAdmin = isAdmin;
}