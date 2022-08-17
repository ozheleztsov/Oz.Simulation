using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oz.Blog.Contracts;
using Oz.Blog.Entities;
using Oz.Data.Contracts;
using System.Collections.Immutable;

namespace Oz.Blog.Pages;

public class IndexModel : PageModel
{
    public IAdminService AdminService { get; }
    private readonly IBlogDataService _blogDataService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IBlogDataService blogDataService, IAdminService adminService, ILogger<IndexModel> logger)
    {
        AdminService = adminService;
        _blogDataService = blogDataService;
        _logger = logger;
    }
    public ImmutableList<BlogEntryResponseData> Posts { get; private set; } = ImmutableList<BlogEntryResponseData>.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Posts = (await _blogDataService.GetBlogEntriesAsync(entry => true, cancellationToken))
            .OrderByDescending(x => x.ModifiedDate)
            .ToImmutableList();
        
        _logger.LogInformation("{Number} of posts received", Posts.Count);
        return Page();
    }
}