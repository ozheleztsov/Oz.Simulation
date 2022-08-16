using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oz.Data.Contracts;
using Oz.Data.Entities;
using System.Collections.Immutable;

namespace Oz.Blog.Pages;

public class GetBlogPostsModel : PageModel
{
    private readonly IBlogDataService _blogDataService;
    private readonly ILogger<GetBlogPostsModel> _logger;

    public GetBlogPostsModel(IBlogDataService blogDataService, ILogger<GetBlogPostsModel> logger)
    {
        _blogDataService = blogDataService;
        _logger = logger;
    }
    public ImmutableList<BlogEntryResponseData> Posts { get; private set; } = ImmutableList<BlogEntryResponseData>.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Posts = (await _blogDataService.GetBlogEntriesAsync(entry => true, cancellationToken)).ToImmutableList();
        _logger.LogInformation("{Number} of posts received", Posts.Count);
        return Page();
    }
}