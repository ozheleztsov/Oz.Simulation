using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oz.Blog.Contracts;
using Oz.Blog.Entities;
using Oz.Blog.ViewModels;
using Oz.Data.Contracts;

namespace Oz.Blog.Pages;

public class EditBlogPostModel : PageModel
{
    public IAdminService AdminService { get; }
    private readonly IBlogDataService _blogDataService;
    private readonly ILogger<EditBlogPostModel> _logger;

    public EditBlogPostModel(IBlogDataService blogDataService, IAdminService adminService,
        ILogger<EditBlogPostModel> logger)
    {
        AdminService = adminService;
        _blogDataService = blogDataService;
        _logger = logger;
    }
    
    [BindProperty]
    public UpdateBlogViewModel UpdateBlogViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var post  = await _blogDataService.GetBlogEntryByIdAsync(id, cancellationToken);
        if (post == null)
        {
            return RedirectToPage("Index");
        }
        
        UpdateBlogViewModel.Id = post.Id;
        UpdateBlogViewModel.Content = post.Content;
        UpdateBlogViewModel.Title = post.Title;
        UpdateBlogViewModel.AuthorSign = post.AuthorSign;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var sourceBlog = await _blogDataService.GetBlogEntryByIdAsync(UpdateBlogViewModel.Id, cancellationToken);
        if (sourceBlog == null)
        {
            ModelState.AddModelError(nameof(sourceBlog), $"Not found blog with id: {UpdateBlogViewModel.Id}");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var blogEntryUpdateData = new BlogEntryUpdateData(UpdateBlogViewModel.Title,
            UpdateBlogViewModel.Content,
            UpdateBlogViewModel.AuthorSign,
            sourceBlog!.Annotations);
        await _blogDataService.UpdateBlogEntryAsync(UpdateBlogViewModel.Id, blogEntryUpdateData, cancellationToken);
        return RedirectToPage("Blog", new { id = UpdateBlogViewModel.Id });
    }
}