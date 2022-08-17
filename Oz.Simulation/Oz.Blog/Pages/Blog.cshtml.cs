using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oz.Blog.Contracts;
using Oz.Blog.Entities;
using Oz.Data.Contracts;

namespace Oz.Blog.Pages;

public class BlogModel : PageModel
{
    public IAdminService AdminService { get; }
    private readonly IBlogDataService _dataService;
    private readonly ILogger<BlogModel> _logger;

    public BlogModel(IBlogDataService dataService, IAdminService adminService, ILogger<BlogModel> logger)
    {
        AdminService = adminService;
        _dataService = dataService;
        _logger = logger;
    }
    
    public BlogEntryResponseData? Blog { get; private set; }
    
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Blog = await _dataService.GetBlogEntryByIdAsync(id, cancellationToken);
        return Page();
    }
}