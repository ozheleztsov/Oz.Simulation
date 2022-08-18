using System.ComponentModel.DataAnnotations;

namespace Oz.Blog.ViewModels;

public class UpdateBlogViewModel
{
    public Guid Id { get; set; }
    
    [Required]
    [Display(Name = "Post title")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Post content")]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Author sign")]
    [StringLength(256, MinimumLength = 5)]
    public string AuthorSign { get; set; } = string.Empty;
}