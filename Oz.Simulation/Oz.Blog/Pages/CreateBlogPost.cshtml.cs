using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oz.Blog.ViewModels;
using Oz.Data.Contracts;
using Oz.Data.Entities;
using System.Text.Json;

namespace Oz.Blog.Pages;

public class CreateBlogPostModel : PageModel
{
    private readonly IBlogDataService _blogDataService;
    private readonly ILogger<CreateBlogPostModel> _logger;

    public CreateBlogPostModel(IBlogDataService blogDataService, ILogger<CreateBlogPostModel> logger)
    {
        _blogDataService = blogDataService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [BindProperty]
    public CreateBlogViewModel CreateBlogViewModel { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation("Received model: {Model}", JsonSerializer.Serialize(CreateBlogViewModel));
            var blogEntryCreateData = new BlogEntryCreateData(
                CreateBlogViewModel.Title,
                CreateBlogViewModel.Content,
                CreateBlogViewModel.AuthorSign,
                CreateBlogViewModel.Annotations);
            await _blogDataService.CreateBlogEntryAsync(blogEntryCreateData);
            return RedirectToPage("/Index");
        }

        return Page();
    }
}