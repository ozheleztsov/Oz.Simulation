using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oz.Blog.Contracts;
using Oz.Blog.Entities;
using Oz.Blog.ViewModels;
using Oz.Data.Contracts;

namespace Oz.Blog.Pages;

public class AddRemoveAnnotationModel : PageModel
{
    public IAdminService AdminService { get; }
    private readonly IBlogDataService _blogDataService;
    private readonly ILogger<AddRemoveAnnotationModel> _logger;

    public AddRemoveAnnotationModel(IBlogDataService blogDataService, IAdminService adminService, ILogger<AddRemoveAnnotationModel> logger)
    {
        AdminService = adminService;
        _blogDataService = blogDataService;
        _logger = logger;
    }

    [BindProperty]
    public CreateAnnotationViewModel AnnotationViewModel { get; set; } = new();

    public BlogEntryResponseData? BlogEntry { get; private set; }


    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Post value of id: {Id}", AnnotationViewModel.BlogPostId);
        BlogEntry = await _blogDataService.GetBlogEntryByIdAsync(AnnotationViewModel.BlogPostId, cancellationToken);
        if (BlogEntry == null)
        {
            ModelState.AddModelError(nameof(AnnotationViewModel.BlogPostId), "Blog with such id is not found");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        List<BlogEntryAnnotation> newAnnotations = new();
        newAnnotations.AddRange(BlogEntry.Annotations);
        newAnnotations.Add(new BlogEntryAnnotation
        {
            Content = AnnotationViewModel.Content,
            CreateDate = DateTime.UtcNow,
            Id = Guid.NewGuid()
        });

        var updateData = new BlogEntryUpdateData(BlogEntry.Title,
            BlogEntry.Content, BlogEntry.AuthorSign, newAnnotations);
        await _blogDataService.UpdateBlogEntryAsync(BlogEntry.Id, updateData, cancellationToken);
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid blogId, Guid annotationId, CancellationToken cancellationToken)
    {
        var blog = await _blogDataService.GetBlogEntryByIdAsync(blogId, cancellationToken);
        if (blog == null)
        {
            ModelState.AddModelError(nameof(blogId), $"There is no blog with id: {blogId}");
            return Page();
        }

        var annotation = blog.Annotations.FirstOrDefault(x => x.Id == annotationId);
        if (annotation == null)
        {
            ModelState.AddModelError(nameof(annotationId), $"There is no annotation with id: {annotationId}");
            return Page();
        }

        blog.Annotations.Remove(annotation);
        var updateData = new BlogEntryUpdateData(blog.Title,
            blog.Content,
            blog.AuthorSign,
            blog.Annotations);
        await _blogDataService.UpdateBlogEntryAsync(blog.Id, updateData, cancellationToken);
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnGet(Guid id, CancellationToken cancellationToken)
    {
        AnnotationViewModel.BlogPostId = id;
        BlogEntry = await _blogDataService.GetBlogEntryByIdAsync(AnnotationViewModel.BlogPostId, cancellationToken);
        if (BlogEntry != null)
        {
            _logger.LogInformation("Got blog with id: {Id}", BlogEntry.Id);
        }
        else
        {
            _logger.LogInformation("Not found blog with id: {Id}", AnnotationViewModel.BlogPostId);
        }

        return Page();
    }
}